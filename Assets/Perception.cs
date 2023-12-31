using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Perception
{
    public int numberOfRaycasts = 8;
    public float angleSpread = 60f;

    public List<GameObject>[] fireRaycasts(Transform transform)
    {
        List<GameObject>[] returnArray = new List<GameObject>[2];
        List<GameObject> percievedObjects = new List<GameObject>();
        List<GameObject> percievedNPCs = new List<GameObject>();

        float angleStep = angleSpread / (numberOfRaycasts - 1);
        int wallLayerMask = LayerMask.NameToLayer("Wall");
        Vector3 raycastPos = new Vector3(transform.position.x, 1f, transform.position.z);
        
        for (int i = 0; i < numberOfRaycasts; i++)
        { 
            float currentAngle = -angleSpread / 2f + i * angleStep;
            Vector3 rayDirection = (Quaternion.Euler(0f, currentAngle, 0f) * transform.forward)*10;
            RaycastHit hit;
            if (Physics.Raycast(raycastPos, rayDirection, out hit, 10f))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    //Debug.Log("Hit " + hit.collider.gameObject.name + " with tag " + hit.collider.tag);
                }
                if (hit.collider.CompareTag("Artefact") || hit.collider.CompareTag("Donation Box") || hit.collider.CompareTag("Furniture"))
                {
                    if (!percievedObjects.Contains(hit.collider.gameObject)) //change to the NPCs memory
                    {
                        //Debug.Log("Hit new " + hit.collider.gameObject.name + " with tag " + hit.collider.tag);
                        percievedObjects.Add(hit.collider.gameObject);
                        Debug.DrawRay(raycastPos, rayDirection, Color.red); //DEBUG ONLY
                    }
                    else
                    {
                        Debug.DrawRay(raycastPos, rayDirection, Color.yellow); //DEBUG ONLY
                    }
                }
                if(hit.collider.CompareTag("NPC"))
                {
                    if (!percievedNPCs.Contains(hit.collider.gameObject)) //change to the NPCs memory
                    {
                        //Debug.Log("Hit new " + hit.collider.gameObject.name + " with tag " + hit.collider.tag);
                        percievedNPCs.Add(hit.collider.gameObject);
                        Debug.DrawRay(raycastPos, rayDirection, Color.red); //DEBUG ONLY
                    }
                    else
                    {
                        Debug.DrawRay(raycastPos, rayDirection, Color.yellow); //DEBUG ONLY
                    }
                }
                else
                {
                    Debug.DrawRay(raycastPos, rayDirection, Color.green); //DEBUG ONLY
                }
            }
        }
        returnArray[0] = percievedObjects;
        returnArray[1] = percievedNPCs;

        return returnArray;
    }
    public static List<ObjectInstance> convertToObjectInstanceList(List<GameObject> inList)
    {
        List<ObjectInstance> result = new List<ObjectInstance>();
        foreach(GameObject obj in inList)
        {
            result.Add(obj.GetComponent<ObjectInstance>());
        }
        return result;
    }
    public static List<NPC> convertToNPCList(List<GameObject> inList)
    {
        List<NPC> result = new List<NPC>();
        foreach (GameObject obj in inList)
        {
            result.Add(obj.GetComponent<NPCController>().npc);
        }
        return result;
    }
}
