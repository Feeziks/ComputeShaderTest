using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{

    //Public Members
    public float seperationWeight = 0.33f;
    public float alignmentWeight = 0.33f;
    public float cohesionWeight = 0.33f;

    //Private members
    private List<GameObject> nearbyBoids;
    private const float viewDistance = 20.0f;
    private Vector3 DAccel; //Delta Acceleration - force to apply after performing all rules / checks
    private Rigidbody myBody;
    void Start()
    {
        myBody = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Update will only serve to move the boid
    }

    void FixedUpdate()
    {
        //Clear the Delta Acceleration from last update
        DAccel = Vector3.zero;
        
        //Fixed update will actually perform the seperation, alignment, and cohesion steps
        GetNearbyBoids();

        Vector3 sperationAccel = Seperation();

        Vector3 alignmentAccel = Alignment();

        Vector3 cohesionAccel = Cohesion();

        //Sum all of the accelerations according to some weights
        DAccel += sperationAccel * seperationWeight;
        DAccel += alignmentAccel * alignmentWeight;
        DAccel += cohesionAccel * cohesionWeight;


        //Apply this acceleration to the boid as a force
        myBody.AddForce(DAccel, ForceMode.Acceleration);
    }


    private Vector3 Seperation()
    {
        //Look at the boids we have hit nearby
        //Calculate a cumulative change to move away from them
        //Weighted by how close we are to each boid

        return Vector3.zero;
    }

    private Vector3 Alignment()
    {


        return Vector3.zero;
    }

    private Vector3 Cohesion()
    {

        return Vector3.zero;
    }

    // Helpers

    private void GetNearbyBoids()
    {
        nearbyBoids = new List<GameObject>();

        //Cast a sphere of radius max view distance and find all collisions
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewDistance);

        foreach(Collider hit in hitColliders)
        {
            //In the future change this to only look at boids within a given field of vision
            Debug.Log("Hit boid: " + hit.gameObject.name);
            nearbyBoids.Add(hit.gameObject);
        }
    }

    // Misc.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }

}
