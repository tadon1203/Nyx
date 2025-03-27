using Nyx.SDK.Navigation;
using Nyx.SDK.Pickups;
using Nyx.SDK.Players;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.SDK;

public static class SDK
{
    private const float ObjectFindInterval = 5.0f;
    private static float _lastObjectFindTime;
    
    private const float PlayerFindInterval = 0.1f;
    private static float _lastPlayerFindTime;
    
    public static PlayerManager Players { get; } = new();
    public static PickupManager Pickups { get; } = new();
    public static NavMeshManager NavMeshAgents { get; } = new();

    public static void Update()
    {
        if (Networking.LocalPlayer == null) 
            return;
        
        var currentTime = Time.time;
        
        if (currentTime - _lastObjectFindTime > ObjectFindInterval)
        {
            Pickups.FindObjects();
            NavMeshAgents.FindObjects();
            
            _lastObjectFindTime = currentTime;
        }

        if (currentTime - _lastPlayerFindTime > PlayerFindInterval)
        {
            Players.FindObjects();
            
            _lastPlayerFindTime = currentTime;
        }
        
        Players.UpdateObjects();
        Pickups.UpdateObjects();
        NavMeshAgents.UpdateObjects();
    }
}