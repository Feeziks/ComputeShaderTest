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
    public Slider SeperationSlider;
    public Slider AlignmentSlider;
    public Slider CohesionSlider;
    public Toggle GPUToggle;

    public TMP_Text numBoidsText;
    public BoidsSettings settings;

    public GameObject obstacleControllerGO;

    public ComputeShader boidComputeShader;

    //Private
    private List<GameObject> boids;

    private boidSettingsShader[] settingsShader = new boidSettingsShader[1];
    private boidShader[] computeShaderBoids;
    private obstacleShader[] computeShaderObstacles;

    private ObstacleController obstacles;


    //Structs to use for the compute shader
    struct boidShader
    {
        public Vector3 pos;
        public Vector3 vel;
        public Vector3 adjustment;
    };

    const int boidShaderSize = sizeof(float) * 3 * 3;

    struct obstacleShader
    {
        public Vector3 pos;
        public Vector3 vel;
    };

    const int obstacleShaderSize = sizeof(float) * 3 * 2;

    const int boidSettingsShaderSize = sizeof(float) * 13;

    private void Awake()
    {
        boids = new List<GameObject>();

        SpawnBoids((int)numBoidsSlider.value);

        obstacles = obstacleControllerGO.GetComponent<ObstacleController>();
    }

    void Start()
    {
        //Casting the class to the struct would probably work but I did not want to risk introducing a difficult to track bug that way
        settingsShader[0].maxVelocityMagnitude     = settings.maxVelocityMagnitude;
        settingsShader[0].alignmentPower           = settings.alignmentPower;
        settingsShader[0].cohesionPower            = settings.cohesionPower;
        settingsShader[0].seperationSliderValue    = settings.seperationSliderValue;
        settingsShader[0].alignmentSliderValue     = settings.alignmentSliderValue;
        settingsShader[0].cohesionSliderValue      = settings.cohesionSliderValue;
        settingsShader[0].seperationWeight         = settings.seperationWeight;
        settingsShader[0].alignmentWeight          = settings.alignmentWeight;
        settingsShader[0].cohesionWeight           = settings.cohesionWeight;
        settingsShader[0].obstacleWeight           = settings.obstacleWeight;
        settingsShader[0].futureSight              = settings.futureSight;
        settingsShader[0].viewDistance             = settings.viewDistance;
        settingsShader[0].sperationViewDistance    = settings.sperationViewDistance;

        UpdateComputeShaderStructs();
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //Use the GPU Shader if it is enabled
        if(GPUToggle.isOn)
        {
            ComputeBuffer boidsBuffer = new ComputeBuffer(computeShaderBoids.Length, boidShaderSize);
            ComputeBuffer settingsBuffer = new ComputeBuffer(1, boidSettingsShaderSize);
            ComputeBuffer obstacleBuffer = null;

            boidsBuffer.SetData(computeShaderBoids);
            settingsBuffer.SetData(settingsShader);

            int boidKernelNumber = boidComputeShader.FindKernel("Boid");

            boidComputeShader.SetBuffer(boidKernelNumber, "boids", boidsBuffer);
            boidComputeShader.SetBuffer(boidKernelNumber, "settings", settingsBuffer);

            boidComputeShader.SetInt("numBoids", boids.Count);
            boidComputeShader.SetInt("numObstacles", obstacles.obstacles.Count);
            boidComputeShader.SetFloat("fixedDeltaTime", Time.fixedDeltaTime);


            if (computeShaderObstacles.Length > 0)
            {
                obstacleBuffer = new ComputeBuffer(computeShaderObstacles.Length, obstacleShaderSize);
                obstacleBuffer.SetData(computeShaderObstacles);
                boidComputeShader.SetBuffer(boidKernelNumber, "obstacles", obstacleBuffer);
            }

            boidComputeShader.Dispatch(boidKernelNumber, 1, 1, 1);

            boidsBuffer.GetData(computeShaderBoids);

            boidsBuffer.Release();
            settingsBuffer.Release();

            if(computeShaderObstacles.Length > 0)
            {
                obstacleBuffer.Release();
            }

            //Apply the adjustments to each boid
            for(int i = 0; i < boids.Count; i++)
            {
                if (VerifyShaderOutput(computeShaderBoids[i].adjustment))
                {
                    Debug.Log(computeShaderBoids[i].adjustment);
                    boids[i].GetComponent<Rigidbody>().AddForce(computeShaderBoids[i].adjustment, ForceMode.Acceleration);
                    boids[i].GetComponent<Rigidbody>().velocity = Vector3.ClampMagnitude(boids[i].GetComponent<Rigidbody>().velocity, settings.maxVelocityMagnitude);
                }
            }
        }
    }

    public void OnGPUToggleChange()
    {
        //Get the new value
        if(GPUToggle.isOn)
        {
            //Switch to using the GPU Shader
            foreach(GameObject go in boids)
            {
                go.GetComponent<Boid>().enabled = false;
            }
        }
        else
        {
            //Switch to using the CPU
            foreach(GameObject go in boids)
            {
                go.GetComponent<Boid>().enabled = true;
            }
        }
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

        //Update values for the compute shader
        UpdateComputeShaderStructs();
    }

    public void OnWeightSliderValueChanged()
    {
        settings.seperationSliderValue = SeperationSlider.value;
        settings.alignmentSliderValue = AlignmentSlider.value;
        settings.cohesionSliderValue = CohesionSlider.value;
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

            if(GPUToggle.isOn)
            {
                thisBoid.GetComponent<Boid>().enabled = false;
            }
        }    
    }

    private void UpdateComputeShaderStructs()
    {
        computeShaderBoids = new boidShader[(int)numBoidsSlider.value];
        computeShaderObstacles = new obstacleShader[obstacles.obstacles.Count];

        //Fill in the data
        for(int i = 0; i < computeShaderBoids.Length; i++)
        {
            computeShaderBoids[i].pos = boids[i].transform.position;
            computeShaderBoids[i].vel = boids[i].GetComponent<Rigidbody>().velocity;
            computeShaderBoids[i].adjustment = new Vector3(0.0f, 0.0f, 0.0f);
        }

        for (int i = 0; i < computeShaderObstacles.Length; i++)
        {
            computeShaderObstacles[i].pos = obstacles.obstacles[i].transform.position;
            computeShaderObstacles[i].vel = obstacles.obstacles[i].GetComponent<Rigidbody>().velocity;
        }
    }

    private bool VerifyShaderOutput(Vector3 adjustment)
    {
        if(float.IsNaN(adjustment.x) || float.IsNaN(adjustment.y) || float.IsNaN(adjustment.z) ||
           float.IsInfinity(adjustment.x) || float.IsInfinity(adjustment.y) || float.IsInfinity(adjustment.z))
        {
            return false;
        }

        return true;
    }
}
