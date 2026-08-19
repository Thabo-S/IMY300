using System.Collections.Generic;
using UnityEngine;

//Strategy interface: any patrol traversal style implements this.
//Path.cs (the context talkst to IPatrolStrategy
public interface IPatrolStrategy
{
   void BuildRoute(List<Vector3> points); // Build Route
    Vector3 GetCurrentWaypoint(); //retun current Waypoint
    Vector3 GetNextWaypoint(); // Return the next Waypoint
}
