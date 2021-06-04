﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{

    //Public Members
    public float seperationWeight = 0.33f;
    public float alignmentWeight = 0.33f;
    public float cohesionWeight = 0.33f;

    public float futureSight = 100;// How many fixed updates into the future can the boid predict its fellow boids motion?

    //Private members
    private List<GameObject> nearbyBoids;
    private const float viewDistance = 20.0f;
    private const float seperateDistance = 10.0f;
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

        //TODO: How / When to change the orientation of the boid so it continues to move "forward"


        //Apply this acceleration to the boid as a force
        myBody.AddForce(DAccel, ForceMode.Acceleration);

        //If the velocity of the boid is not at the max add another nudge to it in its current direction
    }


    private Vector3 Seperation()
    {
        //Look at the boids we have hit nearby
        //Calculate a cumulative change to move away from them
        //Weighted by how close we are to each boid

        Vector3 adjustment = new Vector3(0, 0, 0);

        foreach(GameObject go in nearbyBoids)
        {
            //Whats faster, a second cast at seperate distance or just check the collider for distance
            if (Vector3.Distance(go.GetComponent<Collider>().ClosestPointOnBounds(transform.position), transform.position) <= seperateDistance)
            {
                //Nudge the boid to move away from the nearby boid
                //Take the nearby boids orientation and speed into account if possible
                //TODO: Is there any need to divide the resulting adjustment by the number of boids nearby?

                //Naive predict boids next location 
                Vector3 thatBoidVel = go.GetComponent<Rigidbody>().velocity;
                Vector3 thatBoidsNextPos = go.transform.position + (thatBoidVel * (Time.fixedDeltaTime * futureSight));
                Vector3 closestPoint = NearestPointOnLine(go.transform.position, thatBoidsNextPos, transform.position);

                //Move away from that point - scaled by inverse distance, so closer points have more weight than further ones
                Vector3 CrossProd = Vector3.Cross(transform.position, closestPoint);
                CrossProd.Normalize();
                float inverseDistance = (float)1.0f / Mathf.Abs(Vector3.Distance(transform.position, closestPoint));

                //TODO: How to reduce the number of potential divide by 0's
                if (float.IsNaN(inverseDistance) || float.IsInfinity(inverseDistance) || float.IsNegativeInfinity(inverseDistance))
                {
                    //Error state - skip this
                    continue;
                }
                else
                {
                    Vector3 ScaledAvoidance = CrossProd * inverseDistance;
                    adjustment += ScaledAvoidance;
                }
            }
        }

        return adjustment;
    }

    private Vector3 Alignment()
    {
        //Steer towards the average heading of flockmates

        Vector3 adjustment = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 averageHeading = new Vector3(0.0f, 0.0f, 0.0f);

        foreach(GameObject go in nearbyBoids)
        {
            //Get the heading of each boid
            Vector3 thatBoidVel = go.GetComponent<Rigidbody>().velocity;
            Vector3 thatBoidsNextPos = go.transform.position + (thatBoidVel * (Time.fixedDeltaTime * futureSight));

            Vector3 thatBoidHeading = thatBoidsNextPos - go.transform.position;
            averageHeading += thatBoidHeading;
        }

        adjustment = averageHeading / (float)nearbyBoids.Count;
        Debug.Log(adjustment);

        return adjustment;
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
            nearbyBoids.Add(hit.gameObject);
        }
    }

    //https://stackoverflow.com/a/51906100
    private Vector3 NearestPointOnLine(Vector3 origin, Vector3 end, Vector3 point)
    {
        Vector3 heading = end - origin;
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        Vector3 lhs = point - origin;
        float dotProduct = Vector3.Dot(lhs, heading);
        dotProduct = Mathf.Clamp(dotProduct, 0f, magnitudeMax);
        return origin + heading * dotProduct;
    }

    // Misc.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        foreach (GameObject go in nearbyBoids)
        {
            Gizmos.color = Color.green;

            if (Vector3.Distance(go.GetComponent<Collider>().ClosestPointOnBounds(transform.position), transform.position) <= seperateDistance)
            {
                Gizmos.color = Color.white;
            }
            Gizmos.DrawLine(transform.position, go.transform.position);
        }
    }

}
