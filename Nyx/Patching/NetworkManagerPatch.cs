using ExitGames.Client.Photon;
using HarmonyLib;

namespace Nyx.Patching;

[HarmonyPatch(typeof(NetworkManager_), nameof(NetworkManager_.Method_Public_Virtual_Final_New_Void_EventData_0))]
class NetworkManagerPatch
{
    private static NetworkManager_ _instance;

    static void Prefix(NetworkManager_ __instance, EventData param_1)
    {
        if (_instance == null)
        {
            _instance = __instance;
        }
    }

    public static NetworkManager_ GetInstance()
    {
        return _instance;
    }
}