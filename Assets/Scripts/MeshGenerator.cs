using UnityEngine;

public static class MeshGenerator
{
    public static void GenerateTerrainMesh(float[] heightmap)
    {
        int size = (int) Mathf.Sqrt(heightmap.Length);
        float topLeftX = (size - 1) / -2f;
        float topLeftZ = (size - 1) / 2f;

        MeshData meshData = new MeshData(size);

        for(int x = 0; x < size; x++)
        {
           for(int z = 0; z < size; z++)
            {
              
            }
        }
    }

    public static Mesh CreateGridMesh(int quadsPerSide, float size)
    {
        int vertsPerSide = quadsPerSide + 1;

        Vector3[] vertices = new Vector3[vertsPerSide * vertsPerSide];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[quadsPerSide * quadsPerSide * 6];

        float step = size / quadsPerSide;

        int v = 0;

        for(int z = 0; z < vertsPerSide; z++)
        {
            for(int x = 0; x < vertsPerSide; x++)
            {
                vertices[v] = new Vector3(x * step, 0f, z * step);
                uvs[v] = new Vector2(x / (float)quadsPerSide, z / (float)quadsPerSide);
                v++;
            }
        }

        int t = 0;

        for (int z = 0; z < quadsPerSide; z++)
        {
            for (int x = 0; x < quadsPerSide; x++)
            {
                int i = z * vertsPerSide + x;

                triangles[t++] = i;
                triangles[t++] = i + vertsPerSide;
                triangles[t++] = i + 1;

                triangles[t++] = i + 1;
                triangles[t++] = i + vertsPerSide;
                triangles[t++] = i + vertsPerSide + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "IGBMN";

        if(vertices.Length > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
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
