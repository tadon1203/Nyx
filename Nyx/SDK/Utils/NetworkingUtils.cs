using Nyx.Core;
using Nyx.Patching;

namespace Nyx.SDK;

public static class NetworkingUtils
{
    public static bool SendEvent(byte eventCode, object eventContent, int sender)
    {
        if (NetworkManagerPatch.GetInstance() == null)
        {
            ConsoleLogger.Log(LogType.Error, "Failed to get network manager instance.");
            return false;
        }
        
        
        
        return true;
    }
}