using System;
using System.Collections.Generic;
using Nyx.SDK.Core;
using Nyx.SDK.Utils;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace Nyx.SDK.Navigation;

public class NavMeshManager : BaseManager<NavMeshAgent, NavMeshAgentData>
{
    public override void FindObjects()
        => TrackedObjects = [..Object.FindObjectsOfType<NavMeshAgent>()];

    protected override NavMeshAgentData CreateObjectData(NavMeshAgent agent)
    {
        var position = agent.transform.position;
        var size = new Vector3(agent.radius * 2f, agent.height, agent.radius * 2f);
        var center = position + new Vector3(0, agent.height * 0.5f, 0);
        var bounds = new Bounds(center, size);
        var camera = Camera.main;

        return new()
        {
            Name = agent.gameObject.name,
            Distance = Vector3.Distance(camera.transform.position, position),
            ScreenPosition = ScreenUtils.WorldToScreenPoint(camera, position),
            BoxCorners = CalculateBoxCorners(camera, bounds),
            OriginalReference = new(agent)
        };
    }
}