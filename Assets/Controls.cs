using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Controls : MonoBehaviour
{
    [SerializeField] Transform plane;
    [SerializeField] Camera cam;
    [SerializeField] GameObject artefact;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] Artefact[] artefacts;
    [SerializeField] Material doorwayMaterial;
    Dictionary<string, Artefact> artefactDictionary = new Dictionary<string, Artefact>();
    public bool meshUpdate = false;
    private static Vector2Int startNavPoint = new Vector2Int(-1,-1);

    void Start()
    {
<<<<<<< Updated upstream
        foreach(Artefact a in artefacts)
=======
        Wall.doorwayMaterial = doorwayMaterial;
        foreach (Artefact a in artefacts)
>>>>>>> Stashed changes
        {
            artefactDictionary.Add(a.artefactName, a);
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.transform == plane)
            {
                Vector3 targetPos = hit.point;
                targetPos.x = Mathf.Round(targetPos.x);
                targetPos.y += 0.5f;
                targetPos.z = Mathf.Round(targetPos.z);
                placeBlockBasedOnKeyDown(targetPos);
            }
            else if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.transform.gameObject.CompareTag("Artefact"))
            {
                Destroy(hit.transform.gameObject);
            }
            meshUpdate = true;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.transform == plane)
            {
                agent.SetDestination(hit.point);
            }
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
                    List<Vector2Int> path = Node.convertToVector2Int(p.AStarSolve(startNavPoint, endNavPoint));
                    foreach(Vector2Int v in path)
                    {
                        //Debug.Log("23vbbw");
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = new Vector3(v.x, 0.5f, v.y);
                        cube.transform.localScale = new Vector3(1f,0.1f,1f);
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

    void placeBlockBasedOnKeyDown(Vector3 targetPos)
    {
        GameObject theArtefact;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            theArtefact = Instantiate(artefact, targetPos, Quaternion.identity);
            theArtefact.GetComponent<ArtefactDisplay>().artefact = artefactDictionary["Blue Block"];
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            theArtefact = Instantiate(artefact, targetPos, Quaternion.identity);
            theArtefact.GetComponent<ArtefactDisplay>().artefact = artefactDictionary["Red Block"];
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            theArtefact = Instantiate(artefact, targetPos, Quaternion.identity);
            theArtefact.GetComponent<ArtefactDisplay>().artefact = artefactDictionary["Yellow Block"];
        }
        else
        {
            Debug.Log("No colour selected");
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
