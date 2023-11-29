using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artefact", menuName = "Artefact")]
public class Artefact : ScriptableObject
{
    public string artefactName;
    public string description;
    public ArtefactType type;
    public Rarity rarity;
    public int value;
    public bool authenticity = true;
    
    public Mesh mesh;
    public Material material;
}
public enum ArtefactType
{
    Red,
    Yellow,
    Blue
}
public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}