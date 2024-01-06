using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPCPathfinding
{
    private NavMeshAgent agent;
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
            //Debug.Log("Path impossible to " + targetPOS);
            return null;
        }
        Vector3[] pathWaypoints = new Vector3[path.corners.Length];
        pathWaypoints[0] = path.corners[0];
        for (int i = 1; i < path.corners.Length; i++)
        {
            pathWaypoints[i] = path.corners[i];
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
        Vector2 agentVector2 = new Vector2(agent.transform.position.x, agent.transform.position.z);
        Vector2 targetVector2 = new Vector2(currentTargetInPath.x, currentTargetInPath.z);
        Vector2 newPos = agentVector2 + ((targetVector2 - agentVector2).normalized * (speed * Time.deltaTime * GameTimer.GameTimeScale));
        Vector3 returnPos = new Vector3(newPos.x, 1.5f, newPos.y);
        return returnPos;
    }
}
