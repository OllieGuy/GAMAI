using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artefact", menuName = "Object/Artefact/New Artefact")]
public class Artefact : Object
{
    public ArtefactType type;
    public bool authenticity = true;
}
public enum ArtefactType
{
    Red,
    Yellow,
    Blue
}