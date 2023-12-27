using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Controls : MonoBehaviour
{
    [SerializeField] Transform plane;
    [SerializeField] Camera cam;
    [SerializeField] GameObject artefact;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] Artefact[] artefacts;
    [SerializeField] Material doorwayMaterial;
    [SerializeField] GameObject testNPC; //THIS IS TEMPORARY FOR TESTING A SINGLE NPC
    Dictionary<string, Artefact> artefactDictionary = new Dictionary<string, Artefact>();
    public bool meshUpdate = false;
    private static Vector2Int startNavPoint = new Vector2Int(-1,-1);

    void Start()
    {
        Wall.doorwayMaterial = doorwayMaterial;
        foreach (Artefact a in artefacts)
        {
            artefactDictionary.Add(a.objectName, a);
        }
    }
    void Update()
    {
        {
            checkInput();
        }

        void checkInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.transform == plane)
                {
                    Vector3 targetPos = hit.point;
                    Vector2Int placePos = new Vector2Int();
                    placePos.x = (int)Mathf.Round(targetPos.x);
                    placePos.y = (int)Mathf.Round(targetPos.z);
                    placeBlockBasedOnKeyDown(placePos);
                }
                else if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.transform.gameObject.CompareTag("Artefact"))
                {
                    ObjectInstance oi = hit.transform.gameObject.GetComponent<ObjectInstance>();
                    oi.removeObject();
                }
                meshUpdate = true;
            }
            if (Input.GetMouseButtonDown(1))
            {
                testNPC.SetActive(true);
                //Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                //RaycastHit hit;
                //if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.transform == plane)
                //{
                //    //agent.SetDestination(hit.point);
                //    NavMeshPath path = new();
                //    agent.CalculatePath(hit.point, path);
                //    Vector3[] pathWaypoints = new Vector3[path.corners.Length];
                //    for (int i = 0; i < path.corners.Length; i++)
                //    {
                //        pathWaypoints[i] = path.corners[i];
                //    }
                //    foreach (Vector3 point in pathWaypoints)
                //    {
                //        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //        sphere.transform.position = point;
                //        sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                //    }
                //}
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.transform == plane)
                {
                    if (startNavPoint.x != -1)
                    {
                        Vector2Int endNavPoint = new Vector2Int();
                        Vector3 targetPos = hit.point;
                        endNavPoint.x = (int)Mathf.Round(targetPos.x);
                        endNavPoint.y = (int)Mathf.Round(targetPos.z);
                        Pathfinding p = new Pathfinding();
                        List<Vector2Int> path = AStarNode.convertToVector2Int(p.AStarSolve(startNavPoint, endNavPoint));
                        foreach (Vector2Int v in path)
                        {
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.transform.position = new Vector3(v.x, 0.5f, v.y);
                            cube.transform.localScale = new Vector3(1f, 0.1f, 1f);
                        }
                        startNavPoint = new Vector2Int(-1, -1);
                    }
                    else
                    {
                        Vector3 targetPos = hit.point;
                        startNavPoint.x = (int)Mathf.Round(targetPos.x);
                        startNavPoint.y = (int)Mathf.Round(targetPos.z);
                    }
                }
            }
        }
    }

    void placeBlockBasedOnKeyDown(Vector2Int placePos)
    {
        GameObject theArtefact = null;
        string artefactToPlace = null;
        if (Input.anyKeyDown)
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                artefactToPlace = "Blue Block";
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                artefactToPlace = "Red Block";
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                artefactToPlace = "Yellow Block";
            }
        }
        if (artefactToPlace != null)
        {
            if (Object.checkGridForValidHardPlacement(artefactDictionary[artefactToPlace].localFootprint, placePos))
            {
                theArtefact = Instantiate(artefact, new Vector3(placePos.x, 1f, placePos.y), Quaternion.identity);
                ObjectInstance oi = theArtefact.GetComponent<ObjectInstance>();
                oi.artefact = artefactDictionary[artefactToPlace];
                oi.createObject(placePos);
            }
        }

    }
    void LateUpdate()
    {
        if (meshUpdate)
        {
            surface.BuildNavMesh();
            meshUpdate = false;
        }
    }
}
