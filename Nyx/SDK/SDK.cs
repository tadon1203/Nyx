using UnityEngine;
using VRC.SDKBase;

namespace Nyx.SDK;

public static class SDK
{
    private const float UpdateInterval = 1.0f;
    private static float _timeSinceLastUpdate = 0.0f;
    
    public static void Update()
    {
        if (Networking.LocalPlayer == null)
            return;
        
        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate >= UpdateInterval)
        {
            Navigation.NavMeshManager.UpdateObjectList();
            Pickups.PickupManager.UpdateObjectList();
            _timeSinceLastUpdate = 0.0f;
        }
        Players.PlayerManager.Update();
        Navigation.NavMeshManager.Update();
        Pickups.PickupManager.Update();
    }
}