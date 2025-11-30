using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RakipBul.ViewModels;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using RakipBul.Data;
using Microsoft.EntityFrameworkCore;
using RakipBul.Models;

namespace RakipBul.Managers
{
    public class NotificationManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly OpenAiManager _aiManager;
        private readonly ApplicationDbContext _context;


        public NotificationManager(IConfiguration configuration, ILogger<NotificationManager> logger, OpenAiManager aiManager, ApplicationDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _aiManager = aiManager;

            if (FirebaseApp.DefaultInstance == null)
            {
                // Dosya yolu: appsettings.json'dan veya sabit olarak belirtebilirsin
                var firebaseJsonPath = Path.Combine(AppContext.BaseDirectory, "firebase_secret.json");
                if (!File.Exists(firebaseJsonPath))
                    throw new FileNotFoundException("Firebase config dosyası bulunamadı.", firebaseJsonPath);

                var json = File.ReadAllText(firebaseJsonPath);
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(json)
                });
            }

            _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task<(bool success, string message)> SendNotificationToAllUsers(NotificationViewModel model, string topicnoculture)
        {
            try
            {
                // Languages: tr (original), ru, ro, en
                var languages = new List<(string langCode, string displayName)>
                {
                    ("tr", "Turkish"), 
                    ("en", "English")
                };

                var responses = new List<string>();

                foreach (var (langCode, displayName) in languages)
                {
                    string title = model.TitleTr;
                    string body = model.MessageTr;

                    if (langCode != "tr")
                    {
                        // Translate title and body from Turkish to target language
                        title = await _aiManager.TranslateFromTurkishAsync(model.TitleTr ?? string.Empty, displayName);
                        body = await _aiManager.TranslateFromTurkishAsync(model.MessageTr ?? string.Empty, displayName);
                    }

                    var topic = $"{topicnoculture}_{langCode}";

                    var message = new Message()
                    {
                        Notification = new Notification()
                        {
                            Title = title,
                            Body = body
                        },
                        Topic = topic
                    };

                    var resp = await _firebaseMessaging.SendAsync(message);
                    responses.Add($"{topic}:{resp}");
                }

                return (true, string.Join("; ", responses));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gönderilirken hata oluştu");
                return (false, "Bildirim gönderilirken bir hata oluştu.");
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

        /// <summary>
        /// Firebase'de bir topic (grup) oluşturur. Firebase'de topic'ler otomatik oluşur, bu metot sadece isim kontrolü için kullanılabilir.
        /// </summary>
        public Task<(bool success, string message)> CreateGroup(string groupName)
        {
            // Firebase'de topic'ler otomatik olarak ilk abone ile oluşur, ekstra bir API çağrısı gerekmez.
            // Ancak uygulama tarafında grup ismini kaydetmek isterseniz burada ek işlemler yapabilirsiniz.
            if (string.IsNullOrWhiteSpace(groupName))
                return Task.FromResult((false, "Grup adı boş olamaz."));

            // Gerekirse burada veritabanına da kaydedebilirsiniz.
            return Task.FromResult((true, $"Grup '{groupName}' oluşturulmaya hazır (Firebase topic olarak)."));
        }

        public async Task<(bool success, string message)> SendNotificationToGroup(string groupName, NotificationViewModel model)
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
                    Topic = groupName
                };

                var response = await _firebaseMessaging.SendAsync(message);
                return (true, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gruba özel bildirim gönderilirken hata oluştu");
                return (false, "Gruba özel bildirim gönderilirken bir hata oluştu.");
            }
        }

        /// <summary>
        /// Bir veya birden fazla cihazı (token) belirtilen topic'e abone eder.
        /// </summary>
        public async Task<(bool success, string message)> SubscribeToTopicAsync(
      string token,
      string topic,
      string platform = "Unknown")
        {
            try
            {
                // 1) Bu token + topic kaydı var mı?
                var exists = await _context.DeviceTokens
                    .AnyAsync(x => x.Token == token && x.Topic == topic);

                if (exists)
                    return (true, "Zaten bu topic'e abone.");

                // 2) Firebase subscribe
                var response = await _firebaseMessaging
                    .SubscribeToTopicAsync(new[] { token }, topic);

                if (response.FailureCount > 0)
                {
                    var error = response.Errors.First().Reason;
                    return (false, $"Firebase subscribe başarısız: {error}");
                }

                // 3) DB’ye yeni kayıt ekle
                _context.DeviceTokens.Add(new DeviceToken
                {
                    Token = token,
                    Topic = topic,
                    Platform = platform,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return (true, "Topic'e abone edildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscribe hatası");
                return (false, "Topic'e abone edilirken bir hata oluştu.");
            }
        }


        /// <summary>
        /// Bir veya birden fazla cihazı (token) belirtilen topic'ten çıkarır.
        /// </summary>
        public async Task<(bool success, string message)> UnsubscribeFromTopicAsync(
    string token,
    string topic)
        {
            try
            {
                // 1) Token-topic kaydını bul
                var record = await _context.DeviceTokens
                    .FirstOrDefaultAsync(x => x.Token == token && x.Topic == topic);

                if (record == null)
                    return (true, "Zaten bu topic'te kayıt yok.");

                // 2) Firebase unsubscribe
                var response = await _firebaseMessaging
                    .UnsubscribeFromTopicAsync(new[] { token }, topic);

                if (response.FailureCount > 0)
                {
                    var error = response.Errors.First().Reason;
                    return (false, $"Firebase unsubscribe başarısız: {error}");
                }

                // 3) DB kaydını sil
                _context.DeviceTokens.Remove(record);
                await _context.SaveChangesAsync();

                return (true, "Topic'ten çıkarıldı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unsubscribe hatası");
                return (false, "Topic'ten çıkarılırken bir hata oluştu.");
            }
        }

    }
}