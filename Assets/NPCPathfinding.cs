using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPCPathfinding
{
    private NavMeshAgent agent;
    private float speed = 1.7f;
    public Vector3[] currentPath;
    private float[] magnitudeList;
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
        magnitudeList = new float[path.corners.Length];
        pathWaypoints[0] = path.corners[0];
        magnitudeList[0] = 0;
        for (int i = 1; i < path.corners.Length; i++)
        {
            pathWaypoints[i] = path.corners[i];
            magnitudeList[i] = (pathWaypoints[i] - pathWaypoints[i - 1]).magnitude;
            Debug.Log(magnitudeList[i]);
        }
        foreach (Vector3 point in pathWaypoints)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = point;
            sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            sphere.GetComponent<SphereCollider>().enabled = false;
        }
        pathLength = pathWaypoints.Length - 1; //KEEP AN EYE ON THIS IN CASE IT BREAKS THINGS
        return pathWaypoints;
    }
    public Vector3 moveTowardsCurrentTarget()
    {
        Vector3 newPos = agent.transform.position + (((currentTargetInPath - currentPath[currentTargetIndex - 1]) * speed) / magnitudeList[currentTargetIndex] * Time.deltaTime * GameTimer.GameTimeScale);
        return (newPos);
    }
}
