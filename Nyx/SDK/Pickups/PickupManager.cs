using System;
using System.Linq;
using Nyx.SDK.Core;
using Nyx.SDK.Utils;
using UnityEngine;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace Nyx.SDK.Pickups;

public class PickupManager : BaseManager<VRC_Pickup, PickupData>
{
    public override void FindObjects() 
        => TrackedObjects = Object.FindObjectsOfType<VRC_Pickup>().ToList();

    protected override PickupData CreateObjectData(VRC_Pickup pickup)
    {
        var position = pickup.transform.position;
        var collider = pickup.GetComponent<Collider>();
        var bounds = collider?.bounds ?? new Bounds(position, Vector3.one * 0.5f);
        var camera = Camera.main;

        return new()
        {
            Name = pickup.gameObject.name,
            Distance = Vector3.Distance(camera.transform.position, position),
            ScreenPosition = ScreenUtils.WorldToScreenPoint(camera, position),
            BoxCorners = CalculateBoxCorners(camera, bounds),
            InteractionText = pickup.InteractionText,
            OriginalReference = new(pickup),
        };
    }
}