using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOfLife1 : MonoBehaviour
{
    public int width = 16;
    public int height = 9;
    public float cellSize = 1f;
    public GameObject cellExemple;
    public bool[,] initialCells;

    public ComputeShader gameOfLifeComputeShader;

    public bool[,] grid;
    public bool[,] nextGrid;
    public GameObject[,] cells;
    private RenderTexture renderTexture;

    private float timer, timeLimit;
    private int kernelUpdateGrid;
    public bool CanRun, randomStart = false;
    private void Start()
    {
        InitializeGrid();
        CreateCells();
        //InitializeComputeShader();
    }

    private void Update()
    {


        if (CanRun && timer >= timeLimit)
        {
            timer += Time.deltaTime;
            UpdateGrid();
            UpdateCells();
        }
    }


    private void InitializeGrid()
    {
        grid = new bool[width, height];
        nextGrid = new bool[width, height];

        if (randomStart == false)
        {

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = false;
                }
            }
        }
        else
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = Random.Range(0, 2) == 1;
                }
            }
        }
    }

    private void CreateCells()
    {
        cells = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cell.AddComponent<ClickRecognizer>();
                cell.transform.position = new Vector3(x * cellSize, 0f, y * cellSize);
                cell.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                cell.GetComponent<Renderer>().material.color = grid[x, y] ? Color.black : Color.white;

                cells[x, y] = cell;
            }
        }
    }

    private void InitializeComputeShader()
    {
        int bufferSize = width * height;
        ComputeBuffer gridBuffer = new ComputeBuffer(bufferSize, sizeof(float));
        ComputeBuffer nextGridBuffer = new ComputeBuffer(bufferSize, sizeof(float));

        kernelUpdateGrid = gameOfLifeComputeShader.FindKernel("UpdateGrid");
        gameOfLifeComputeShader.SetInt("width", width);
        gameOfLifeComputeShader.SetInt("height", height);
        gameOfLifeComputeShader.SetBuffer(kernelUpdateGrid, "grid", gridBuffer);
        gameOfLifeComputeShader.SetBuffer(kernelUpdateGrid, "nextGrid", nextGridBuffer);

        gameOfLifeComputeShader.SetTexture(kernelUpdateGrid, "Result", renderTexture);
        gameOfLifeComputeShader.Dispatch(kernelUpdateGrid, width / 8, height / 8, 1);

        gridBuffer.Release();
        nextGridBuffer.Release();
    }


    private void UpdateGrid()
    {
        gameOfLifeComputeShader.Dispatch(kernelUpdateGrid, width / 8, height / 8, 1);
    }

    private void UpdateCells()
    {
        RenderTexture.active = renderTexture;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RFloat, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        Color[] pixels = texture.GetPixels();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = y * width + x;
                grid[x, y] = pixels[index].r == 1f;
                cells[x, y].GetComponent<Renderer>().material.color = grid[x, y] ? Color.black : Color.white;
            }
        }

        RenderTexture.active = null;
        Destroy(texture);
    }

}
