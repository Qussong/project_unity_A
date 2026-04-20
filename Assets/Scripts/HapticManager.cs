using UnityEngine;
using System.Runtime.InteropServices;

public static class HapticManager
{
    public enum HapticType { SoftMedium = 0, BasicMedium = 1, Error = 2, Wiggle = 3 }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void AIT_Vibrate(int type);
#endif

    public static void Vibrate(HapticType type = HapticType.SoftMedium)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        AIT_Vibrate((int)type);
#endif
    }
}
