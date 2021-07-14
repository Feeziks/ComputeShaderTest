using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Boid Setting", menuName = "ScriptableObjects/ObstacleSetting", order = 2)]
public class ObstacleSettings : ScriptableObject
{

    public float obstacleFreq;
    public float maxNumObstacles;
    public float obstacleVelocity;
    public float obstacleLifeTime;

}
