using System;
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
    public List<LocalPosition> localArtefactFootprint;
    [NonSerialized] public List<Vector2Int> worldArtefactFootprint = new List<Vector2Int>();
    public List<LocalPosition> localViewingPositions;
    public List<TranslatedPosition> worldViewingPositions = new List<TranslatedPosition>();

    public Mesh mesh;
    public Material material;

    public void calculateWorldViewPositions(Transform _transform)
    {
        foreach (LocalPosition lvp in localViewingPositions)
        {
            worldViewingPositions.Add(new TranslatedPosition(lvp, _transform.GetComponent<Transform>()));
        }
    }
    public void calculateWorldFootprint(Transform _transform)
    {
        foreach (LocalPosition laf in localArtefactFootprint)
        {
            worldArtefactFootprint.Add(TranslatedPosition.translatePos(laf, _transform.GetComponent<Transform>()));
        }
    }
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
[Serializable]
public class LocalPosition
{
    public int xOffset;
    public int yOffset;
    public LocalPosition(int _xOffset, int _yOffset)
    {
        xOffset = _xOffset;
        yOffset = _yOffset;
    }
}
public class TranslatedPosition
{
    public Vector2Int position;
    public bool beingUsed;
    public TranslatedPosition(LocalPosition localPosition, Transform worldPos)
    {
        position = translatePos(localPosition,worldPos);
        beingUsed = false;
    }
    public TranslatedPosition(Vector2Int _position)
    {
        position = _position;
    }

    public static Vector2Int translatePos(LocalPosition localPosition, Transform worldPos)
    {
        Vector2Int rPos = new Vector2Int((int)worldPos.position.x + localPosition.xOffset, (int)worldPos.position.z + localPosition.yOffset);
        return rPos;
    }
}