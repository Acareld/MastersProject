using UnityEngine;
using Unity.Mathematics;


[System.Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color color;
}


public class TileGeneration : MonoBehaviour
{

    [SerializeField]
    NoiseMapGenerator noiseMapGenerator;

    [SerializeField]
    PathFinder pathFinder;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private float mapScaleLarge;

    [SerializeField]
    private float mapScaleSmall;

    [SerializeField]
    private TerrainType[] terrainTypes;

    [SerializeField]
    private float heightMultiplier;

    [SerializeField]
    private AnimationCurve heightCurve;

    public bool shouldRegenerate = true;

    public Vector2 offset;

    private Vector2[] uvs;

    public Vector3[] vertices;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        noiseMapGenerator = GetComponent<NoiseMapGenerator>();
        pathFinder = GetComponent<PathFinder>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        vertices = meshFilter.mesh.vertices;
        uvs = new Vector2[vertices.Length];
    }

    void Update()
    {
        if (shouldRegenerate) Regenerate();
    }

    public Vector3[] Regenerate()
    {
        noiseMapGenerator = GetComponent<NoiseMapGenerator>();
        pathFinder = GetComponent<PathFinder>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        vertices = meshFilter.mesh.vertices;
        uvs = new Vector2[vertices.Length];

        float tileSize = GetComponent<MeshRenderer>().bounds.size.x;
        float offsetX = this.gameObject.transform.position.x / tileSize;
        float offsetY = this.gameObject.transform.position.z / tileSize;

        Debug.Log("TileSize: " +tileSize);
        Debug.Log("offsetX: " + offsetX);
        Debug.Log("offsetY: " + offsetY);

        offset = new Vector2(offsetX, offsetY);

        MapUVs();
        GenerateTile();
        shouldRegenerate = false;

        
       // pathFinder.BuildNodeNetwork(vertices);
        //pathFinder.AStar();

        return vertices;
    }

    void MapUVs()
    {
        for(int i = 0; i < vertices.Length; i++)
        {
            float u = Mathf.InverseLerp(meshFilter.mesh.bounds.min.x, meshFilter.mesh.bounds.max.x, vertices[i].x);
            float v = Mathf.InverseLerp(meshFilter.mesh.bounds.min.z, meshFilter.mesh.bounds.max.z, vertices[i].z);
            uvs[i] = new Vector2(u, v);
        }
    }

    void GenerateTile()
    { 
        float[] heightMap = noiseMapGenerator.GenerateNoiseMap(this.mapScaleLarge, this.mapScaleSmall, offset, uvs, heightCurve);

        Texture2D tileTexture = BuildTexture(heightMap);
        this.meshRenderer.material.mainTexture = tileTexture;

        UpdateMeshVertices(heightMap);
    }

    private TerrainType ChooseTerrainType(float height)
    {
        foreach (TerrainType type in this.terrainTypes)
        {
            if(height < type.height)
            {
                return type;
            }
        }
        return this.terrainTypes[terrainTypes.Length - 1];
    }

    private Texture2D BuildTexture(float[] heightMap)
    {
        Color[] colorMap = new Color[uvs.Length];
        float[] remappedHeightMap = RemapHeightsLocally(heightMap);
        for(int i = 0; i < uvs.Length; i++)
        {
            float height = heightMap[i];
            TerrainType type = ChooseTerrainType(height);

            colorMap[i] = type.color;
        }

        int tileDepth = (int)Mathf.Sqrt(vertices.Length);
        Texture2D tileTexture = new Texture2D(tileDepth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    private float[] RemapHeightsLocally(float[] heightMap)
    {
        float max = heightMap[0];
        float min = heightMap[0];
        float[] remappedHeightMap = new float[heightMap.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            if (heightMap[i] > max) max = heightMap[i];
            if (heightMap[i] < min) min = heightMap[i];
        }

        for (int i = 0; i < uvs.Length; i++)
        {
            remappedHeightMap[i] = Unity.Mathematics.math.remap(0,heightMultiplier,0,1, heightMap[i]);
        }

        return remappedHeightMap;

    }

    private void UpdateMeshVertices(float[] heightMap)
    {

        for(int i = 0; i < vertices.Length; i++)
        {
            float height = heightMap[i];

            Vector3 vertex = vertices[i];
            vertices[i] = new Vector3(vertex.x, heightCurve.Evaluate(height) * heightMultiplier, vertex.z);
            
        }

        this.meshFilter.mesh.vertices = vertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();
        this.meshCollider.sharedMesh = this.meshFilter.mesh;

        for(int i =0; i<uvs.Length; i++)
        {
            vertices[i] = transform.TransformPoint(vertices[i]);
        }
    }
}
