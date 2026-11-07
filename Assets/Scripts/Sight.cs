/*
 * Sight.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Finds a visible target using a detection radius, viewing cone, and obstruction checks.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    [Header("Detection Volume")]
    [Tooltip("Search radius in world units.")]
    public float distance;
    [Tooltip("Viewing half-angle in degrees, measured from this object's forward direction.")]
    public float angle;
    [Header("Detection Layers")]
    [Tooltip("Layers containing possible targets.")]
    public LayerMask objectsLayers;
    [Tooltip("Layers that block line of sight; exclude the target's own layer.")]
    public LayerMask obstaclesLayers;
    // Detection is runtime state, never a target saved in the scene or prefab.
    [System.NonSerialized] public Collider detectedObject;

    void OnEnable()
    {
        detectedObject = null;
    }

    // Rebuild detection each frame so a stale target cannot survive an obstruction or scene change.
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
                    // Select the first visible candidate; this is not a nearest-target search.
                    detectedObject = collider;
                    Debug.DrawLine(transform.position, collider.bounds.center, Color.green);
                    break;
                }
            }
        }


    }
}
