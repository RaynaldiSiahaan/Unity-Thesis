using UnityEngine;

public class AddFrictionToTerrain : MonoBehaviour
{
    void Start()
    {
        // Create a new Physics Material
        PhysicsMaterial frictionMaterial = new PhysicsMaterial();

        // Set friction properties
        frictionMaterial.dynamicFriction = 0.6f;  // Adjust as needed
        frictionMaterial.staticFriction = 0.7f;
        frictionMaterial.frictionCombine = PhysicsMaterialCombine.Maximum;

        // Get the Terrain Collider and assign the material
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider != null)
        {
            terrainCollider.material = frictionMaterial;
        }
        else
        {
            Debug.LogError("No TerrainCollider found on this object!");
        }
    }
}