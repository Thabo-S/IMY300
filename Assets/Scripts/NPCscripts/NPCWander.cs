
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.NPCscripts; // Import the namespace for NPC scripts
using UnityEngine.AI; // AI library for pathfinding and navigation

// NameSpace for the NPC scripts, which can be used to organize related classes and avoid naming conflicts
namespace Assets.Scripts.NPCscripts
{
    public class NPCWander : NPCComponent
    {
        [SerializeField] private PatrolArea patrolArea; // Reference to the PatrolArea component that defines the wandering area

        private void Start()
        {
            setRandomDestination();
        }
        private void Update()
        {
            if (hasArrived())
            {
                setRandomDestination();
            }

        }
        public void ChangeState()
        {

        }
        private void setRandomDestination()
        {
            if (patrolArea == null)
                return;

            if (NPC == null || NPC.agent == null)
                return;

            if (!NPC.agent.isOnNavMesh)
                return;

            NPC.agent.SetDestination(patrolArea.GetRandomPoint());
        }


        private bool hasArrived()
        {
            if (NPC == null || NPC.agent == null)
                return false;

            if (!NPC.agent.isActiveAndEnabled)
                return false;

            if (!NPC.agent.isOnNavMesh)
                return false;

            if (NPC.agent.pathPending)
                return false;

            return NPC.agent.remainingDistance <= NPC.agent.stoppingDistance;
        }

        //
        private IEnumerator Wander()
        {
            while (true)
            {
                setRandomDestination();

                while (!hasArrived())
                {
                    yield return null;
                }

                yield return new WaitForSeconds(2f);
            }
        }

    }
}


