using System.Collections.Generic;
using Nyx.SDK.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Nyx.SDK.Navigation;

public static class NavMeshManager
{
    private static List<NavMeshAgent> _navMeshAgents = new();
    private static List<NyxNavMeshAgent> _navMeshAgentData = new();
    private static readonly object Lock = new();
    
    public static List<NyxNavMeshAgent> GetAgentData()
    {
        lock (Lock)
        {
            return [.._navMeshAgentData];
        }
    }
    
    public static void UpdateObjectList()
    {
        _navMeshAgents = new List<NavMeshAgent>(Object.FindObjectsOfType<NavMeshAgent>());
    }

    public static void Update()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;
        
        Vector3 cameraPosition = camera.transform.position;
        var tempNavMeshData = new List<NyxNavMeshAgent>();

        foreach (var agent in _navMeshAgents)
        {
            if (agent == null)
                continue;
            
            Vector3 position = agent.transform.position;
            float distance = Vector3.Distance(cameraPosition, position);
            float height = agent.height;
            float radius = agent.radius;
            Vector3 size = new Vector3(radius * 2f, height, radius * 2f);
            Vector3 center = position + new Vector3(0, height * 0.5f, 0);
            Bounds bounds = new(center, size);
            
            Vector3 screenPosRaw = camera.WorldToScreenPoint(position);
            SysVec2 screenPos = ScreenPosUtils.GetScreenPositionSafe(screenPosRaw);

            var data = new NyxNavMeshAgent
            {
                Name = agent.gameObject.name,
                Distance = distance,
                IsVisible = screenPosRaw.z > 0,
                ScreenPosition = screenPos,
                BoxCorners = BoundsUtils.CalculateScreenCorners(camera, bounds),
            };
            tempNavMeshData.Add(data);
        }
        lock (Lock)
        {
            _navMeshAgentData = tempNavMeshData;
        }
    }
}