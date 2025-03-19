using System;
using HarmonyLib;
using Nyx.Core;
using VRC.Core;

namespace Nyx.Patching
{
    [HarmonyPatch(typeof(ObjectPublicStBoStDaBo1StILBo1Unique), nameof(ObjectPublicStBoStDaBo1StILBo1Unique.Method_Public_Static_ApiVRChatSubscription_0))]
    class FakeVRCPlusPatch
    {
        static void Postfix(ref ApiVRChatSubscription __result)
        {
            if (__result != null)
                return;

            var result = new ApiVRChatSubscription();

            var yesterday = DateTime.UtcNow.AddDays(-1.0);

            result._transactionId_k__BackingField = "txn_" + Guid.NewGuid();
            result._steamItemId_k__BackingField = "4000";
            result._amount_k__BackingField = 999;
            result._description_k__BackingField = "VRChat+ (Monthly)";
            result._store_k__BackingField = "Steam";
            result._period_k__BackingField = "month";
            result._active_k__BackingField = true;
            result._status_k__BackingField = "active";
            result._tier_k__BackingField = 5;
            result._starts_k__BackingField = "";
            result._expires_k__BackingField = yesterday.AddMonths(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            result._created_at_k__BackingField = yesterday.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            result._updated_at_k__BackingField = yesterday.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            result._isGift_k__BackingField = false;
            result._isBulkGift_k__BackingField = false;
            result._giftedBy_k__BackingField = "";
            result._giftedByDisplayName_k__BackingField = "";
            result._licenseGroups_k__BackingField = new Il2CppSystem.Collections.Generic.List<string>();
                
            result.transactionId = "txn_" + Guid.NewGuid();
            result.steamItemId = "4000";
            result.amount = 999;
            result.description = "VRChat+ (Monthly)";
            result.store = "Steam";
            result.period = "month";
            result.active = true;
            result.status = "active";
            result.tier = 5;
            result.starts = "";
            result.expires = yesterday.AddMonths(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            result.created_at = yesterday.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            result.updated_at = yesterday.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            result.isGift = false;
            result.isBulkGift = false;
            result.giftedBy = "";
            result.giftedByDisplayName = "";
            result.licenseGroups = new Il2CppSystem.Collections.Generic.List<string>();

            __result = result;
        }
    }
}
