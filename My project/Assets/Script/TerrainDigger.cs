using UnityEngine;
using TMPro; // For TextMeshPro UI

public class TerrainDigger : MonoBehaviour
{
    public Terrain terrain;  // Assign your Terrain in the Inspector
    public Vector2Int digAreaStart = new Vector2Int(50, 50); // X and Y start positions in terrain heightmap
    public Vector2Int digAreaSize = new Vector2Int(20, 20);  // Width and Length of the digging area
    public float maxDepth = 0.5f;  // Maximum depth in terrain heightmap (0 = ground, 1 = highest)
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

        // Get the heightmap data for the area
        float[,] heights = terrainData.GetHeights(xStart, yStart, xSize, ySize);

        // Modify the height values based on depth input
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                heights[x, y] = originalHeights[x, y] + (depth/100); // Add depth to raise terrain
            }
        }

        // Apply the modified heightmap back to the terrain
        terrainData.SetHeights(xStart, yStart, heights);

        // Create a cube as big as the digging area
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = new Vector3(xSize, 10, ySize); // Set the size of the cube to match the digging area
        Vector3 terrainPosition = terrain.transform.position;
        float cubeX = terrainPosition.x + xStart + xSize / 2f;
        float cubeZ = terrainPosition.z + yStart + ySize / 2f;
        float cubeY = terrain.SampleHeight(new Vector3(cubeX, 0, cubeZ)) + terrainPosition.y + 5f; // Position above terrain
        cube.transform.position = new Vector3(cubeX, cubeY, cubeZ);
    }
}
