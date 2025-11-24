using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public class RakipbulApiManager
{
    private readonly RakipbulApiSettings _settings;

    public RakipbulApiManager(IOptions<RakipbulApiSettings> options)
    {
        _settings = options.Value;
    }

   public async Task<RakipbulAuthResponse> GetAuthTokenAsync()
{
    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var payload = new
        {
            username = _settings.Username,
            password = _settings.Password
        };

        var jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(_settings.TokenEndpoint, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        // 🔹 JSON'u DTO'ya deserialize et
        var result = JsonConvert.DeserializeObject<RakipbulAuthResponse>(json);

        return result;
    }
}
public async Task<List<RakipbulLeagueDto>> GetLeaguesAsync()
{
    using (var client = new HttpClient())
    {
        // 1) Token al
        var tokenResult = await GetAuthTokenAsync();
        var accessToken = tokenResult.Access;

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var leaguesUrl = $"{_settings.Endpoint}leagues/"; 
        // NOT: Eğer EndpointBase yoksa appsettings’e ekleyeceğiz.

        var response = await client.GetAsync(leaguesUrl);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        // 2) Mapping
        var leagues = JsonConvert.DeserializeObject<List<RakipbulLeagueDto>>(json);

        return leagues;
    }
}

public async Task<List<RakipbulSeasonDto>> GetLeagueSeasonsAsync(int leagueId)
{
    using (var client = new HttpClient())
    {
        // 1) Token al
        var tokenResult = await GetAuthTokenAsync();
        var accessToken = tokenResult.Access;

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var seasonsUrl = $"{_settings.Endpoint}leagues/{leagueId}/seasons/";
        var response = await client.GetAsync(seasonsUrl);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var seasons = JsonConvert.DeserializeObject<List<RakipbulSeasonDto>>(json) ?? new List<RakipbulSeasonDto>();
        return seasons;
    }
}

}
