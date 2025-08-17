using System.Text.Encodings.Web;
using System.Text.Json;

namespace Bot.Application.Shared;

internal static class JsonOptionsProvider
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };
}