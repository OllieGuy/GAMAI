using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perception
{
    public int numberOfRaycasts = 8;
    public float angleSpread = 45f;

    public List<GameObject> fireRaycasts(Transform transform)
    {
        List<GameObject> percievedObjects = new List<GameObject>();
        float angleStep = angleSpread / (numberOfRaycasts - 1);
        int wallLayerMask = LayerMask.NameToLayer("Wall");
       
        for (int i = 0; i < numberOfRaycasts; i++)
        { 
            Debug.Log("tttt");
            float currentAngle = -angleSpread / 2f + i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0f, currentAngle, 0f) * transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection * 10, out hit, 10f, wallLayerMask))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    Debug.Log("Hit " + hit.collider.gameObject.name + " with tag " + hit.collider.tag);
                }
                if (hit.collider.CompareTag("Artefact") || hit.collider.CompareTag("NPC"))
                {
                    percievedObjects.Add(hit.collider.gameObject);
                    Debug.Log("Hit " + hit.collider.gameObject.name + " with tag " + hit.collider.tag);
                }
                
            }

            Debug.DrawRay(transform.position, rayDirection * 10, Color.green);
        }
        return percievedObjects;
    }
}
