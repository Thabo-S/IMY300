//Own data type for the Enemy's state, which can be waiting, wandering, suspicious, or alerted.
using System;
namespace Assets.Scripts.NPCscripts
{
    public enum State
    {
        Waiting,
        Wandering,
        Suspicious,
        Alerted
    }
}