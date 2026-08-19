using System.Collections.Generic;
using Assets.Scripts.Guardscripts; // for GuardNode, EndFacingDirection
using UnityEngine;

// Concrete strategy: 1 -> 2 -> 3 -> ... -> N -> ... -> 3 -> 2 -> 1 -> 2 -> 3 ...
// Also owns the end-point facing behaviour, since that's specific to this style
public class BackAndForthPatrolStrategy : IPatrolStrategy
{
    private GuardNode head;
    private GuardNode tail;
    private GuardNode currentNode;
    private bool movingForward = true;

    private readonly EndFacingDirection endPointFacing;

    public BackAndForthPatrolStrategy(EndFacingDirection endPointFacing)
    {
        this.endPointFacing = endPointFacing;
    }

    public EndFacingDirection EndPointFacingSetting => endPointFacing;

    public void BuildRoute(List<Vector3> points)
    {
        head = null;
        tail = null;
        currentNode = null;
        movingForward = true;

        if (points == null || points.Count == 0)
        {
            Debug.LogWarning("BackAndForthPatrolStrategy has no waypoints assigned.");
            return;
        }

        foreach (Vector3 point in points)
        {
            GuardNode node = new GuardNode(point);

            if (head == null)
            {
                head = node;
                tail = node;
            }
            else
            {
                node.Previous = tail;
                tail.Next = node;
                tail = node;
            }
        }

        // Deliberately NOT closed into a loop - tail.Next and head.Previous
        // stay null, which is how GetNextWaypoint() knows when to reverse.
        currentNode = head;
    }

    public Vector3 GetCurrentWaypoint()
    {
        return currentNode != null ? currentNode.Position : Vector3.zero;
    }

    public Vector3 GetNextWaypoint()
    {
        if (currentNode == null) return Vector3.zero;

        if (movingForward)
        {
            if (currentNode.Next != null)
            {
                currentNode = currentNode.Next;
            }
            else
            {
                // Hit the end of the list (tail) - reverse direction.
                movingForward = false;
                currentNode = currentNode.Previous;
            }
        }
        else
        {
            if (currentNode.Previous != null)
            {
                currentNode = currentNode.Previous;
            }
            else
            {
                // Hit the start of the list (head) - go forward again.
                movingForward = true;
                currentNode = currentNode.Next;
            }
        }

        return currentNode.Position;
    }

    public Quaternion ComputeFacingRotation(Vector3 incomingDirection, Transform fallbackTransform)
    {
        incomingDirection.y = 0f;

        if (incomingDirection.sqrMagnitude < 0.0001f)
        {
            return fallbackTransform.rotation; // nothing sensible to derive - keep current facing
        }

        incomingDirection.Normalize();

        Vector3 faceDirection;
        switch (endPointFacing)
        {
            case EndFacingDirection.Left:
                faceDirection = Quaternion.Euler(0f, -90f, 0f) * incomingDirection;
                break;
            case EndFacingDirection.Right:
                faceDirection = Quaternion.Euler(0f, 90f, 0f) * incomingDirection;
                break;
            case EndFacingDirection.Forward:
            default:
                faceDirection = incomingDirection;
                break;
        }

        return Quaternion.LookRotation(faceDirection);
    }
}
