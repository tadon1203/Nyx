using System.Collections.Generic;
using Nyx.SDK.Utils;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.SDK.Pickups;

public static class PickupManager
{
    private static List<VRC_Pickup> _pickups = new();
    private static List<NyxPickup> _pickupData = new();
    private static readonly object Lock = new();
    
    public static List<NyxPickup> GetPickupData()
    {
        lock (Lock)
        {
            return [.._pickupData];
        }
    }
    
    public static void UpdateObjectList()
    {
        _pickups = new List<VRC_Pickup>(Object.FindObjectsOfType<VRC_Pickup>());
    }

    public static void Update()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;
        
        Vector3 cameraPosition = camera.transform.position;
        var tempPickupData = new List<NyxPickup>();

        foreach (var pickup in _pickups)
        {
            if (pickup == null)
                continue;
            
            Vector3 position = pickup.transform.position;
            float distance = Vector3.Distance(cameraPosition, position);
            float sizeScalar = pickup.GetComponent<Collider>()?.bounds.size.magnitude ?? 0.5f;
            Vector3 boundSize = new Vector3(sizeScalar, sizeScalar, sizeScalar);
            Bounds bounds = new(position, boundSize);

            Vector3 screenPosRaw = camera.WorldToScreenPoint(position);
            Vector2 screenPos = ScreenPosUtils.GetScreenPositionSafe(screenPosRaw);
            
            var data = new NyxPickup
            {
                Name = $"{pickup.gameObject.name} | {pickup.InteractionText}",
                Distance = distance,
                IsVisible = screenPosRaw.z > 0,
                ScreenPosition = screenPos,
                BoxCorners = BoundsUtils.CalculateScreenCorners(camera, bounds),
            };
            tempPickupData.Add(data);
        }
        
        lock (Lock)
        {
            _pickupData = tempPickupData;
        }
    }
}