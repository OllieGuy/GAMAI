using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artefact", menuName = "Object/Artefact/New Artefact")]
public class Artefact : Object
{
    public ArtefactType type;
    public bool authenticity = true;

    public Mesh mesh;
    public Material material;

    public float calculateHappinessChange()
    {
        float change = 0;
        return change;
    }
}
public enum ArtefactType
{
    Red,
    Yellow,
    Blue
}