using System.Net.Sockets;
using System.Text.Json;
using PocketStation.Domain;
using PocketStation.Host;
using PocketStation.Infrastructure.Network;
using PocketStation.Infrastructure.Serialization;

namespace PocketStation.Api.Controllers;

public sealed class SettingsController : IHttpController
{
    private static readonly HashSet<string> AllowedThemes = new(StringComparer.Ordinal)
    {
        "phantom",
        "spotlight",
        "velvet",
        "neon",
        "afterlife",
        "braindance"
    };

    private readonly Configuration configuration;
    private readonly Action saveConfiguration;

    public SettingsController(Configuration configuration, Action saveConfiguration)
    {
        this.configuration = configuration;
        this.saveConfiguration = saveConfiguration;
    }

    public async Task<bool> TryHandleAsync(NetworkStream stream, HttpRequest request, CancellationToken ct)
    {
        if (request.Method == "GET" && request.Path == "/api/settings/theme")
        {
            await HttpHelpers.WriteJsonAsync(stream, new { theme = NormalizeTheme(configuration.UiTheme) }, ct)
                .ConfigureAwait(false);
            return true;
        }

        if (request.Method == "POST" && request.Path == "/api/settings/theme")
        {
            try
            {
                var payload = JsonSerializer.Deserialize<ThemeSettingsRequest>(request.Body, PocketJson.Options);
                var theme = NormalizeTheme(payload?.Theme);
                configuration.UiTheme = theme;
                saveConfiguration();

                await HttpHelpers.WriteJsonAsync(stream,
                    new CommandResult(true, "theme saved", new { theme }), ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex, "Failed to save UI theme");
                await HttpHelpers.WriteResponseAsync(stream, 500, "application/json",
                    HttpHelpers.JsonBytes(new CommandResult(false, ex.Message)), ct).ConfigureAwait(false);
            }

            return true;
        }

        return false;
    }

    private static string NormalizeTheme(string? theme) =>
        theme != null && AllowedThemes.Contains(theme) ? theme : "phantom";

    private sealed record ThemeSettingsRequest(string Theme);
}
