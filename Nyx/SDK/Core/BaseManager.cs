using System.Collections.Generic;
using Nyx.SDK.Utils;
using UnityEngine;

namespace Nyx.SDK.Core;

public abstract class BaseManager<T, TData> where TData : ObjectData, new()
{
    protected List<T> TrackedObjects = [];
    private readonly List<TData> _objectsData = [];
    private readonly object _dataLock = new();

    public IReadOnlyList<TData> ObjectsData
    {
        get
        {
            lock (_dataLock)
                return _objectsData.AsReadOnly();
        }
    }
    
    public abstract void FindObjects();
    
    public void UpdateObjects()
    {
        if (Camera.main == null) return;

        var newData = new List<TData>();
        foreach (var obj in TrackedObjects)
        {
            if (obj == null) continue;
            newData.Add(CreateObjectData(obj));
        }

        lock (_dataLock)
        {
            _objectsData.Clear();
            _objectsData.AddRange(newData);
        }
    }

    protected abstract TData CreateObjectData(T obj);

    protected SysVec2[] CalculateBoxCorners(Camera camera, Bounds bounds)
    {
        Vector3[] corners =
        [
            new(bounds.min.x, bounds.min.y, bounds.min.z),
            new(bounds.max.x, bounds.min.y, bounds.min.z),
            new(bounds.max.x, bounds.min.y, bounds.max.z),
            new(bounds.min.x, bounds.min.y, bounds.max.z),
            new(bounds.min.x, bounds.max.y, bounds.min.z),
            new(bounds.max.x, bounds.max.y, bounds.min.z),
            new(bounds.max.x, bounds.max.y, bounds.max.z),
            new(bounds.min.x, bounds.max.y, bounds.max.z)
        ];

        var screenCorners = new SysVec2[8];
        for (int i = 0; i < 8; i++)
        {
            screenCorners[i] = ScreenUtils.WorldToScreenPoint(camera, corners[i]);
        }
        return screenCorners;
    }
}