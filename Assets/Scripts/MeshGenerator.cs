using UnityEngine;

public static class MeshGenerator
{
    public static void GenerateTerrainMesh(float[] heightmap)
    {
        int size = (int) Mathf.Sqrt(heightmap.Length);
        float topLeftX = (size - 1) / -2f;
        float topLeftZ = (size - 1) / 2f;

        MeshData meshData = new MeshData(size);
        int vertexIndex = 0;

        for(int x = 0; x < size; x++)
        {
           for(int z = 0; z < size; z++)
            {
              
            }
        }
    }
}


public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;

    int triangleIndex;

    public MeshData(int size)
    {
        vertices = new Vector3[size * size];
        triangles = new int[(size-1)*(size-1)*6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }
}
