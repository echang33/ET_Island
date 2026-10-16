using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShoot : MonoBehaviour
{
    public GameObject prefab;
    public GameObject shootPoint;
    
    // How many seconds to wait between each shot
    public float fireRate = 2f; 
    
    // Tracks time passed since the last shot
    private float timer = 0f;

    void Update()
    {
        // Time.deltaTime is the exact time it took to draw the last frame.
        // Adding it up creates a real-world stopwatch.
        timer += Time.deltaTime;

        // If the stopwatch reaches your fireRate, shoot and reset the clock
        if (timer >= fireRate)
        {
            OnFire();
            timer = 0f; 
        }
    }

    public void OnFire()
    {
        GameObject clone = Instantiate(prefab, shootPoint.transform.position, shootPoint.transform.rotation);
    }
}