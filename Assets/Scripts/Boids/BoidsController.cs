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


}
