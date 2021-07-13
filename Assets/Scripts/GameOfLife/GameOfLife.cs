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
    }

    private List<Cell[,]> cells;

    private const int deadState = 0;
    private const int aliveState = 1;

    static private Color deadColor = Color.black;
    static private Color aliveColor = Color.white;
    private Color[] colorsIdxByState = { deadColor, aliveColor };

    //Compute shader members
    private ComputeShader[] shaders;
    private RenderTexture[] renderTexturesPing;
    private RenderTexture[] renderTexturesPong;
    private bool PingPong = true;

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


        //Create GPU Compute Buffers
        shaders = new ComputeShader[RenderTargets.Count];
        renderTexturesPing = new RenderTexture[RenderTargets.Count];
        renderTexturesPong = new RenderTexture[RenderTargets.Count];

        for (int i = 0; i < RenderTargets.Count; i++)
        {
            shaders[i] = shader;
            ComputeUtils.CreateRenderTexture(ref renderTexturesPing[i], sizeX, sizeY, 24);
            ComputeUtils.CreateRenderTexture(ref renderTexturesPong[i], sizeX, sizeY, 24);
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
            ApplyToTextures();
        }
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

        if(GPUToggle.isOn)
        {
            //Write the cell values to the texture and then copy the textures to render textures
            ApplyToTextures();
            for (int i = 0; i < gameTextures.Count; i++)
            {
                ComputeUtils.CopyToRenderTexture(gameTextures[i], renderTexturesPing[i]);
                ComputeUtils.CopyToRenderTexture(gameTextures[i], renderTexturesPong[i]);
            }
        }
    }

    private void TimeStepGPU()
    {
        for (int i = 0; i < RenderTargets.Count; i++)
        {
            if (PingPong)
            {
                shaders[i].SetTexture(0, "Input", renderTexturesPing[i]);
                shaders[i].SetTexture(0, "Result", renderTexturesPong[i]);
            }
            else 
            {
                shaders[i].SetTexture(0, "Input", renderTexturesPong[i]);
                shaders[i].SetTexture(0, "Result", renderTexturesPing[i]);
            }
            shaders[i].SetFloat("sizeX", sizeX);
            shaders[i].SetFloat("sizeY", sizeY);
            //Dispatch the kernels
            shaders[i].Dispatch(0, Mathf.CeilToInt(sizeX / 16f), Mathf.CeilToInt(sizeY / 16f), 1);
            
        }

        //Get the data of each kernel and apply it to our 2d textures
        for(int i = 0; i < RenderTargets.Count; i++)
        {
            //Apply the rendertexture to our texture2D's on the objects
            if (PingPong)
            {
                //ComputeUtils.CopyRenderTextureToTexture2D(renderTexturesPong[i], gameTextures[i]);
                RenderTargets[i].GetComponent<Renderer>().material.mainTexture = renderTexturesPong[i];
            }
            else
            {
                //ComputeUtils.CopyRenderTextureToTexture2D(renderTexturesPing[i], gameTextures[i]);
                RenderTargets[i].GetComponent<Renderer>().material.mainTexture = renderTexturesPing[i];
            }
        }

        PingPong = !PingPong;
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

    public void OnGpuToggleChange()
    {
        if (GPUToggle.isOn)
        {
            for (int i = 0; i < gameTextures.Count; i++)
            {
                ComputeUtils.CopyToRenderTexture(gameTextures[i], renderTexturesPing[i]);
                ComputeUtils.CopyToRenderTexture(gameTextures[i], renderTexturesPong[i]);
            }
        }
        else
        {
            for(int i =0; i < RenderTargets.Count; i++)
            {
                RenderTargets[i].GetComponent<Renderer>().material.mainTexture = gameTextures[i];
            }
        }
    }
}
