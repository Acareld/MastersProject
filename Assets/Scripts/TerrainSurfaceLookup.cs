using UnityEngine;

public class TerrainSurfaceLookup : MonoBehaviour
{
    private TerrainGenerator generator;

    public void SetGenerator(TerrainGenerator owner)
    {
        generator = owner;
    }

    public bool TryGetSurfaceType(Vector3 worldPosition, out TerrainNode.TerrainType surfaceType)
    {
        if (generator == null)
        {
            Debug.Log("Generator null");
            surfaceType = TerrainNode.TerrainType.TERRAIN;
            return false;
        }

        return generator.TryGetSurfaceType(worldPosition, out surfaceType);
    }

}
