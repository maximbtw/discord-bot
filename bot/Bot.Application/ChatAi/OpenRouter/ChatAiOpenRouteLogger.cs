using Bot.Contracts.ChatAi.OpenRouter;
using Microsoft.Extensions.Logging;

namespace Bot.Application.ChatAi.OpenRouter;

public class ChatAiOpenRouteLogger
{
    private readonly ILogger _logger;

    public ChatAiOpenRouteLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void Log(TimeSpan duration, ModelRequest request, ModelResponse? response, Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogError(
                exception,
                "[DURATION] {Duration}ms {@Request} [REQUEST]",
                duration.TotalMilliseconds,
                request);
        }
        else
        {
            _logger.LogInformation(
                "[DURATION] {Duration}ms [REQUEST] {@Request} [RESPONSE] {@Response}",
                duration.TotalMilliseconds,
                request,
                response);
        }
    }
}