using Nyx.SDK.Navigation;
using Nyx.SDK.Pickups;
using Nyx.SDK.Players;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.SDK;

public static class SDK
{
    private const float UpdateInterval = 1.0f;
    private static float _timeSinceLastUpdate;
    
    public static void Update()
    {
        if (Networking.LocalPlayer == null)
            return;
        
        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate >= UpdateInterval)
        {
            NavMeshManager.UpdateObjectList();
            PickupManager.UpdateObjectList();
            _timeSinceLastUpdate = 0.0f;
        }
        PlayerManager.Update();
        NavMeshManager.Update();
        PickupManager.Update();
    }
}