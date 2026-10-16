using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState { Idle, AttackPlayer}
    public EnemyState currentState;
    public float lastShootTime;
    public Sight sightSensor;
    public GameObject prefab;
    public GameObject shootPoint;

    public float fireRate = 2f;

    void Start()
    {
        if (sightSensor.detectedObject != null)
        {
            currentState = EnemyState.AttackPlayer;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == EnemyState.Idle)
        {
            if (sightSensor.detectedObject != null)
            {
                currentState = EnemyState.AttackPlayer;
            }
        } else if (currentState == EnemyState.AttackPlayer){
            AttackPlayer();
        }
    }

    void AttackPlayer()
    {
        if (sightSensor.detectedObject == null)
        {
            currentState = EnemyState.Idle;
            return;
        }
        LookTo(sightSensor.detectedObject.transform.position);
        Shoot();

    }

    void Shoot()
    {
        var timeSinceLastShoot = Time.time - lastShootTime;
        if (timeSinceLastShoot > fireRate)
        {
            lastShootTime = Time.time;
            Instantiate(prefab, shootPoint.transform.position, shootPoint.transform.rotation);
        }
    }

    void LookTo(Vector3 targetPosition)
    {
        Vector3 directionToPosition = Vector3.Normalize(targetPosition - transform.parent.position);
        directionToPosition.y = 0;
        transform.parent.forward = directionToPosition;
      
    }


}
