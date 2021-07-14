using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    //Public Members
    public ObstacleSettings settings;
    public List<GameObject> obstacles;

    //Private Members

    //Public functions

    //Private functions
    void Start()
    {
        obstacles = new List<GameObject>();
        //Start a co-routine that will spawn an obstacle every n seconds
        StartCoroutine("SpawnObstaclesForever");
    }

    private void Update()
    {
        obstacles.RemoveAll(item => item == null);
    }

    private void SpawnObstacle()
    {
        GameObject newObstacle = GameObject.CreatePrimitive((PrimitiveType)((int)Random.Range(0.0f, 3.0f))); //Create a random primitive
        newObstacle.transform.SetParent(transform);
        newObstacle.transform.localScale = new Vector3(Random.Range(10.0f, 40.0f), Random.Range(10.0f, 40.0f), Random.Range(10.0f, 40.0f));
        newObstacle.layer = LayerMask.NameToLayer("Obstacle_Layer");

        int spawnPoint = (int)Random.Range(0.0f, 6.0f);
        Vector3 spawnPointVector = new Vector3(0.0f, 0.0f, 0.0f);

        switch(spawnPoint)
        {
            case 0:
                spawnPointVector.x = -1.0f;
                break;
            case 1:
                spawnPointVector.x = 1.0f;
                break;
            case 2:
                spawnPointVector.y = -1.0f;
                break;
            case 3:
                spawnPointVector.y = 1.0f;
                break;
            case 4:
                spawnPointVector.z = -1.0f;
                break;
            case 5:
            default:
                spawnPointVector.z = 1.0f;
                break;
        }

        newObstacle.transform.position = spawnPointVector * 200.0f;

        newObstacle.AddComponent(typeof(Rigidbody));
        Rigidbody rb = newObstacle.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.velocity = -1.0f * spawnPointVector * settings.obstacleVelocity;
        obstacles.Add(newObstacle);
        Destroy(newObstacle, settings.obstacleLifeTime);
    }

    //Co-routines
    private IEnumerator SpawnObstaclesForever()
    {
        while(true)
        {
            if (transform.childCount < settings.maxNumObstacles)
            {
                SpawnObstacle();
            }
            yield return new WaitForSeconds((float) 1.0f / settings.obstacleFreq);
        }
    }
}
