using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfLife : MonoBehaviour
{
    //public members
    public GameObject RenderTarget;

    //private members
    private const int sizeX = 1920;
    private const int sizeY = 1080;
    private Texture2D GameTexture;

    struct Cell
    {
        public Vector2 position;
        public int state;
        public int nextState;
    }

    private Cell[,] cells;

    private const int deadState = 0;
    private const int aliveState = 1;

    static private Color deadColor = Color.black;
    static private Color aliveColor = Color.white;
    private Color[] colorsIdxByState = { deadColor, aliveColor };

    private void Start()
    {
        cells = new Cell[sizeX, sizeY];
        RandomizeCells();

        GameTexture = new Texture2D(sizeX, sizeY);
        RenderTarget.GetComponent<Renderer>().material.SetTexture("_mainTex", GameTexture);
    }

    private void Update()
    {
        TimeStep();
        ApplyToTexture();
    }

    //Helpers
    private void RandomizeCells()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                cells[x, y].position = new Vector2(x, y);
                cells[x, y].state = (int)Random.Range(0.0f, 1.0f);
            }
        }
    }

    private void TimeStep()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                int numAliveNeighbors = 0;
                if (x == 0 || x == sizeX - 1)
                    continue;
                if (y == 0 || y == sizeY - 1)
                    continue;

                //Get the state of all 8 neighbors
                numAliveNeighbors += cells[x, y - 1].state;
                numAliveNeighbors += cells[x + 1, y - 1].state;
                numAliveNeighbors += cells[x + 1, y].state;
                numAliveNeighbors += cells[x + 1, y + 1].state;
                numAliveNeighbors += cells[x, y + 1].state;
                numAliveNeighbors += cells[x - 1, y + 1].state;
                numAliveNeighbors += cells[x - 1, y].state;
                numAliveNeighbors += cells[x - 1, y - 1].state;

                //Change our next state based on that number of neighbors
                if (cells[x, y].state == aliveState)
                {
                    if (numAliveNeighbors == 2 || numAliveNeighbors == 3)
                    {
                        cells[x, y].nextState = aliveState;
                    }
                    else
                    {
                        cells[x, y].nextState = deadState;
                    }
                }
                else
                {
                    if (numAliveNeighbors == 3)
                    {
                        cells[x, y].nextState = aliveState;
                    }
                    else
                    {
                        cells[x, y].nextState = deadState;
                    }
                }
            }
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                cells[x, y].state = cells[x, y].nextState;
            }
        }
    }

    private void ApplyToTexture()
    {
        for(int x = sizeX - 1; x >= 0; x--)
        {
            for(int y = sizeY - 1; y >= 0; y--)
            {
                GameTexture.SetPixel(x, y, colorsIdxByState[cells[x, y].state]);
            }
        }

        
    }
}
