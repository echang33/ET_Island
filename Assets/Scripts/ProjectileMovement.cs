using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    public float speed = 15f;
    
    // How long the laser exists before deleting itself, to prevent lagging your game
    public float lifetime = 5f; 

    void Start()
    {
        // Automatically destroy this object after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move forward along the Z-axis every frame, independent of framerate
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
