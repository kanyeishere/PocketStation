using System.Collections;
using System.Reflection;
using Dalamud.Plugin;
using OmenTools.Dalamud.Helpers;
using OmenTools.Extensions;

namespace PocketStation.Core;

public static class PluginStateManager
{
    public static async Task<string?> SetPluginStateAsync(string internalName, bool enable)
    {
        try
        {
            var pm = DalamudReflector.GetPluginManager();
            if (pm == null) return "PluginManager not available.";

            var installedPlugins = (IList)pm.GetType()
                .GetProperty("InstalledPlugins")?.GetValue(pm);
            if (installedPlugins == null) return "InstalledPlugins not available.";

            foreach (var plugin in installedPlugins)
            {
                var type = plugin.GetType().Name == "LocalDevPlugin"
                    ? plugin.GetType().BaseType
                    : plugin.GetType();
                if (type == null) continue;

                var name = (string?)type.GetProperty("InternalName")?.GetValue(plugin);
                if (!string.Equals(name, internalName, StringComparison.OrdinalIgnoreCase)) continue;

                var isLoaded = (bool?)type.GetProperty("IsLoaded")?.GetValue(plugin) ?? false;

                if (enable && isLoaded) return null;
                if (!enable && !isLoaded) return null;

                if (enable)
                {
                    var loadTask = pm.Call<Task>("LoadPluginAsync", [plugin, PluginLoadReason.Installer]);
                    if (loadTask != null)
                        await loadTask.ConfigureAwait(false);
                    else
                        return "LoadPluginAsync not available.";
                }
                else
                {
                    var unloadTask = pm.Call<Task>("UnloadPluginAsync", [plugin]);
                    if (unloadTask != null)
                        await unloadTask.ConfigureAwait(false);
                    else
                        return "UnloadPluginAsync not available.";
                }

                return null;
            }

            return $"Plugin '{internalName}' not found.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
