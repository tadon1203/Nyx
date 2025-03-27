using ImGuiNET;
using System.Numerics;

namespace Nyx.Core.Utils;

public static class ESPUtils
{
    public static void Draw2DBox(ImDrawListPtr drawList, Vector2 pos, float height, 
        Vector4 boxColor, Vector4 outlineColor)
    {
        float width = height * 0.5f;
        float outlineThickness = 1.0f;

        Vector2 boxMin = new(pos.X - width / 2, pos.Y - height);
        Vector2 boxMax = new(pos.X + width / 2, pos.Y);

        uint boxColorU32 = ImGui.ColorConvertFloat4ToU32(boxColor);
        uint outlineColorU32 = ImGui.ColorConvertFloat4ToU32(outlineColor);

        // Outer outline
        drawList.AddRect(
            new Vector2(boxMin.X - outlineThickness, boxMin.Y - outlineThickness),
            new Vector2(boxMax.X + outlineThickness, boxMax.Y + outlineThickness),
            outlineColorU32, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
        );

        // Inner outline
        drawList.AddRect(
            new Vector2(boxMin.X + outlineThickness, boxMin.Y + outlineThickness),
            new Vector2(boxMax.X - outlineThickness, boxMax.Y - outlineThickness),
            outlineColorU32, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
        );

        // Main box
        drawList.AddRect(boxMin, boxMax, boxColorU32, 0.0f, ImDrawFlags.RoundCornersAll, 1.0f);
    }
}
