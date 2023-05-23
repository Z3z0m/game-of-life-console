using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOfLife : MonoBehaviour
{
    [SerializeField] bool CPU;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] float cellSize = 1f;
    [SerializeField] float updateInterval = 1f;
    [SerializeField] bool startAleatorio;

    private bool[,] grid;
    private bool[,] nextGrid;
    private GameObject[,] cells;
    [SerializeField] bool[,] initialCells;

    private float timer;
    [SerializeField] TextMeshProUGUI text;
    private int runs;

////////////////////////////////////////////////////////////////////////////////////////////
    public ComputeShader gameOfLifeGPU;

    private void Start()
    {
        InitializeGrid();
        CreateCells();
    }

    private void Update()
    {
        if(CPU)
        {
            timer += Time.deltaTime;

            if (timer >= updateInterval)
            {
                UpdateGrid();
                UpdateCells();
                UpdateUI();
                timer = 0f;
            }
        }
    }

    private void InitializeGrid()
    {
        grid = new bool[width, height];
        nextGrid = new bool[width, height];

        // Inicialize a grade com células vivas ou mortas aleatoriamente
        if(startAleatorio)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = Random.Range(0, 2) == 1;
                }
            }
        }
        else
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = initialCells[x, y];
                }
            }
        }
    }

    private void CreateCells()
    {
        cells = new GameObject[width, height];

        // Crie um objeto de célula para cada célula na grade
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

    private void UpdateGrid()
    {
        // Atualize a próxima geração da grade com base nas regras do jogo
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int liveNeighbors = CountLiveNeighbors(x, y);
                bool isAlive = grid[x, y];

                if (isAlive && (liveNeighbors < 2 || liveNeighbors > 3))
                {
                    nextGrid[x, y] = false; // Celula morre por solidão ou superpopulação
                }
                else if (!isAlive && liveNeighbors == 3)
                {
                    nextGrid[x, y] = true; // Nova célula nasce por reprodução
                }
                else
                {
                    nextGrid[x, y] = isAlive; // Célula permanece no mesmo estado
                }
            }
        }

        // Atualize a grade atual com a próxima geração
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = nextGrid[x, y];
            }
        }
    }

    private int CountLiveNeighbors(int x, int y)
    {
        int count = 0;

        // Verifique os vizinhos de uma célula e conte quantos estão vivos
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                int neighborX = x + i;
                int neighborY = y + j;

                // Verifique se o vizinho está dentro dos limites da grade
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (grid[neighborX, neighborY])
                        count++;
                }
            }
        }

        return count;
    }

    private void UpdateCells()
    {
        // Atualize a cor das células com base na grade atualizada
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y].GetComponent<Renderer>().material.color = grid[x, y] ? Color.black : Color.white;
            }
        }
    }
    private void UpdateUI()
    {
        runs++;
        text.GetComponentInChildren<TMP_Text>().text = runs.ToString();
    }
}
