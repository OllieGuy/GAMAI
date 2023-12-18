using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Controls : MonoBehaviour
{
    [SerializeField] Transform plane;
    [SerializeField] Camera cam;
    [SerializeField] GameObject artefact;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] NavMeshSurface surface;
    [SerializeField] Artefact[] artefacts;
    Dictionary<string, Artefact> artefactDictionary = new Dictionary<string, Artefact>();
    public bool meshUpdate = false;

    void Start()
    {
        foreach (Artefact a in artefacts)
        {
            artefactDictionary.Add(a.objectName, a);
        }
        meshUpdate = true;
    }
    void Update()
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
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.transform == plane)
            {
                agent.SetDestination(hit.point);
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
        //string a = "";
        //for (int i = 0; i < Museum.grid.GetLength(0); i++)
        //{
        //    for (int j = 0; j < Museum.grid.GetLength(1); j++)
        //    {
        //        if (Museum.grid[i, j].occupation == Occupation.Hard)
        //        {
        //            a += "h";
        //        }
        //        else if (Museum.grid[i, j].occupation == Occupation.Soft)
        //        {
        //            a += "s";
        //        }
        //        else
        //        {
        //            a += "0";
        //        }
        //        a += "  ";
        //    }
        //    a += "\n";
        //}
        //Debug.Log(a);
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
