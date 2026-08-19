using System.Collections.Generic;
using Assets.Scripts.Guardscripts; // for GaurdNode 
using UnityEngine;

// Concrete strategy: 1 -> 2 -> 3 -> ... -> N -> 1 (wraps around forever).
public class LoopPatrolStrategy : IPatrolStrategy
{
    private GuardNode head;
    private GuardNode tail;
    private GuardNode currentNode;

    public void BuildRoute(List<Vector3> points)
    {
        head = null;
        tail = null;
        currentNode = null;

        if (points == null || points.Count == 0) //List doesn't exist  
        {
            Debug.LogWarning("LoopPatrolStrategy has no waypoints assigned.");
            return;
        }
        for (int i = 0; i < points.Count; i++)
        {
            GuardNode node = new GuardNode(points[i]);

            if (head == null) //if list is empty
            {
                head = node; //
                tail = node;
            }
            else
            {
                //list not empty
                node.Previous = tail;
                tail.Next = node;
                tail = node;
            }
        }
        //close Loop once , when list is ordered
        tail.Next = head;
        head.Previous = tail;

        // This was the missing piece - without it, currentNode stays null
        // forever, and every GetCurrentWaypoint()/GetNextWaypoint() call
        // falls through to Vector3.zero (world origin) instead of an
        // actual waypoint.
        currentNode = head;
    }
    public Vector3 GetCurrentWaypoint()
    {
        if (currentNode != null)
        {
            return currentNode.Position;
        }
        return Vector3.zero;
    }

    public Vector3 GetNextWaypoint()
    {
        if (currentNode == null)
        {
            return Vector3.zero;
        }
        else
        {
            currentNode = currentNode.Next;
        }
        return currentNode.Position;
    }
}