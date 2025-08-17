using System.Text;
using System.Text.Json;
using Bot.Application.Shared;
using Bot.Contracts.ChatAi.OpenRouter;

namespace Bot.Application.ChatAi.OpenRouter;

internal class ChatAiOpenRouterClient
{
    private static readonly Uri BaseUri = new("https://openrouter.ai/");
    private const string Endpoint = "api/v1/chat/completions";
    
    private readonly HttpClient _httpClient;

    public ChatAiOpenRouterClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.BaseAddress ??= BaseUri;
    }

    public async Task<ModelResponse?> PostAsync(ModelRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string json = JsonSerializer.Serialize(request, JsonOptionsProvider.Default);

        using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(Endpoint, httpContent, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new InvalidOperationException("Request error", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            
            throw new HttpRequestException(
                $"Unsuccessful request to OpenRouter API: {(int)response.StatusCode} {response.ReasonPhrase}. " +
                $"Response: {errorContent}");
        }

        string responseContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        try
        {
            return JsonSerializer.Deserialize<ModelResponse>(responseContent, JsonOptionsProvider.Default);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Deserialize response error", ex);
        }
    }
}