using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{

    //Public Members
    public BoidsSettings settings;

    //Private members
    private List<GameObject> nearbyBoids;
    private List<GameObject> nearbyObstacles;
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
        GetNearbyBoidsObstacles();

        Vector3 sperationAccel = Seperation();

        Vector3 alignmentAccel = Alignment();

        Vector3 cohesionAccel = Cohesion();

        Vector3 obstacleAceel = AvoidObstacles();

        //Sum all of the accelerations according to some weights
        DAccel += sperationAccel * settings.seperationWeight;
        DAccel += alignmentAccel * settings.alignmentWeight;
        DAccel += cohesionAccel * settings.cohesionWeight;
        DAccel += obstacleAceel * settings.obstacleWeight;

        //TODO: How / When to change the orientation of the boid so it continues to move "forward"

        //Apply this acceleration to the boid as a force
        myBody.AddForce(DAccel, ForceMode.Acceleration);

        //If the velocity of the boid is not at the max add another nudge to it in its current direction
        /*
        if(myBody.velocity.magnitude < settings.maxVelocityMagnitude)
        {
            myBody.AddForce(transform.up * settings.speedUp);
        }
        */
        //Otherwise clamp the boid's velocity
        if(myBody.velocity.magnitude > settings.maxVelocityMagnitude)
        {
            myBody.velocity = Vector3.ClampMagnitude(myBody.velocity, settings.maxVelocityMagnitude);
        }

        //TODO:Rotate the boid in the direction of travel
        myBody.MoveRotation(Quaternion.LookRotation(myBody.velocity.normalized));
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
            if (Vector3.Distance(go.GetComponent<Collider>().ClosestPointOnBounds(transform.position), transform.position) <= settings.sperationViewDistance)
            {
                //Nudge the boid to move away from the nearby boid
                //Take the nearby boids orientation and speed into account if possible
                //TODO: Is there any need to divide the resulting adjustment by the number of boids nearby?

                //Naive predict boids next location 
                Vector3 thatBoidVel = go.GetComponent<Rigidbody>().velocity;
                Vector3 thatBoidsNextPos = go.transform.position + (thatBoidVel * (Time.fixedDeltaTime * settings.futureSight));
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
            Vector3 thatBoidsNextPos = go.transform.position + (thatBoidVel * (Time.fixedDeltaTime * settings.futureSight));

            Vector3 thatBoidHeading = thatBoidsNextPos - go.transform.position;
            averageHeading += thatBoidHeading;
        }
        if (nearbyBoids.Count != 0)
        {
            averageHeading /= (float)nearbyBoids.Count;
        }

        //This was wrong previously, need to make the adjustment align this boid with the average heading not just assign it the average heading
        //Probably why we saw adjustment dominate the seperation
        adjustment = averageHeading - transform.position;
        adjustment.Normalize();
        adjustment *= settings.alignmentPower;

        return adjustment;
    }

    private Vector3 Cohesion()
    {
        //Steer towards the center of mass of flockmates 

        Vector3 adjustment = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 centerOfMass = new Vector3(0.0f, 0.0f, 0.0f);

        foreach(GameObject go in nearbyBoids)
        {
            centerOfMass += go.transform.position;
        }

        if(nearbyBoids.Count != 0)
        {
            centerOfMass /= nearbyBoids.Count;
        }

        adjustment = centerOfMass - transform.position;
        adjustment.Normalize();
        adjustment *= settings.cohesionPower;

        return adjustment;
    }

    private Vector3 AvoidObstacles()
    {
        Vector3 adjustment = new Vector3(0.0f, 0.0f, 0.0f);
        //Avoid all the obstacles in the boids path
        foreach(GameObject go in nearbyObstacles)
        {
            Vector3 obstacleVel = go.GetComponent<Rigidbody>().velocity;
            Vector3 obstacleNextPos = go.transform.position + (obstacleVel * (Time.fixedDeltaTime * settings.futureSight));
            Vector3 closestPoint = NearestPointOnLine(go.transform.position, obstacleNextPos, transform.position);

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
        return adjustment;
    }

    // Helpers

    private void GetNearbyBoidsObstacles()
    {
        nearbyBoids = new List<GameObject>();
        nearbyObstacles = new List<GameObject>();

        //Cast a sphere of radius max view distance and find all collisions
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, settings.viewDistance);

        foreach(Collider hit in hitColliders)
        {
            //Sort of bad, since we can ensure the objects we create are only on the one layer this should work
            //Otherwise if the gameobject had multiple layers we should only check that individual bit like this
            //if(hit.gameObject.layer & ( 1 << layerNumber) == (1 << layerNumber))
            if (hit.gameObject.layer == LayerMask.NameToLayer("Boid_Layer"))
            {
                nearbyBoids.Add(hit.gameObject);
            }

            if(hit.gameObject.layer == LayerMask.NameToLayer("Obstacle_Layer"))
            {
                nearbyObstacles.Add(hit.gameObject);
            }
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
        Gizmos.DrawWireSphere(transform.position, settings.viewDistance);

        foreach (GameObject go in nearbyBoids)
        {
            Gizmos.color = Color.green;

            if (Vector3.Distance(go.GetComponent<Collider>().ClosestPointOnBounds(transform.position), transform.position) <= settings.sperationViewDistance)
            {
                Gizmos.color = Color.white;
            }
            Gizmos.DrawLine(transform.position, go.transform.position);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, (DAccel * settings.futureSight));
    }

}
