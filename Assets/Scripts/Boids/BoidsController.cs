using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsController : MonoBehaviour
{
    //Public 
    [Header("Flock Parameters")]
    [Min(1)]
    public int numBoids;

    [Header("Individual Parameters")]
    public float maxVel;
    public float maxAccel;
    public float maxVisionDistance;
    public float maxTurnSpeed;


    //Private


    void Start()
    {

        for(int i = 0; i < numBoids; i++)
        {
            GameObject thisBoid = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            thisBoid.AddComponent( typeof(Boid) ); //Add our custom script to this new object
            thisBoid.AddComponent(typeof(Rigidbody));
            thisBoid.GetComponent<Rigidbody>().useGravity = false; // Float
            thisBoid.transform.SetParent(transform);
            thisBoid.transform.position = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f)); //Place the boid randomly
            thisBoid.transform.rotation = Random.rotation; //Have the boid face a random direction
            thisBoid.GetComponent<Rigidbody>().AddForce(thisBoid.transform.up * Random.Range(0.0f, 100.0f), ForceMode.Acceleration); //Kick the boid
        }
    }

    void Update()
    {

    }

}
