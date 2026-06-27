using OmenTools.OmenService;
using FFXIVClientStructs.FFXIV.Client.Game;
using PocketStation.Domain;

namespace PocketStation.Infrastructure.Game;

public static class CurrencyHelper
{
    private static readonly (uint ItemId, string Name, string IconId, bool IsWeeklyLimited)[] KnownCurrencies =
    [
        (1,     "金币",         "gil",      false),
        (28,    "诗学神典石",   "poetics",  false),
        (48,    "数理神典石",   "math",     false),
        (49,    "记忆神典石",   "mnemo",    true),
        (26807, "双色宝石",     "bicolor",  false)
    ];

    public static IReadOnlyList<CurrencyInfo> Capture()
    {
        var result = new List<CurrencyInfo>();
        var limitedProgress = CaptureLimitedTomestoneProgress();

        foreach (var (itemId, name, iconId, isWeeklyLimited) in KnownCurrencies)
        {
            var count = LocalPlayerState.GetItemCount(itemId);
            result.Add(new CurrencyInfo(
                itemId,
                name,
                count,
                iconId,
                isWeeklyLimited ? limitedProgress.WeeklyAcquired : null,
                isWeeklyLimited ? limitedProgress.WeeklyLimit : null));
        }

        return result;
    }

    private unsafe static (uint? WeeklyAcquired, uint? WeeklyLimit) CaptureLimitedTomestoneProgress()
    {
        try
        {
            var inventoryManager = InventoryManager.Instance();
            if (inventoryManager == null)
                return (null, null);

            var acquired = Math.Max(0, inventoryManager->GetWeeklyAcquiredTomestoneCount());
            var limit = Math.Max(0, InventoryManager.GetLimitedTomestoneWeeklyLimit());
            return ((uint)acquired, (uint)limit);
        }
        catch (Exception ex)
        {
            Plugin.Log.Debug(ex, "Failed to capture limited tomestone weekly progress");
            return (null, null);
        }
    }
}
