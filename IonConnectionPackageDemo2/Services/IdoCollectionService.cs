using InforIonClientLibrary.src;
using IonConnectionPackageDemo2.Models;
using InforIonClientLibrary;
using System.Text.Json;

namespace IonConnectionPackageDemo2.Services;
public class IdoCollectionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdoCollectionService> _logger;
    private readonly ITokenService _tokenService;
    private readonly string _baseUri;
    private readonly string _mongooseConfig;

    public IdoCollectionService(HttpClient httpClient, ILogger<IdoCollectionService> logger, ITokenService tokenService, ConnectionOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _baseUri = $"{options.Iu}/{options.Ti}/MONGOOSE/IDORequestService";
        _mongooseConfig = options.Ti ?? throw new ArgumentNullException(nameof(options.Ti));
    }

    public async Task<List<string>> GetConfigurationsAsync(string? configGroup = null)
    {
        configGroup ??= _mongooseConfig;

        var tokenResponse = await _tokenService.GetTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
        _httpClient.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", _mongooseConfig);

        var url = $"{_baseUri}/ido/configurations";
        if (!string.IsNullOrEmpty(configGroup))
        {
            url += $"?configGroup={configGroup}";
        }

        _logger.LogInformation("Calling configurations endpoint with configGroup: {ConfigGroup}, URL: {Url}", configGroup, url);

        try
        {
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var configResponse = JsonSerializer.Deserialize<ConfigurationNames>(json);

            if (configResponse?.Configurations == null || configResponse.Configurations.Count == 0)
            {
                _logger.LogWarning("Configuration response was null or empty.");
                return new List<string>();
            }

            _logger.LogInformation("Successfully retrieved configurations.");

            if (configResponse.Configurations.Count == 1)
            {
                _logger.LogInformation("Only one configuration found, selecting it by default: {Configuration}", configResponse.Configurations[0]);
                return configResponse.Configurations;
            }

            _logger.LogInformation("Multiple configurations found, selecting the first one by default: {Configuration}", configResponse.Configurations[0]);

            return configResponse.Configurations;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Error calling configurations endpoint: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<List<string>> GetAvailableCollectionsAsync()
    {
        var tokenResponse = await _tokenService.GetTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
        _httpClient.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", _mongooseConfig);

        var url = $"{_baseUri}/collections";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var collectionsResponse = JsonSerializer.Deserialize<List<string>>(json);

        return collectionsResponse ?? new List<string>();
    }

    public async Task<LoadCollectionResponse> LoadIdoCollectionAsync(
    string idoName,
    string properties,
    string configuration,
    string filter = null,
    string orderBy = null,
    int? recordCap = null)
    {
        var tokenResponse = await _tokenService.GetTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
        _httpClient.DefaultRequestHeaders.Remove("X-Infor-MongooseConfig");
        _httpClient.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", configuration);

        var url = $"{_baseUri}/ido/load/{idoName}?properties={properties}";
        if (!string.IsNullOrEmpty(filter)) url += $"&filter={filter}";
        if (!string.IsNullOrEmpty(orderBy)) url += $"&orderBy={orderBy}";
        if (recordCap.HasValue) url += $"&recordCap={recordCap}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoadCollectionResponse>(json) ?? new LoadCollectionResponse();
    }
}
