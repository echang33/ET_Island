/*
 * ContactDamager.cs
 * Team members: Ethan Chang, Ryan Wu, Steven Tan
 * Purpose: Applies damage to Life components on trigger entry and optionally consumes the damaging object.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactDamager : MonoBehaviour
{

    [Header("Contact Damage")]
    [Tooltip("Health removed once when a collider with Life enters this trigger.")]
    public float damage;
    [Tooltip("Destroy this object on any trigger entry, even if the other collider has no Life.")]
    public bool destroyOnContact = false;

    void OnTriggerEnter(Collider other)
    {
        if (destroyOnContact)
        {
            Destroy(gameObject);
        }

        // Destroy is deferred until the end of the frame, so damage still applies on this hit.
        Life targetLife = other.GetComponent<Life>();
        if (targetLife != null)
        {
            targetLife.amount -= damage;
        }
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
