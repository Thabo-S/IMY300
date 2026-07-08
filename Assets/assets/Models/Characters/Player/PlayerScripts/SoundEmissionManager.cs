using UnityEngine;
using System;

public static class SoundEmissionManager
{
    // position of the sound, and its volume (used as detection
    // radius to check if the noise made by the player can be detected by the guard)
    public static event Action<Vector3, float> OnSoundEmitted;

    public static void EmitSound(Vector3 position, float volume)
    {
        OnSoundEmitted?.Invoke(position, volume);
    }
}