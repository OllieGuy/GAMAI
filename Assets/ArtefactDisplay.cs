using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtefactDisplay : MonoBehaviour
{
    public Artefact artefact;
    void Start()
    {
        gameObject.GetComponent<MeshFilter>().mesh = artefact.mesh;
        gameObject.GetComponent<MeshRenderer>().material = artefact.material;
        gameObject.GetComponent<MeshCollider>().sharedMesh = artefact.mesh;
    }
}

