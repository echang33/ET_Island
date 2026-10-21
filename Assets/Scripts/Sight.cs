using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    // Start is called before the first frame update
    public float distance;
    public float angle;
    public LayerMask objectsLayers;
    public LayerMask obstaclesLayers;
    // Detection is runtime state, never a target saved in the scene or prefab.
    [System.NonSerialized] public Collider detectedObject;

    void OnEnable()
    {
        detectedObject = null;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, objectsLayers);
        detectedObject = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            Vector3 directionToCollider = Vector3.Normalize(collider.bounds.center - transform.position);
            Vector3 directionToController = Vector3.Normalize(collider.bounds.center - transform.position);
            float angleToCollider = Vector3.Angle(transform.forward, directionToCollider);
            if (angleToCollider < angle)
            {
                if (!Physics.Linecast(transform.position, collider.bounds.center, obstaclesLayers))
                {
                    detectedObject = collider;
                    Debug.DrawLine(transform.position, collider.bounds.center, Color.green);
                    break;
                }
            }
        }


    }
}
