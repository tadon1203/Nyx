using ExitGames.Client.Photon;
using HarmonyLib;

namespace Nyx.Patching;

public class NetworkManagerPatch
{
    private static NetworkManager_ instance;
    
    [HarmonyPatch(typeof(NetworkManager_), nameof(NetworkManager_.Method_Public_Virtual_Final_New_Void_EventData_0))]
    [HarmonyPrefix]
    public static void Prefix(NetworkManager_ __instance, EventData param_1)
    {
        if (instance == null)
        {
            instance = __instance;
        }
    }

    public static NetworkManager_ GetInstance()
    {
        return instance;
    }
}