//Own data type for the Enemy's state, which can be waiting, wandering, suspicious, or alerted.
using UnityEngine;

namespace Assets.Scripts.Guardscripts 
{
    public enum GuardState //different states of Guards(for Vision)
    {
        Standing,
        Patrol, //Guard doesn't know player is there
        Alerted,//Gaurds on the map are aware of player is in building
        Attack // Alerted that the Gaurd 
    }
    public enum EndFacingDirection // Which way the Guard should turn to face once it reaches the LAST point
    {
        Forward, // Keep facing the same way you were walking
        Left,    // Turn 90 degrees left relative to your walking direction
        Right    // Turn 90 degrees right relative to your walking direction
    }
    public enum SuspicisusType //determines which way the guard is put in a suspisoius state. 
    {
        Eyes, // Guard will become suspicious if they see the player
        Hearing, // Guard will become suspicious if they hear the player
        none // Guard will not become suspicious
    }
    public enum PatrolMode //different Patrol Mode for the Gaurd 
    {
        Loop,
        BackAndForth
    }
}
