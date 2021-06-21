using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ComputeShaderUtility;

public class GameOfLife : MonoBehaviour
{
    //public members
    public List<GameObject> RenderTargets;
    public TMP_Text fpsText;
    public Toggle GPUToggle;

    public ComputeShader shader;

    //private members
    private const int sizeX = 200;
    private const int sizeY = 200;

    private List<Texture2D> gameTextures;

    struct Cell
    {
        public Vector2 position;
        public int state;
        public int nextState;

        public int idx;
        public int idy;
        public int idz;
    }

    private const int cellSize = sizeof(float) * 2 + sizeof(int) * 2 + sizeof(int) * 3;

    private List<Cell[,]> cells;

    private const int deadState = 0;
    private const int aliveState = 1;

    static private Color deadColor = Color.black;
    static private Color aliveColor = Color.white;
    private Color[] colorsIdxByState = { deadColor, aliveColor };

    private void Awake()
    {
        cells = new List<Cell[,]>(RenderTargets.Count);
        gameTextures = new List<Texture2D>(RenderTargets.Count);

        for(int i = 0; i < RenderTargets.Count; i++)
        {
            cells.Add(new Cell[sizeX, sizeY]);
            gameTextures.Add(new Texture2D(sizeX, sizeY));
            RenderTargets[i].GetComponent<Renderer>().material.mainTexture = gameTextures[i];
        }

        //Randomizes EVERY game
        RandomizeCells();
    }

    private void Update()
    {
        float frameTime = 1.0f / Time.deltaTime;
        fpsText.text = "FPS: " + Mathf.Ceil(frameTime).ToString();
        if (GPUToggle.isOn)
        {
            //Switch to using the GPU
            TimeStepGPU();
        }
        else
        {
            //Switch to using CPU
            TimeStepCPU();
        }
        ApplyToTextures();
    }

    //Helpers
    private void RandomizeCells()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    cells[i][x, y].position = new Vector2(x, y);
                    cells[i][x, y].state = Mathf.RoundToInt(Random.Range(0.0f, 1.0f));
                }
            }
        }
    }

    private void TimeStepGPU()
    {
        //"Unrolling" the loop so we dont need to wait for dispatches to finish
        ComputeBuffer[] cellBuffers = new ComputeBuffer[6]       

        ComputeUtils.CreateStructuredBuffer<Cell>(ref cellBuffers[0], sizeX * sizeY);
        ComputeUtils.CreateStructuredBuffer(ref cellBuffers[1], sizeX * sizeY, cellSize);

        //cellBuffers[0] = new ComputeBuffer(sizeX * sizeY, cellSize);
        //cellBuffers[1] = new ComputeBuffer(sizeX * sizeY, cellSize);
        cellBuffers[2] = new ComputeBuffer(sizeX * sizeY, cellSize);
        cellBuffers[3] = new ComputeBuffer(sizeX * sizeY, cellSize);
        cellBuffers[4] = new ComputeBuffer(sizeX * sizeY, cellSize);
        cellBuffers[5] = new ComputeBuffer(sizeX * sizeY, cellSize);

        cellBuffers[0].SetData(cells[0]);
        cellBuffers[1].SetData(cells[1]);
        cellBuffers[2].SetData(cells[2]);
        cellBuffers[3].SetData(cells[3]);
        cellBuffers[4].SetData(cells[4]);
        cellBuffers[5].SetData(cells[5]);

        int gameOfLifeKernelNumber = shader.FindKernel("GameOfLife");

        //Create instances of the shader? idk if this is a thing or not
        ComputeShader[] shaderInstances = new ComputeShader[6];

        shaderInstances[0] = shader;
        shaderInstances[1] = shader;
        shaderInstances[2] = shader;
        shaderInstances[3] = shader;
        shaderInstances[4] = shader;
        shaderInstances[5] = shader;

        shaderInstances[0].SetBuffer(gameOfLifeKernelNumber, "cells", cellBuffers[0]);
        shaderInstances[1].SetBuffer(gameOfLifeKernelNumber, "cells", cellBuffers[1]);
        shaderInstances[2].SetBuffer(gameOfLifeKernelNumber, "cells", cellBuffers[2]);
        shaderInstances[3].SetBuffer(gameOfLifeKernelNumber, "cells", cellBuffers[3]);
        shaderInstances[4].SetBuffer(gameOfLifeKernelNumber, "cells", cellBuffers[4]);
        shaderInstances[5].SetBuffer(gameOfLifeKernelNumber, "cells", cellBuffers[5]);

        shaderInstances[0].SetInt("sizeX", sizeX);
        shaderInstances[1].SetInt("sizeX", sizeX);
        shaderInstances[2].SetInt("sizeX", sizeX);
        shaderInstances[3].SetInt("sizeX", sizeX);
        shaderInstances[4].SetInt("sizeX", sizeX);
        shaderInstances[5].SetInt("sizeX", sizeX);

        shaderInstances[0].SetInt("sizeY", sizeY);
        shaderInstances[1].SetInt("sizeY", sizeY);
        shaderInstances[2].SetInt("sizeY", sizeY);
        shaderInstances[3].SetInt("sizeY", sizeY);
        shaderInstances[4].SetInt("sizeY", sizeY);
        shaderInstances[5].SetInt("sizeY", sizeY);

        int numKernels = Mathf.Max(1, Mathf.CeilToInt((sizeX * sizeY) / (16 * 16)));

        shaderInstances[0].Dispatch(gameOfLifeKernelNumber, numKernels, 1, 1);
        shaderInstances[1].Dispatch(gameOfLifeKernelNumber, numKernels, 1, 1);
        shaderInstances[2].Dispatch(gameOfLifeKernelNumber, numKernels, 1, 1);
        shaderInstances[3].Dispatch(gameOfLifeKernelNumber, numKernels, 1, 1);
        shaderInstances[4].Dispatch(gameOfLifeKernelNumber, numKernels, 1, 1);
        shaderInstances[5].Dispatch(gameOfLifeKernelNumber, numKernels, 1, 1);

        cellBuffers[0].GetData(cells[0]);
        cellBuffers[1].GetData(cells[1]);
        cellBuffers[2].GetData(cells[2]);
        cellBuffers[3].GetData(cells[3]);
        cellBuffers[4].GetData(cells[4]);
        cellBuffers[5].GetData(cells[5]);

        cellBuffers[0].Release();
        cellBuffers[1].Release();
        cellBuffers[2].Release();
        cellBuffers[3].Release();
        cellBuffers[4].Release();
        cellBuffers[5].Release();
        

        for (int i = 0; i < cells.Count; i++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    cells[i][x, y].state = cells[i][x, y].nextState;
                    Debug.Log(cells[i][x, y].idx + " " + cells[i][x, y].idy + " " + cells[i][x, y].idz);
                }
            }
        }
    }

    private void TimeStepCPU()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int numAliveNeighbors = 0;
                    if(x == 0 || x == sizeX - 1)
                    {
                        cells[i][x, y].nextState = deadState;
                        continue;
                    }
                    if(y == 0 || y == sizeY - 1)
                    {
                        cells[i][x, y].nextState = deadState;
                        continue;
                    }

                    //Get the state of all 8 neighbors
                    numAliveNeighbors += cells[i][x, y - 1].state;
                    numAliveNeighbors += cells[i][x + 1, y - 1].state;
                    numAliveNeighbors += cells[i][x + 1, y].state;
                    numAliveNeighbors += cells[i][x + 1, y + 1].state;
                    numAliveNeighbors += cells[i][x, y + 1].state;
                    numAliveNeighbors += cells[i][x - 1, y + 1].state;
                    numAliveNeighbors += cells[i][x - 1, y].state;
                    numAliveNeighbors += cells[i][x - 1, y - 1].state;

                    //Change our next state based on that number of neighbors
                    if (cells[i][x, y].state == aliveState)
                    {
                        if (numAliveNeighbors == 2 || numAliveNeighbors == 3)
                        {
                            cells[i][x, y].nextState = aliveState;
                        }
                        else
                        {
                            cells[i][x, y].nextState = deadState;
                        }
                    }
                    else
                    {
                        if (numAliveNeighbors == 3)
                        {
                            cells[i][x, y].nextState = aliveState;
                        }
                        else
                        {
                            cells[i][x, y].nextState = deadState;
                        }
                    }
                    
                }
            }
        }

        for (int i = 0; i < cells.Count; i++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    cells[i][x, y].state = cells[i][x, y].nextState;
                }
            }
        }
    }

    private void ApplyToTextures()
    {
        for(int i = 0; i < gameTextures.Count; i++)
        {
            for (int x = sizeX - 1; x >= 0; x--)
            {
                for (int y = sizeY - 1; y >= 0; y--)
                {
                    gameTextures[i].SetPixel(x, y, colorsIdxByState[cells[i][x, y].state]);
                }
            }
            gameTextures[i].Apply();
        }
    }

    public void OnRandomizePress()
    {
        RandomizeCells();
    }
}
