using UnityEngine;

namespace Assets.Scripts.Guardscripts
{
    // A single node in the patrol route. Each node stores one Vector3
    // position plus links to the node before and after it (double linked).
    public class GuardNode 
    {
        public Vector3 Position;
        public GuardNode Next;
        public GuardNode Previous;
        public GuardNode(Vector3 position)
        {
            Position = position;
            Next = null;
            Previous = null;
        }
    }
}
