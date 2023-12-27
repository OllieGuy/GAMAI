using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPCPathfinding
{
    private NavMeshAgent agent;
    private float speed = 3f;
    public Vector3[] currentPath;
    public Vector3 currentTargetInPath;
    public Vector3 target;
    public int pathLength;
    public int currentTargetIndex;
    public NPCPathfinding(NavMeshAgent _nMAgent)
    {
        agent = _nMAgent;
    }

    public Vector3[] findPath(Vector3 targetPOS)
    {
        NavMeshPath path = new();
        bool pathIsPossible = agent.CalculatePath(targetPOS, path);
        if (!pathIsPossible)
        {
            Debug.Log("Path impossible");
            return null;
        }
        Vector3[] pathWaypoints = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++)
        {
            pathWaypoints[i] = path.corners[i];
        }
        foreach (Vector3 point in pathWaypoints)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = point;
            sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
        pathLength = pathWaypoints.Length - 1; //KEEP AN EYE ON THIS IN CASE IT BREAKS THINGS
        return pathWaypoints;
    }
    public Vector3 moveTowardsCurrentTarget()
    {
        //Debug.Log((agent.transform.position - Vector3.MoveTowards(agent.transform.position, currentTargetInPath, speed * Time.deltaTime)).magnitude);
        Vector3 newPos = Vector3.MoveTowards(agent.transform.position, currentTargetInPath, speed * Time.deltaTime * GameTimer.GameTimeScale);
        return (newPos);
    }
}
