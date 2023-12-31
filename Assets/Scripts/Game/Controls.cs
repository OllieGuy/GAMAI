using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Controls : MonoBehaviour
{
    [SerializeField] Transform plane;
    [SerializeField] Camera cam;
    [SerializeField] GameObject baseObject;
    [SerializeField] GameObject NPCprefab;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] Object[] objects;
    [SerializeField] Material doorwayMaterial;
    [SerializeField] Camera placementCam;
    [SerializeField] Camera viewCam;

    //[SerializeField] GameObject testNPC; //THIS IS TEMPORARY FOR TESTING A SINGLE NPC
    Dictionary<string, Object> objectDictionary = new Dictionary<string, Object>();
    public bool meshUpdate = false;
    private static Vector2Int startNavPoint = new Vector2Int(-1,-1);

    void Start()
    {
        Wall.doorwayMaterial = doorwayMaterial;
        foreach (Object o in objects)
        {
            objectDictionary.Add(o.objectName, o);
            o.baseHappinessValue = MathsFunctions.baseHappinessValue(o.value, o.getRarityMultiplier()); //exec at start means values can be tweaked without having to manually recalc
            //Debug.Log(o.objectName + ": " + o.baseHappinessValue);
        }
        placementCam.enabled = true;
        viewCam.enabled = false;
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
                else if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && (hit.transform.gameObject.CompareTag("Artefact") || hit.transform.gameObject.CompareTag("Donation Box")))
                {
                    ObjectInstance oi = hit.transform.gameObject.GetComponent<ObjectInstance>();
                    oi.removeObject();
                }
                meshUpdate = true;
            }
            if (Input.GetMouseButtonDown(1))
            {
                //StartCoroutine(instantiateNPCQuickly());
                //StartCoroutine(instantiateNPCRandomly());
                Instantiate(NPCprefab, new Vector3(0, 1.5f, 0), Quaternion.identity);
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
            if (Input.GetKeyDown(KeyCode.Z))
            {
                placementCam.enabled = !placementCam.enabled;
                viewCam.enabled = !viewCam.enabled;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void placeBlockBasedOnKeyDown(Vector2Int placePos)
    {
        GameObject theObject;
        string objectToPlace = null;
        string tagToUse = "Untagged";
        if (Input.anyKeyDown)
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                objectToPlace = "Blue Block";
                tagToUse = "Artefact";
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                objectToPlace = "Red Block";
                tagToUse = "Artefact";
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                objectToPlace = "Yellow Block";
                tagToUse = "Artefact";
            }
            else if (Input.GetKey(KeyCode.Alpha4))
            {
                objectToPlace = "Donation Box";
                tagToUse = "Donation Box";
            }
        }
        if (objectToPlace != null)
        {
            if (Object.checkGridForValidHardPlacement(objectDictionary[objectToPlace].localFootprint, placePos))
            {
                theObject = Instantiate(baseObject, new Vector3(placePos.x, 1f, placePos.y), Quaternion.identity);
                theObject.tag = tagToUse;
                ObjectInstance oi = theObject.GetComponent<ObjectInstance>();
                oi.theObject = objectDictionary[objectToPlace];
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

    IEnumerator instantiateNPCQuickly()
    {
        int totalCount = 0;
        while (totalCount < 2000)
        {
            Instantiate(NPCprefab, new Vector3(0, 1.5f, 0), Quaternion.identity);
            totalCount++;
            yield return new WaitForSeconds(0.02f);
        }
    }
    IEnumerator instantiateNPCRandomly()
    {
        int totalCount = 0;
        while (totalCount < 25)
        {
            Instantiate(NPCprefab, new Vector3(0, 1.5f, 0), Quaternion.identity);
            totalCount++;

            float delay = UnityEngine.Random.Range(1f, 5f);
            yield return new WaitForSeconds(delay);
        }
    }
}
