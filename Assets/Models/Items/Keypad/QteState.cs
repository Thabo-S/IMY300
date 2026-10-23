using UnityEngine;

public static class QteState
{
    public static bool Keypad;
    public static bool Lockpick;

    public static bool IsActive => Keypad || Lockpick;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        Keypad = false;
        Lockpick = false;
    }
}