using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI; // AI library for pathfinding and navigation
using UnityEngine;

// NameSpace for the NPC scripts, which can be used to organize related classes and avoid naming conflicts
namespace Assets.Scripts.NPCscripts
{
    [RequireComponent(typeof(NavMeshAgent))] // Ensure that a NavMeshAgent component is attached to the GameObject
    [RequireComponent(typeof(Animator))] // Ensure that an Animator component is attached to the GameObject

    public class NPCScript : MonoBehaviour
    {
        [HideInInspector] public NavMeshAgent agent; // Reference to the NavMeshAgent component
        [HideInInspector] public Animator animator; // Reference to the Animator component

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public float CurrentSpeed()
        {
            return agent.velocity.magnitude; // Return the current speed of the NavMeshAgent
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }
    }


    //NPCComponent 
    public class NPCComponent : MonoBehaviour
    {
        protected NPCScript NPC;

        protected virtual void Awake()
        {
            NPC = GetComponent<NPCScript>();
        }
    }

}
