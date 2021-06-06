using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Boid Setting", menuName = "ScriptableObjects/BoidSetting", order = 1)]
public class BoidsSettings : ScriptableObject
{
    public float maxVelocityMagnitude;
    public float speedUp;

    public float alignmentPower;
    public float cohesionPower;

    public float seperationSliderValue;
    public float alignmentSliderValue;
    public float cohesionSliderValue;

    public float seperationWeight;
    public float alignmentWeight;
    public float cohesionWeight;
    public float obstacleWeight;

    public float futureSight;
    public float viewDistance;
    public float sperationViewDistance;
}
