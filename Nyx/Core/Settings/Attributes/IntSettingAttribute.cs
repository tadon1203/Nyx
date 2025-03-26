namespace Nyx.Core.Settings;

public class IntSettingAttribute : SettingAttribute
{
    public int MinValue { get; }
    public int MaxValue { get; }

    public IntSettingAttribute(string displayName, string description, 
        int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
        : base(displayName, description, defaultValue.ToString(), typeof(int))
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}