using UnityEngine;
using TMPro; // For TextMeshPro UI

public class TerrainDigger : MonoBehaviour
{
    public Terrain terrain;  // Assign your Terrain in the Inspector
    public Vector2Int digAreaStart = new Vector2Int(50, 50); // X and Y start positions in terrain heightmap
    public Vector2Int digAreaSize = new Vector2Int(20, 20);  // Width and Length of the digging area
    public float maxDepth = 0.5f;  // Maximum depth in terrain heightmap (0 = ground, 1 = highest)
    public int amount;  // Amount of Terrain to dig
    public GameObject inputPanel; // Reference to the input panel

    private float depthInput;
    private TerrainData terrainData;
    private int heightmapRes;
    private string input;
    private float[,] originalHeights;  // Store the original terrain heights

    void Start()
    {
        InitializeTerrainData();
        DigInitialArea();
    }

    private void InitializeTerrainData()
    {
        terrainData = terrain.terrainData;
        heightmapRes = terrainData.heightmapResolution;
        Debug.Log($"Heightmap resolution: {heightmapRes}"); // Log the heightmap resolution
        originalHeights = terrainData.GetHeights(digAreaStart.x, digAreaStart.y, digAreaSize.x, digAreaSize.y);
    }

    private void DigInitialArea()
    {
        int xStart = digAreaStart.x;
        int yStart = digAreaStart.y;
        int xSize = digAreaSize.x;
        int ySize = digAreaSize.y;

        for (int a = 0; a < amount; a++)
        {
            int newXStart = xStart + (a * xSize);

            if (!IsValidDiggingArea(newXStart, yStart, xSize, ySize))
                continue;

            float[,] heights = CreateHeightArray(xSize, ySize, 0.06f);
            terrainData.SetHeights(newXStart, yStart, heights);
        }
    }

    private bool IsValidDiggingArea(int xStart, int yStart, int xSize, int ySize)
    {
        if (xStart < 0 || yStart < 0 || xStart + xSize > heightmapRes || yStart + ySize > heightmapRes)
        {
            Debug.LogError($"Digging area exceeds terrain bounds! Start: ({xStart}, {yStart}), Size: ({xSize}, {ySize}), Resolution: {heightmapRes}");
            return false;
        }
        return true;
    }

    private float[,] CreateHeightArray(int width, int height, float value)
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = value;
            }
        }
        return heights;
    }

    public void PrintOriginalHeights()
    {
        for (int x = 0; x < originalHeights.GetLength(0); x++)
        {
            for (int y = 0; y < originalHeights.GetLength(1); y++)
            {
                Debug.Log($"Original height at ({x}, {y}): {originalHeights[x, y]}");
            }
        }
    }

    public void ReadPanelInput(string s)
    {
        input = s;
        depthInput = float.Parse(input); // Convert string to float
        Debug.Log("Terrain set to: " + depthInput + " units");

        if (inputPanel != null)
        {
            inputPanel.SetActive(false); // Hide the input panel after reading the input
            Debug.Log("Input panel deactivated.");
        }
    }

    public void Dig()
    {
        Debug.Log("Raising terrain with height: " + depthInput);
        float depth = Mathf.Clamp(depthInput, 0f, maxDepth);  // Limit height range
        int xStart = digAreaStart.x;
        int yStart = digAreaStart.y;
        int xSize = digAreaSize.x;
        int ySize = digAreaSize.y;

        for (int a = 0; a < amount; a++)
        {
            int newXStart = xStart + (a * xSize);

            if (!IsValidDiggingArea(newXStart, yStart, xSize, ySize))
                continue;

            originalHeights = terrainData.GetHeights(newXStart, yStart, xSize, ySize);
            float[,] heights = ModifyHeightArray(newXStart, yStart, xSize, ySize, depth);
            terrainData.SetHeights(newXStart, yStart, heights);
        }
    }

    private float[,] ModifyHeightArray(int xStart, int yStart, int xSize, int ySize, float depth)
    {
        float[,] heights = terrainData.GetHeights(xStart, yStart, xSize, ySize);

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Debug.Log($"Original height: {originalHeights[x, y]} at ({x}, {y})");
                heights[x, y] = originalHeights[x, y] - (depth / 600); // Add depth to raise terrain
            }
        }

        return heights;
    }
}
