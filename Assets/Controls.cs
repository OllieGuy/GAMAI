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
    Museum museum;
    public bool meshUpdate = false;

    void Start()
    {
        foreach(Artefact a in artefacts)
        {
            artefactDictionary.Add(a.artefactName, a);
        }
        museum = new Museum();
        museum.setUp();
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
    }

    void placeBlockBasedOnKeyDown(Vector2Int placePos)
    {
        GameObject theArtefact = null;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            theArtefact = Instantiate(artefact, new Vector3(placePos.x, 1f, placePos.y), Quaternion.identity);
            ArtefactDisplay ad = theArtefact.GetComponent<ArtefactDisplay>();
            ad.artefact = artefactDictionary["Blue Block"];
            ad.artefact.worldViewingPositions = museum.checkGridForValidSoftPlacement(ad.artefact.localViewingPositions, placePos);
            museum.updateGridWithPlacedObject(ad.artefact);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            theArtefact = Instantiate(artefact, new Vector3(placePos.x, 1f, placePos.y), Quaternion.identity);
            ArtefactDisplay ad = theArtefact.GetComponent<ArtefactDisplay>();
            ad.artefact = artefactDictionary["Red Block"];
            ad.artefact.worldViewingPositions = museum.checkGridForValidSoftPlacement(ad.artefact.localViewingPositions, placePos);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            theArtefact = Instantiate(artefact, new Vector3(placePos.x, 1f, placePos.y), Quaternion.identity);
            ArtefactDisplay ad = theArtefact.GetComponent<ArtefactDisplay>();
            ad.artefact = artefactDictionary["Yellow Block"];
            ad.artefact.worldViewingPositions = museum.checkGridForValidSoftPlacement(ad.artefact.localViewingPositions, placePos);
        }
        else
        {
            Debug.Log("No colour selected");
        }
        if (theArtefact != null) //DEBUG STUFF
        {
            foreach (TranslatedPosition tvp in theArtefact.GetComponent<ArtefactDisplay>().artefact.worldViewingPositions)
            {
                Debug.Log("cg");
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(tvp.position.x, 1f, tvp.position.y);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
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
