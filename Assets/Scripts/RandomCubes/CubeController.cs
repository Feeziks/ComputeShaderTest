using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    //Public Inputs
    public Camera cam;
    
    [Header("Cube Settings")]
    [Min(1)]
    public int numCubesX;
    [Min(1)]
    public int numCubesY;
    [Min(1)]
    public int iterations;

    [Header("ComputeShader")]
    public ComputeShader shader;

    private GameObject[,] cubes = new GameObject[0,0];
    private Renderer[,] cubesRenderers = new Renderer[0,0];


    public struct Cube
    {
        public Vector3 position;
        public Color color; 
    }

    public void OnCPURandomizeClick()
    {

        //Measure the amount of time this function takes
        float startTime = Time.realtimeSinceStartup;

        //Check if the current cubes array is the right size
        if (cubes.GetLength(0) != numCubesX || cubes.GetLength(1) != numCubesY)
            UpdateCubesArray();
        for (int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < numCubesX; x++)
            {
                for (int y = 0; y < numCubesY; y++)
                {
                    //Randomly Set the color of the cube
                    if (cubesRenderers[x, y] != null)
                        cubesRenderers[x, y].material.SetColor("_BaseColor", Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f));
                    else
                        cubes[x, y].GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV(0f, 1f, 0f, 1f, 0.5f, 1f));
                }
            }
        }

        float endTime = Time.realtimeSinceStartup;
        float runTime = endTime - startTime;
        Debug.Log("Total Run time for CPU To Randomize " + (numCubesX * numCubesY) + " cubes " + iterations + " times: " + runTime);
    }

    public void OnGPURandomizeClick()
    {

        //Measure the amount of time this function takes
        float startTime = Time.realtimeSinceStartup;

        //Create our cube array and compute buffer
        Cube[,] data = new Cube[numCubesX, numCubesY];
        int cubeSize = sizeof(float) * 3 + sizeof(float) * 4;
        ComputeBuffer cubesBuffer = new ComputeBuffer(data.GetLength(0) * data.GetLength(1), cubeSize);
        cubesBuffer.SetData(data);

        shader.SetBuffer(0, "cubes", cubesBuffer);
        shader.SetInt("numCubesX", numCubesX);
        shader.SetInt("numCubesY", numCubesY);
        shader.SetFloat("timeOffset", Time.time);
        shader.SetInt("iterations", iterations);

        shader.Dispatch(0, data.GetLength(0), data.GetLength(1), 1);

        if (cubes.GetLength(0) != numCubesX || cubes.GetLength(1) != numCubesY)
            UpdateCubesArray();

        cubesBuffer.GetData(data);
        
        //Set the gameobjects color to the gpu color output
        for (int x = 0; x < data.GetLength(0); x++)
        {
            for(int y = 0; y < data.GetLength(1); y++)
            {
                if (cubesRenderers[x, y] != null)
                    cubesRenderers[x, y].material.SetColor("_BaseColor", data[x, y].color);
                else
                    cubes[x, y].GetComponent<Renderer>().material.SetColor("_BaseColor", data[x,y].color);
            }
        }
        
        //Release buffer
        cubesBuffer.Release();

        //Measure the amount of time this function takes
        float endTime = Time.realtimeSinceStartup;
        float runTime = endTime - startTime;
        Debug.Log("Total Run time for GPU To Randomize " + (numCubesX * numCubesY) + " cubes " + iterations + " times: " + runTime);

    }

    //Helpers
    private void UpdateCubesArray()
    {
        GameObject[,] newCubes = new GameObject[numCubesX, numCubesY];
        Renderer[,] newRenderer = new Renderer[numCubesX, numCubesY];

        Debug.Log("Resizing Cubes Array ... ");

        for (int x = 0; x < numCubesX; x++)
        {
            for (int y = 0; y < numCubesY; y++)
            {
                if(x < cubes.GetLength(0) && y < cubes.GetLength(1))
                {
                    newCubes[x, y] = cubes[x, y];
                    continue;
                }

                newCubes[x, y] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newCubes[x, y].transform.SetParent(this.transform);
                newCubes[x, y].transform.position = new Vector3(x, y, Random.Range(-0.25f, 0.25f));

                newRenderer[x, y] = newCubes[x, y].GetComponent<Renderer>();
            }
        }

        if (cubes.GetLength(0) > numCubesX)
        {
            for (int x = numCubesX; x < cubes.GetLength(0); x++)
            {
                for (int y = 0; y < cubes.GetLength(1); y++)
                {
                    Destroy(cubes[x, y]);
                }
            }
        }

        if(cubes.GetLength(1) > numCubesY)
        {
            for(int x = 0; x < cubes.GetLength(0); x++)
            {
                for(int y = numCubesY; y < cubes.GetLength(1); y++)
                {
                    Destroy(cubes[x, y]);
                }
            }
        }

        cubes = newCubes;
        cubesRenderers = newRenderer;

        Debug.Log("Cubes array resized to :[" + cubes.GetLength(0) + ", " + cubes.GetLength(1) + "]");

        UpdateCameraPosition();

    }

    private void UpdateCameraPosition()
    {
        //Get the center position x - y
        cam.transform.position = new Vector3(numCubesX / 2, numCubesY / 2, -1.25f * Mathf.Max(numCubesX, numCubesY));
    }
}

