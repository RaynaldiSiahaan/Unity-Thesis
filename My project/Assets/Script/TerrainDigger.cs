using UnityEngine;
using TMPro; // For TextMeshPro UI

public class TerrainDigger : MonoBehaviour
{
    public Terrain terrain;  // Assign your Terrain in the Inspector
    public Vector2Int digAreaStart = new Vector2Int(50, 50); // X and Y start positions in terrain heightmap
    public Vector2Int digAreaSize = new Vector2Int(20, 20);  // Width and Length of the digging area
    public float maxDepth = 0.5f;  // Maximum depth in terrain heightmap (0 = ground, 1 = highest)
    public int amount;  // Amount of Terrain to
    public GameObject inputPanel; // Reference to the input panel
 

    private float depthInput;    
    private TerrainData terrainData;
    private int heightmapRes;
    private string input;

    
    private float[,] originalHeights;  // Store the original terrain heights

    void Start()
    {
        terrainData = terrain.terrainData;
        heightmapRes = terrainData.heightmapResolution;
        Debug.Log($"Heightmap resolution: {heightmapRes}"); // Log the heightmap resolution

        // Store the original terrain heights to allow for reset
        originalHeights = terrainData.GetHeights(digAreaStart.x, digAreaStart.y, digAreaSize.x, digAreaSize.y);        
    }

    public void ReadPanelInput(string s)
    {
        input = s;
        depthInput = float.Parse(input); // Convert string to float
        Debug.Log("Terrain set to: " + depthInput + "units");

        if (inputPanel !=  null)
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
            // Calculate the new starting position for this iteration
            int newXStart = xStart + (a * xSize);

            // Validate the new digging area dimensions
            if (newXStart < 0 || yStart < 0 || newXStart + xSize > heightmapRes || yStart + ySize > heightmapRes)
            {
                Debug.LogError($"Digging area exceeds terrain bounds! Start: ({newXStart}, {yStart}), Size: ({xSize}, {ySize}), Resolution: {heightmapRes}");
                continue; // Skip this iteration if the area is invalid
            }

            // Refresh the originalHeights array for the new area
            originalHeights = terrainData.GetHeights(newXStart, yStart, xSize, ySize);

            // Get the heightmap data for the new area
            float[,] heights = terrainData.GetHeights(newXStart, yStart, xSize, ySize);

            // Modify the height values based on depth input
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    Debug.Log($"Original height: {originalHeights[x, y]} at ({x}, {y})");
                    heights[x, y] = originalHeights[x, y] - (depth / 600); // Add depth to raise terrain
                }
            }

            // Apply the modified heightmap back to the terrain
            terrainData.SetHeights(newXStart, yStart, heights);
        }
    }
}
