using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.NPCscripts; // Import the namespace for NPC scripts

public class NPCmovementScript : NPCComponent 
{
    private void Update()
    {
        NPC.animator.SetFloat("Speed", NPC.CurrentSpeed());// Update the "Speed" parameter in the Animator based on the current speed of the NavMeshAgent
    }
}
