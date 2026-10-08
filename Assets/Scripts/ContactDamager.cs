/*
Team members: Ethan Chang, Ryan Wu, Steven tan
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactDamager : MonoBehaviour
{

    public float damage;
    public bool destroyOnContact = false;

    void OnTriggerEnter(Collider other)
    {
        if (destroyOnContact)
        {
            Destroy(gameObject);
        }

        Life life = other.GetComponent<Life>();
        if (life != null)
        {
            life.amount -= damage;
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
