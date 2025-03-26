using System.Globalization;

namespace Nyx.Core.Settings;

public class FloatSettingAttribute : SettingAttribute
{
    public float MinValue { get; }
    public float MaxValue { get; }

    public FloatSettingAttribute(string displayName, string description, 
        float defaultValue, float minValue = float.MinValue, float maxValue = float.MaxValue)
        : base(displayName, description, defaultValue.ToString(CultureInfo.InvariantCulture), typeof(float))
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}