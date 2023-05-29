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
    public float updateInterval = 1f;
    public bool[,] initialCells;

    public ComputeShader gameOfLifeComputeShader;

    private bool[,] grid;
    private bool[,] nextGrid;
    private GameObject[,] cells;
    private RenderTexture renderTexture;

    private float timer;
    private int kernelUpdateGrid;

    private void Start()
    {
        InitializeGrid();
        CreateCells();
        InitializeComputeShader();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            UpdateGrid();
            UpdateCells();
            timer = 0f;
        }
    }

    private void InitializeGrid()
    {
        grid = new bool[width, height];
        nextGrid = new bool[width, height];

        if (initialCells != null && initialCells.GetLength(0) == width && initialCells.GetLength(1) == height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = initialCells[x, y];
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
                cell.transform.position = new Vector3(x * cellSize, 0f, y * cellSize);
                cell.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                cell.GetComponent<Renderer>().material.color = grid[x, y] ? Color.black : Color.white;

                cells[x, y] = cell;
            }
        }
    }

    private void InitializeComputeShader()
    {
        renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RInt);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        kernelUpdateGrid = gameOfLifeComputeShader.FindKernel("UpdateGrid");
        gameOfLifeComputeShader.SetInt("width", width);
        gameOfLifeComputeShader.SetInt("height", height);
        gameOfLifeComputeShader.SetTexture(kernelUpdateGrid, "grid", renderTexture);
        gameOfLifeComputeShader.SetTexture(kernelUpdateGrid, "nextGrid", renderTexture);
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
