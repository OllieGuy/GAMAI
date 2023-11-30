using System.Collections;
using System.Collections.Generic;
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
    }

    void placeBlockBasedOnKeyDown(Vector3 targetPos)
    {
        GameObject theArtefact;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            theArtefact = Instantiate(artefact, targetPos, Quaternion.identity);
            theArtefact.GetComponent<ArtefactDisplay>().artefact = artefactDictionary["Blue Block"];
            theArtefact.GetComponent<ArtefactDisplay>().artefact.calculateWorldViewPositions(theArtefact.transform);
            foreach (TranslatedPosition tvp in theArtefact.GetComponent<ArtefactDisplay>().artefact.worldViewingPositions)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(tvp.position.x, 1f, tvp.position.y);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            theArtefact = Instantiate(artefact, targetPos, Quaternion.identity);
            theArtefact.GetComponent<ArtefactDisplay>().artefact = artefactDictionary["Red Block"];
            theArtefact.GetComponent<ArtefactDisplay>().artefact.calculateWorldViewPositions(theArtefact.transform);
            foreach (TranslatedPosition tvp in theArtefact.GetComponent<ArtefactDisplay>().artefact.worldViewingPositions)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(tvp.position.x, 1f, tvp.position.y);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            theArtefact = Instantiate(artefact, targetPos, Quaternion.identity);
            theArtefact.GetComponent<ArtefactDisplay>().artefact = artefactDictionary["Yellow Block"];
            theArtefact.GetComponent<ArtefactDisplay>().artefact.calculateWorldViewPositions(theArtefact.transform);
            foreach (TranslatedPosition tvp in theArtefact.GetComponent<ArtefactDisplay>().artefact.worldViewingPositions)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(tvp.position.x, 1f, tvp.position.y);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }
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
