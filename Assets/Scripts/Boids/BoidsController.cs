using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoidsController : MonoBehaviour
{
    //Public 
    [Header("Flock Parameters")]
    [Min(1)]
    public Slider numBoidsSlider;
    public TMP_Text numBoidsText;
    public BoidsSettings settings;

    [Header("Individual Parameters")]
    public float maxVel;
    public float maxAccel;
    public float maxVisionDistance;
    public float maxTurnSpeed;


    //Private
    private List<GameObject> boids;


    void Start()
    {
        boids = new List<GameObject>();

        SpawnBoids((int)numBoidsSlider.value);
    }

    void Update()
    {
        //Start a timer for spawning obstalces
    }

    public void OnSliderValueChanged()
    {
        if (boids.Count > numBoidsSlider.value)
        {
            DeleteBoids((int)(boids.Count - numBoidsSlider.value));
        }
        else if (boids.Count < numBoidsSlider.value)
        {
            SpawnBoids((int)(numBoidsSlider.value - boids.Count));
        }
        numBoidsText.text = numBoidsSlider.value.ToString();
    }

    //TODO: Tries to delete a negative index? or something like that
    private void DeleteBoids(int numToDelete)
    {
        for(int i = 0; i < numToDelete; i++)
        {
            GameObject boidToRemove = boids[boids.Count - 1];
            boids.RemoveAt(boids.Count - 1);
            Destroy(boidToRemove);
        }
    }

    private void SpawnBoids(int numToSpawn)
    {
        for(int i = 0; i < numToSpawn; i++)
        {
            GameObject thisBoid = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            thisBoid.AddComponent(typeof(Boid)); //Add our custom script to this new object
            thisBoid.GetComponent<Boid>().settings = settings;
            thisBoid.AddComponent(typeof(Rigidbody));
            thisBoid.GetComponent<Rigidbody>().useGravity = false; // Float
            thisBoid.transform.SetParent(transform);
            thisBoid.transform.position = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f)); //Place the boid randomly
            thisBoid.transform.rotation = Random.rotation; //Have the boid face a random direction
            thisBoid.GetComponent<Rigidbody>().AddForce(thisBoid.transform.up * Random.Range(0.0f, 100.0f), ForceMode.Acceleration); //Kick the boid
            thisBoid.layer = LayerMask.NameToLayer("Boid_Layer");
            thisBoid.name = "Boid_" + boids.Count;
            boids.Add(thisBoid);
        }    
    }

}
