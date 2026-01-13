using DocumentFormat.OpenXml.Wordprocessing;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Rakipbul.Models;
using Rakipbul.Models.Dtos;
using RakipBul.Data; 

namespace RakipBul.Managers
{
    public class NotificationManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly ApplicationDbContext _context;
        private readonly OpenAiManager _aiManager;



        public NotificationManager(IConfiguration configuration, ILogger<NotificationManager> logger, ApplicationDbContext context, OpenAiManager aiManager)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;            
            _aiManager = aiManager;


            if (FirebaseApp.DefaultInstance == null)
            {
                try
                {
                    // 1) Firebase config'i appsettings'ten string olarak al
                    var firebaseSection = _configuration.GetSection("Firebase");
                    var firebaseJson = firebaseSection.Get<FirebaseConfigModel>();

                    if (firebaseJson == null)
                        throw new Exception("Firebase config appsettings içinde bulunamadı.");

                    // 2) FirebaseConfigModel → JSON string'e çevir
                    string json = System.Text.Json.JsonSerializer.Serialize(firebaseJson);

                    // 3) Firebase Admin App create
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(json)
                    });

                    _logger.LogInformation("Firebase başarıyla initialize edildi.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Firebase initialize edilirken hata oluştu.");
                    throw;
                }
            }

            _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task<(bool success, string message)> SendNotificationToAllUsersBatch(NotificationViewModel model)
        {
            try
            {
                // 1) Tüm token + culture bilgilerini çek
                var tokens = await _context.UserDeviceToken
                    .Select(x => new { x.Token, x.Culture })
                    .ToListAsync();

                if (!tokens.Any())
                    return (false, "Gönderilecek cihaz bulunamadı.");

                // 2) TR ve EN olarak grupla
                var trTokens = tokens
                    .Where(x => x.Culture == "tr")
                    .Select(x => x.Token)
                    .ToList();

                var enTokens = tokens
                    .Where(x => x.Culture == "en")
                    .Select(x => x.Token)
                    .ToList();

                // 3) Mesajları hazırla
                string titleTR = model.TitleTr ?? "";
                string bodyTR = model.MessageTr ?? "";

                string titleEN = await _aiManager.TranslateFromTurkishAsync(titleTR, "English");
                string bodyEN = await _aiManager.TranslateFromTurkishAsync(bodyTR, "English");

                int totalSent = 0;

                // 4) TR kullanıcılara batch push
                totalSent += await SendBatchAsync(trTokens, titleTR, bodyTR);

                // 5) EN kullanıcılara batch push
                totalSent += await SendBatchAsync(enTokens, titleEN, bodyEN);

                return (true, $"{totalSent} cihaza bildirim gönderildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplu bildirim gönderilirken hata oluştu");
                return (false, "Toplu bildirim gönderilirken bir hata oluştu.");
            }
        }

        public async Task<(bool success, string message)> SendNotificationToUser(string deviceToken, NotificationViewModel model)
        {
            try
            {
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = model.TitleTr,
                        Body = model.MessageTr
                    },
                    Token = deviceToken
                };

                var response = await _firebaseMessaging.SendAsync(message);
                return (true, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kişiye özel bildirim gönderilirken hata oluştu");
                return (false, "Kişiye özel bildirim gönderilirken bir hata oluştu.");
            }
        }
         
        public async Task<(bool success, string message)> SubscribeToTopicAsync(string token,string topic,string platform = "Unknown")
        {
            try
            {
                // 1) DB’de zaten var mı?
                var exists = await _context.DeviceTopicSubscriptions
                    .AnyAsync(x => x.Token == token && x.Topic == topic);

                if (exists)
                    return (true, $"Zaten bu topic'e abone: {topic}");

                // 2) Firebase subscribe
                var response = await _firebaseMessaging
                    .SubscribeToTopicAsync(new[] { token }, topic);

                if (response.FailureCount > 0)
                {
                    var error = response.Errors.First().Reason;
                    return (false, $"Firebase subscribe başarısız: {error}");
                }

                // 3) DB’ye 1 satır ekle
                _context.DeviceTopicSubscriptions.Add(new DeviceTopicSubscription
                {
                    Token = token,
                    Topic = topic,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return (true, $"Topic'e abone edildi: {topic}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscribe hatası");
                return (false, $"Topic'e abone edilirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> SendNotificationToGroupBatch(NotificationViewModel model, string topic)
        {
            try
            {
                // 1) Bu topic'e abone olan tokenları al
                var subscribedTokens = await _context.DeviceTopicSubscriptions
                    .Where(x => x.Topic == topic)
                    .Select(x => x.Token)
                    .ToListAsync();

                if (!subscribedTokens.Any())
                    return (false, $"Bu topic için kayıtlı cihaz yok: {topic}");

                // 2) Token + Culture JOIN
                var tokenCultures = await _context.UserDeviceToken
                    .Where(x => subscribedTokens.Contains(x.Token))
                    .Select(x => new { x.Token, x.Culture })
                    .ToListAsync();

                if (!tokenCultures.Any())
                    return (false, $"Kullanıcı kültür bilgisi bulunamadı: {topic}");

                // 3) TR / EN olarak ayır
                var trTokens = tokenCultures
                    .Where(x => x.Culture == "tr")
                    .Select(x => x.Token)
                    .ToList();

                var enTokens = tokenCultures
                    .Where(x => x.Culture == "en")
                    .Select(x => x.Token)
                    .ToList();

                if (!trTokens.Any() && !enTokens.Any())
                    return (false, "Gönderilecek cihaz bulunamadı.");

                // 4) Mesajları hazırla
                string titleTR = model.TitleTr ?? "";
                string bodyTR = model.MessageTr ?? "";

                string titleEN = await _aiManager.TranslateFromTurkishAsync(titleTR, "English");
                string bodyEN = await _aiManager.TranslateFromTurkishAsync(bodyTR, "English");

                int totalSent = 0;

                // 5) TR kullanıcılara gönder
                if (trTokens.Any())
                    totalSent += await SendBatchAsync(trTokens, titleTR, bodyTR);

                // 6) EN kullanıcılara gönder
                if (enTokens.Any())
                    totalSent += await SendBatchAsync(enTokens, titleEN, bodyEN);

                return (true, $"{topic} için toplam {totalSent} cihaz bildirimi aldı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Grup bildirim gönderilirken hata oluştu ({topic})");
                return (false, "Grup bildirim gönderilirken bir hata oluştu.");
            }
        }


        public async Task<(bool success, string message)> UnsubscribeFromTopicAsync(string token,string topic)
        {
            try
            {
                // 1) DB kaydı var mı?
                var record = await _context.DeviceTopicSubscriptions
                    .FirstOrDefaultAsync(x => x.Token == token && x.Topic == topic);

                if (record == null)
                    return (true, $"Zaten kayıt yok: {topic}");

                // 2) Firebase unsubscribe
                var response = await _firebaseMessaging.UnsubscribeFromTopicAsync(new[] { token }, topic);

                if (response.FailureCount > 0)
                {
                    var error = response.Errors.First().Reason;
                    return (false, $"Firebase unsubscribe başarısız: {error}");
                }

                // 3) DB’den sil
                _context.DeviceTopicSubscriptions.Remove(record);
                await _context.SaveChangesAsync();

                return (true, $"Topic'ten çıkarıldı: {topic}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unsubscribe hatası");
                return (false, $"Topic'ten çıkarılırken hata oluştu: {ex.Message}");
            }
        }

        private async Task<int> SendBatchAsync(List<string> tokens, string title, string body)
        {
            int sentCount = 0;

            const int batchSize = 300;

            for (int i = 0; i < tokens.Count; i += batchSize)
            {
                var chunk = tokens
                    .Skip(i)
                    .Take(batchSize)
                    .ToList();

                if (!chunk.Any())
                    continue;

                var message = new MulticastMessage()
                {
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Tokens = chunk
                };

                var response = await _firebaseMessaging.SendEachForMulticastAsync(message);

                sentCount += response.SuccessCount;

                // İstersen log:
                _logger.LogInformation($"Batch gönderildi. Başarılı: {response.SuccessCount}, Başarısız: {response.FailureCount}");
            }

            return sentCount;
        }


    }
}