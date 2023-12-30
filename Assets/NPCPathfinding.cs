using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPCPathfinding
{
    private NavMeshAgent agent;
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

    public Vector3[] findPath(Vector3 targetPOS) //for some reason will calculate path to closest point rather than not working - maybe nvm?
    {
        NavMeshPath path = new();
        bool pathIsPossible = agent.CalculatePath(targetPOS, path);
        if (!pathIsPossible)
        {
            //Debug.Log("Path impossible to " + targetPOS);
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
        }
        //foreach (Vector3 point in pathWaypoints)
        //{
        //    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    sphere.transform.position = point;
        //    sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //    sphere.GetComponent<SphereCollider>().enabled = false;
        //}
        pathLength = pathWaypoints.Length - 1;
        return pathWaypoints;
    }
    public Vector3 moveTowardsCurrentTarget(float speed) //known bug where this will break when the NPC is already in the position
    {
        Vector3 newPos = agent.transform.position + (((currentTargetInPath - currentPath[currentTargetIndex - 1]) * speed) / magnitudeList[currentTargetIndex] * Time.deltaTime * GameTimer.GameTimeScale);
        return (newPos);
    }
}
