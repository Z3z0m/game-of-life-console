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
    public bool[,] initialCells;

    public ComputeShader gameOfLifeComputeShader;

    public bool[,] grid;
    public bool[,] nextGrid;
    public GameObject[,] cells;
    [SerializeField] private RenderTexture renderTexture;

    private int[] resultData;

    private float timer, timeLimit = 10f;
    private int kernelUpdateGrid;
    public bool CanRun, randomStart = false;

    private void Start()
    {
        InitializeGrid();
        CreateCells();
        InitializeComputeShader();
    }

    private void Update()
    {


        if (CanRun && timer <= timeLimit)
        {
            timer += Time.deltaTime;
            UpdateGPU();
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
        resultData = new int[bufferSize];
        ComputeBuffer gridBuffer = new ComputeBuffer(bufferSize, sizeof(float));
        ComputeBuffer nextGridBuffer = new ComputeBuffer(bufferSize, sizeof(float));
        ComputeBuffer resultBuffer = new ComputeBuffer(bufferSize, sizeof(float));

        kernelUpdateGrid = gameOfLifeComputeShader.FindKernel("UpdateGrid");
        gameOfLifeComputeShader.SetInt("width", width);
        gameOfLifeComputeShader.SetInt("height", height);
        gameOfLifeComputeShader.SetBuffer(0, "grid", gridBuffer);
        gameOfLifeComputeShader.SetBuffer(0, "nextGrid", nextGridBuffer);
        gameOfLifeComputeShader.SetBuffer(0, "Result", resultBuffer);
        gameOfLifeComputeShader.SetTexture(kernelUpdateGrid, "Result", renderTexture);
        gameOfLifeComputeShader.Dispatch(0, width / 10, height / 10, 1);
        
        resultBuffer.GetData(resultData);


        for (int i =0; i < bufferSize; i++)
        {
            int incrementedValue = resultData[i];
            Debug.Log("Resultado para a célula (" + i + ", " + "): " + incrementedValue);
        }

        gridBuffer.Dispose();
        nextGridBuffer.Dispose();
        resultBuffer.Dispose();
    }


    void UpdateGPU()
    {
        int bufferSize = width * height;
        ComputeBuffer gridBuffer = new ComputeBuffer(bufferSize, sizeof(float));
        ComputeBuffer nextGridBuffer = new ComputeBuffer(bufferSize, sizeof(float));
        ComputeBuffer resultBuffer = new ComputeBuffer(bufferSize, sizeof(float));

        kernelUpdateGrid = gameOfLifeComputeShader.FindKernel("UpdateGrid");
        gameOfLifeComputeShader.SetInt("width", width);
        gameOfLifeComputeShader.SetInt("height", height);
        gameOfLifeComputeShader.SetBuffer(0, "grid", gridBuffer);
        gameOfLifeComputeShader.SetBuffer(0, "nextGrid", nextGridBuffer);
        gameOfLifeComputeShader.SetBuffer(0, "Result", resultBuffer);

        gameOfLifeComputeShader.SetTexture(kernelUpdateGrid, "Result", renderTexture);
        gameOfLifeComputeShader.Dispatch(0, width / 10, height / 10, 1);

        resultBuffer.GetData(resultData);

        for (int i = 0; i < bufferSize; i++)
        {
            int incrementedValue = resultData[i];
            Debug.Log("Resultado para a célula (" + i + ", " + "): " + incrementedValue);
        }



        gridBuffer.Dispose();
        nextGridBuffer.Dispose();
        resultBuffer.Dispose();

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

        Destroy(texture);
    }


}