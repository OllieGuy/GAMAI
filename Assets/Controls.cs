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
        foreach(Artefact a in artefacts)
        {
            artefactDictionary.Add(a.objectName, a);
        }
        meshUpdate = true;
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
        if (Input.anyKey)
        {
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            if(Object.checkGridForValidHardPlacement(artefactDictionary["Blue Block"].localFootprint,placePos))
            {
                theArtefact = Instantiate(artefact, new Vector3(placePos.x, 1f, placePos.y), Quaternion.identity);
                ObjectInstance oi = theArtefact.GetComponent<ObjectInstance>();
                oi.artefact = artefactDictionary["Blue Block"];
                oi.worldFootprint = oi.artefact.calculateWorldFootprint(placePos);
                oi.worldInteractionPositions = oi.artefact.checkGridForValidSoftPlacement(placePos);
                oi.updateMuseumGrid(false);
                oi.displayInteractionPoints();
            }
            else
            {
                Debug.Log("nuh uh");
            }
            
            //museum.updateGridWithPlacedObject(ad.artefact);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            theArtefact = Instantiate(artefact, new Vector3(placePos.x, 1f, placePos.y), Quaternion.identity);
            ObjectInstance od = theArtefact.GetComponent<ObjectInstance>();
            od.artefact = artefactDictionary["Red Block"];
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            if (Object.checkGridForValidHardPlacement(artefactDictionary["Yellow Block"].localFootprint, placePos))
            {
                theArtefact = Instantiate(artefact, new Vector3(placePos.x, 1f, placePos.y), Quaternion.identity);
                ObjectInstance oi = theArtefact.GetComponent<ObjectInstance>();
                oi.artefact = artefactDictionary["Yellow Block"];
                oi.worldFootprint = oi.artefact.calculateWorldFootprint(placePos);
                oi.worldInteractionPositions = oi.artefact.checkGridForValidSoftPlacement(placePos);
                oi.updateMuseumGrid(false);
                oi.displayInteractionPoints();
            }
            else
            {
                Debug.Log("nuh uh");
            }
            //ad.artefact.worldViewingPositions = museum.checkGridForValidSoftPlacement(ad.artefact.localViewingPositions, placePos);
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
