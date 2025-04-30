using UnityEngine;

public class GridOverlay : MonoBehaviour
{
   public int gridSize = 50; // Size of each grid cell (in pixels)
    public Color gridColor = Color.green; // Color of the grid lines
    public RectTransform canvasRectTransform; // Reference to the canvas RectTransform

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        DrawGrid();
    }

    void DrawGrid()
    {
        if (lineRenderer == null || canvasRectTransform == null) return;

        // Clear existing lines
        lineRenderer.positionCount = 0;

        // Calculate grid spacing based on the canvas size
        float width = canvasRectTransform.rect.width;
        float height = canvasRectTransform.rect.height;

        // Draw vertical grid lines
        for (float x = 0; x < width; x += gridSize)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, height));
        }

        // Draw horizontal grid lines
        for (float y = 0; y < height; y += gridSize)
        {
            DrawLine(new Vector2(0, y), new Vector2(width, y));
        }
    }

    void DrawLine(Vector2 start, Vector2 end)
    {
        // Set the LineRenderer positions
        lineRenderer.positionCount += 2; // Adding two points for each line
        lineRenderer.SetPosition(lineRenderer.positionCount - 2, start);
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, end);
    }
}
