using UnityEngine;

public class TrenchDigger : MonoBehaviour
{
    public Terrain terrain;
    private TerrainData terrainData;
    private int heightmapRes;
    public int amount;  // Amount of Terrain to dig

    private string input1;
    private string input2;
    public Vector2Int digAreaSize = new Vector2Int(20, 20);  // Width and Length of the digging area

    private float[,] originalHeights;  // Store the original terrain heights

    public float depthInput;
    public float lengthInput;

    public GameObject inputPanel; // Reference to the input panel
    
    public float trenchWidth = 5f;
    public Vector2Int digAreaStart = new Vector2Int(50, 50); // heightmap start position (X, Z)

    void Start()
    {
        InitializeTerrainData();
        DigInitialArea();
    }
    public void ReadDepthInput(string d)
    {
        input1 = d;
        depthInput = float.Parse(input1); // Convert string to float
    }

    public void ReadLengthInput(string h)
    {
        input2 = h;
        lengthInput = float.Parse(input2); // Convert string to float
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
    public void DigTrench()
    {
        Debug.Log("Digging trench with depth: " + depthInput + " and length: " + lengthInput);
        Debug.Log("Digging trench with width: " + trenchWidth);
        Debug.Log("Digging trench with length: " + lengthInput);
        TerrainData terrainData = terrain.terrainData;

        float widthMeters = trenchWidth;
        float depthMeters = depthInput;
        float lengthMeters = lengthInput;

        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;

        Vector3 terrainSize = terrainData.size;

        // Convert world units (meters) to heightmap samples
        int trenchWidthSamples = Mathf.RoundToInt((widthMeters / terrainSize.x) * heightmapWidth);   //Dapetin value ke kanan
        int trenchLengthSamples = Mathf.RoundToInt((lengthMeters / terrainSize.z) * heightmapHeight);  //Dapetin value ke depan

        int startX = digAreaStart.x;
        int startZ = digAreaStart.y;

        float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);   //Dapetin value tinggi

        float terrainMaxHeight = terrainSize.y;
        float depthNormalized = depthMeters / terrainMaxHeight;
        //startX = kanan
        //start Y = bawah???
        //StartZ = depan
        for (int z = 0; z < trenchLengthSamples; z++)   //ubah value ke depan, karena lengthsamples
        {
            for (int x = 0; x < trenchWidthSamples; x++)   //Ubah value ke kanan, karena widthsamples. Berarti dia ke bawah dulu, baru ke samping, baru ke depan
            {
                int posX = startX + x;
                int posZ = startZ + z;
                //iterasi 1 
                //saat x = 1
                // saat x = 2
                if (posX >= 0 && posX < heightmapWidth && posZ >= 0 && posZ < heightmapHeight)
                {
                    heights[posZ, posX] = Mathf.Max(0, heights[posZ, posX] - depthNormalized); //ubah value ke bawah
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }


    public void CloseButton(){
            inputPanel.SetActive(false); // Hide the input panel after reading the input
            Debug.Log("Input panel deactivated.");
    }

}


// using UnityEngine;

// public class TrenchDigger : MonoBehaviour
// {
//     public Terrain terrain;
//     private TerrainData terrainData;
//     private int heightmapRes;
//     public int amount;  // Amount of Terrain to dig

//     private string input1; // depth
//     private string input2; // length

//     public Vector2Int digAreaSize = new Vector2Int(20, 20);  // Width and Length of the digging area
//     private float[,] originalHeights;  // Store the original terrain heights

//     public float depthInput;
//     public float lengthInput; // new input for trench length
//     public float trenchWidthInMeters = 2f; // fixed trench width

//     public GameObject inputPanel; // Reference to the input panel
//     public Vector2Int digAreaStart = new Vector2Int(50, 50); // heightmap start position (X, Z)

//     void Start()
//     {
//         InitializeTerrainData();
//         DigInitialArea();
//     }

//     public void ReadDepthInput(string d)
//     {
//         input1 = d;
//         depthInput = float.Parse(input1); // Convert string to float
//     }

//     public void ReadLengthInput(string l)
//     {
//         input2 = l;
//         lengthInput = float.Parse(input2); // Convert string to float
//     }

//     private void InitializeTerrainData()
//     {
//         terrainData = terrain.terrainData;
//         heightmapRes = terrainData.heightmapResolution;
//         Debug.Log($"Heightmap resolution: {heightmapRes}");
//         originalHeights = terrainData.GetHeights(digAreaStart.x, digAreaStart.y, digAreaSize.x, digAreaSize.y);
//     }

//     private void DigInitialArea()
//     {
//         int xStart = digAreaStart.x;
//         int yStart = digAreaStart.y;
//         int xSize = digAreaSize.x;
//         int ySize = digAreaSize.y;

//         for (int a = 0; a < amount; a++)
//         {
//             int newXStart = xStart + (a * xSize);

//             if (!IsValidDiggingArea(newXStart, yStart, xSize, ySize))
//                 continue;

//             float[,] heights = CreateHeightArray(xSize, ySize, 0.06f);
//             terrainData.SetHeights(newXStart, yStart, heights);
//         }
//     }

//     private bool IsValidDiggingArea(int xStart, int yStart, int xSize, int ySize)
//     {
//         if (xStart < 0 || yStart < 0 || xStart + xSize > heightmapRes || yStart + ySize > heightmapRes)
//         {
//             Debug.LogError($"Digging area exceeds terrain bounds! Start: ({xStart}, {yStart}), Size: ({xSize}, {ySize}), Resolution: {heightmapRes}");
//             return false;
//         }
//         return true;
//     }

//     private float[,] CreateHeightArray(int width, int height, float value)
//     {
//         float[,] heights = new float[width, height];
//         for (int x = 0; x < width; x++)
//         {
//             for (int y = 0; y < height; y++)
//             {
//                 heights[x, y] = value;
//             }
//         }
//         return heights;
//     }

//     public void DigTrench()
//     {
//         terrainData = terrain.terrainData;

//         float widthMeters = trenchWidthInMete rs; // fixed width
//         float depthMeters = depthInput;
//         float lengthMeters = lengthInput;

//         int heightmapWidth = terrainData.heightmapResolution;
//         int heightmapHeight = terrainData.heightmapResolution;
//         Vector3 terrainSize = terrainData.size;

//         // Convert world units (meters) to heightmap samples
//         int trenchWidthSamples = Mathf.RoundToInt((widthMeters / terrainSize.x) * heightmapWidth);
//         int trenchLengthSamples = Mathf.RoundToInt((lengthMeters / terrainSize.z) * heightmapHeight);

//         int startX = digAreaStart.x;
//         int startZ = digAreaStart.y;

//         float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

//         float terrainMaxHeight = terrainSize.y;
//         float depthNormalized = depthMeters / terrainMaxHeight;

//         for (int z = 0; z < trenchLengthSamples; z++)
//         {
//             for (int x = 0; x < trenchWidthSamples; x++)
//             {
//                 int posX = startX + x;
//                 int posZ = startZ + z;

//                 if (posX >= 0 && posX < heightmapWidth && posZ >= 0 && posZ < heightmapHeight)
//                 {
//                     heights[posZ, posX] = Mathf.Max(0, heights[posZ, posX] - depthNormalized);
//                 }
//             }
//         }

//         terrainData.SetHeights(0, 0, heights);
//     }

//     public void CloseButton()
//     {
//         inputPanel.SetActive(false); // Hide the input panel after reading the input
//         Debug.Log("Input panel deactivated.");
//     }
// }
