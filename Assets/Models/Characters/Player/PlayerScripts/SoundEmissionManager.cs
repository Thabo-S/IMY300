using UnityEngine;
using System;

public static class SoundEmissionManager
{
    public static event Action<Vector3, float, bool> OnSoundEmitted;

    public static void EmitSound(Vector3 position, float volume, bool instantAlert = false)
    {
        OnSoundEmitted?.Invoke(position, volume, instantAlert);
    }
}