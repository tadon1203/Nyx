using ImGuiNET;
using Nyx.SDK.Core;
using System.Numerics;

namespace Nyx.Core.Utils;

public static class ESPUtils
{
    public static bool ShouldRenderEntity(ObjectData entity, float maxDistance)
    {
        return entity.IsVisible && entity.Distance <= maxDistance;
    }

    public static void Draw2DBox(ImDrawListPtr drawList, SysVec2 pos, float height, 
        SysVec4 boxColor, SysVec4 outlineColor, float outlineThickness = 1.0f)
    {
        float width = height * 0.5f;
        SysVec2 boxMin = new(pos.X - width / 2, pos.Y - height);
        SysVec2 boxMax = new(pos.X + width / 2, pos.Y);

        uint boxColorU32 = ImGui.ColorConvertFloat4ToU32(boxColor);
        uint outlineColorU32 = ImGui.ColorConvertFloat4ToU32(outlineColor);
        
        drawList.AddRect(
            new(boxMin.X - outlineThickness, boxMin.Y - outlineThickness),
            new(boxMax.X + outlineThickness, boxMax.Y + outlineThickness),
            outlineColorU32, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
        );
        
        drawList.AddRect(
            new(boxMin.X + outlineThickness, boxMin.Y + outlineThickness),
            new(boxMax.X - outlineThickness, boxMax.Y - outlineThickness),
            outlineColorU32, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
        );
        
        drawList.AddRect(boxMin, boxMax, boxColorU32, 0.0f, ImDrawFlags.RoundCornersAll, 1.0f);
    }

    public static void Draw3DBox(ImDrawListPtr drawList, SysVec2[] screenCorners, SysVec4 color, float lineThickness, float fillOpacity)
    {
        int[][] edges =
        [
            [0, 1], [1, 2], [2, 3], [3, 0],
            [4, 5], [5, 6], [6, 7], [7, 4],
            [0, 4], [1, 5], [2, 6], [3, 7]
        ];

        uint lineColor = ImGui.ColorConvertFloat4ToU32(color);
        uint fillColor = ImGui.ColorConvertFloat4ToU32(new(color.X, color.Y, color.Z, fillOpacity));

        foreach (int[] edge in edges)
        {
            int a = edge[0];
            int b = edge[1];

            if (screenCorners[a].X > -999 && screenCorners[b].X > -999)
            {
                drawList.AddLine(screenCorners[a], screenCorners[b], lineColor, lineThickness);
            }
        }

        int[][] faces =
        [
            [0, 1, 2, 3], 
            [4, 5, 6, 7],
            [0, 1, 5, 4],
            [1, 2, 6, 5],
            [2, 3, 7, 6],
            [3, 0, 4, 7] 
        ];

        foreach (int[] face in faces)
        {
            if (screenCorners[face[0]].X > -999 && screenCorners[face[1]].X > -999 &&
                screenCorners[face[2]].X > -999 && screenCorners[face[3]].X > -999)
            {
                drawList.AddQuadFilled(
                    screenCorners[face[0]],
                    screenCorners[face[1]],
                    screenCorners[face[2]],
                    screenCorners[face[3]],
                    fillColor
                );
            }
        }
    }

    public static void DrawBone(ImDrawListPtr drawList, SysVec2 start, SysVec2 end, 
        SysVec4 lineColor, float lineThickness)
    {
        if (start.X > -999 && end.X > -999)
        {
            uint color = ImGui.ColorConvertFloat4ToU32(lineColor);
            drawList.AddLine(start, end, color, lineThickness);
        }
    }

    public static void DrawJoint(ImDrawListPtr drawList, SysVec2 position, 
        SysVec4 jointColor, float size)
    {
        if (position.X > -999)
        {
            uint color = ImGui.ColorConvertFloat4ToU32(jointColor);
            drawList.AddCircleFilled(position, size, color);
        }
    }
}
