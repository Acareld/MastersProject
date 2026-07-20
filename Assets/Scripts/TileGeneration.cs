using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;


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

    [HideInInspector]
    public Vector3[] vertices;

    [HideInInspector]
    public Vector3[] baseVertices;

    private Color[] colorMap;
    private Texture2D tileTexture;

    private Mesh mesh;

    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform vehicleTransform;

    private float chunkSize;

    private Vector4[] biomeData;
    private Vector4[] obstacleData;
    private float[] heightMap;
    private bool isPlayable;

    private int quadsPerSide;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //noiseMapGenerator = GetComponent<NoiseMapGenerator>();
        //pathFinder = GetComponent<PathFinder>();
        //meshRenderer = GetComponent<MeshRenderer>();
        //meshFilter = GetComponent<MeshFilter>();
        //meshCollider = GetComponent<MeshCollider>();

        
    }

    private void Awake()
    {
        noiseMapGenerator = GetComponent<NoiseMapGenerator>();
        pathFinder = GetComponent<PathFinder>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        // get player transform
        playerTransform = GameObject.FindWithTag("Player").GetComponentInChildren<CharacterController>().gameObject.transform;
        vehicleTransform = GameObject.FindWithTag("VehicleRigidbody").transform;
    }

    void Update()
    {
        if (shouldRegenerate) Regenerate(true, 100, 100);   
    }

    public Vector3[] Regenerate(bool isPlayable, int quadsPerSide, float chunkSize)
    {
        if(isPlayable)
        {
            mesh = MeshGenerator.CreateGridMesh(quadsPerSide, chunkSize);
        }
        else
        {
            mesh = MeshGenerator.CreateGridMesh(quadsPerSide / 2, chunkSize);
        }
        
        mesh.MarkDynamic();
        meshFilter.sharedMesh = mesh;

        baseVertices = meshFilter.mesh.vertices;
        vertices = new Vector3[baseVertices.Length];
        baseVertices.CopyTo(vertices, 0);
        uvs = new Vector2[vertices.Length];
        biomeData = new Vector4[vertices.Length];
        heightMap = new float[vertices.Length];
        obstacleData = new Vector4[vertices.Length];

        offset = Vector2.zero;
        MapNoisePositions();
        GenerateTile();
        shouldRegenerate = false;

        this.isPlayable = isPlayable;

        if(!isPlayable)
        {
            if(meshCollider != null)
            {
                meshCollider.sharedMesh = null;
                meshCollider.enabled = false;
            }
            return System.Array.Empty<Vector3>();
        }

        if(meshCollider != null)
        {
            meshCollider.enabled = true;
        }

        this.chunkSize = chunkSize;
        this.quadsPerSide = quadsPerSide;
        StartCoroutine(CheckIsPlayerNearRoutine());
        
        return GetWorldVertices();
    }

    private void MapNoisePositions()
    {
        for(int i = 0;  i < vertices.Length; i++)
        {
            Vector3 worldVertex = transform.TransformPoint(vertices[i]);
            uvs[i] = new Vector2(worldVertex.x, worldVertex.z);
        }
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
        heightMap = noiseMapGenerator.ScheduleNoiseJob(this.mapScaleLarge, offset, uvs);

        tileTexture = BuildTexture(heightMap);
        //this.meshRenderer.material.mainTexture = tileTexture;

        UpdateMeshVertices(heightMap);
    }

    private TerrainType ChooseTerrainType(float height)
    {
        for (int i = 0; i < terrainTypes.Length; i++)
        {
            TerrainType currentType = terrainTypes[i];

            if (height < currentType.height)
            {
                if (i == 0 || i == 1) return currentType;

                TerrainType previousType = terrainTypes[i - 1];

                float currentTypeStart = previousType.height;

                float overlapStart = currentTypeStart - 0.1f;
                float overlapEnd = currentTypeStart + 0.1f;

                if (height >= overlapStart && height <= overlapEnd)
                {
                    float chanceForCurrent = Mathf.InverseLerp(
                        overlapStart,
                        overlapEnd,
                        height
                    );

                    return UnityEngine.Random.value < chanceForCurrent
                        ? currentType
                        : previousType;
                }

                return currentType;
            }
        }

        return terrainTypes[terrainTypes.Length - 1];
   
}

    private Color ChooseTerrainColor(float height, Vector2 worldPos)
    {
        for (int i = 1; i < terrainTypes.Length; i++)
        {
            TerrainType previousType = terrainTypes[i - 1];
            TerrainType currentType = terrainTypes[i];

            float borderHeight = previousType.height;

            float blendWidth = 0.05f;
            float noiseStrength = 0.03f;

            float noise = Mathf.PerlinNoise(
                worldPos.x * 0.08f,
                worldPos.y * 0.08f
            );

            noise = noise * 2f - 1f;

            float noisyBorder = borderHeight + noise * noiseStrength;

            float overlapStart = noisyBorder - blendWidth;
            float overlapEnd = noisyBorder + blendWidth;

            if (height >= overlapStart && height <= overlapEnd)
            {
                float t = Mathf.InverseLerp(overlapStart, overlapEnd, height);
                t = Mathf.SmoothStep(0f, 1f, t);

                return Color.Lerp(previousType.color, currentType.color, t);
            }
        }

        for (int i = 0; i < terrainTypes.Length; i++)
        {
            if (height < terrainTypes[i].height)
                return terrainTypes[i].color;
        }

        return terrainTypes[terrainTypes.Length - 1].color;

    }


    private Texture2D BuildTexture(float[] heightMap)
    {
        colorMap = new Color[uvs.Length];
        for(int i = 0; i < uvs.Length; i++)
        {
            float height = heightMap[i];
            //TerrainType type = ChooseTerrainType(height);
            //colorMap[i] = type.color;
            colorMap[i] = ChooseTerrainColor(height, uvs[i]);
        }

        int tileDepth = (int)Mathf.Sqrt(vertices.Length);
        Texture2D tex = new Texture2D(tileDepth, tileDepth);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(colorMap);
        tex.Apply();

        return tex;
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

            biomeData[i] = new Vector4(height, 0f);
            obstacleData[i] = Vector4.zero;
            
        }

        /* meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateNormals();
       */
        
        mesh.vertices = vertices;
        if(!isPlayable)
        {
            mesh.SetUVs(1, biomeData);
            mesh.SetUVs(2, obstacleData);
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
    }

    public Vector3[] GetExitVerticesAroundWorldPoint(
    Vector3 exitWorldPosition,
    float segmentRightEdgeX,
    int depthInVertices = 10,
    int halfWidthInVertices = 10
)
    {
        List<Vector3> exitVertices = new List<Vector3>();

        int vertsPerSide = 100 + 1;
        float step = chunkSize / 100;

        float tileRightEdgeX = transform.position.x + chunkSize;

        if (Mathf.Abs(tileRightEdgeX - segmentRightEdgeX) > 0.01f)
        {
            return System.Array.Empty<Vector3>();
        }

        Vector3 localExit = transform.InverseTransformPoint(exitWorldPosition);

        int xEnd = vertsPerSide - 1;
        int xStart = Mathf.Max(0, xEnd - depthInVertices);

        int zCenter = Mathf.RoundToInt(localExit.z / step);
        int zStart = Mathf.Max(0, zCenter - halfWidthInVertices);
        int zEnd = Mathf.Min(vertsPerSide - 1, zCenter + halfWidthInVertices);

        if (zEnd < 0 || zStart >= vertsPerSide)
        {
            return System.Array.Empty<Vector3>();
        }

        for (int z = zStart; z <= zEnd; z++)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                int index = z * vertsPerSide + x;

                Vector3 worldVertex = transform.TransformPoint(vertices[index]);
                exitVertices.Add(worldVertex);
            }
        }

        return exitVertices.ToArray();
    }

    public Vector3[] GetWorldVertices()
    {
        Vector3[] worldVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            worldVertices[i] = transform.TransformPoint(vertices[i]);
        }

        return worldVertices;
    }

    public void ApplyVertexHeight(Dictionary<Vector2Int, TerrainNode> nodeDict)
    {
        bool textureChanged = false;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldVertex = transform.TransformPoint(vertices[i]);

            Vector2Int key = new Vector2Int(Mathf.FloorToInt(Mathf.Round(worldVertex.x)), Mathf.FloorToInt(Mathf.Round(worldVertex.z)));

            Vector4 uv1 = new Vector4(heightMap[i], 0f, 0f, 0f);
            Vector4 uv2 = new Vector4(0f, 0f, 0f, 0f);

            if (nodeDict.TryGetValue(key, out TerrainNode node))
            {
                Vector3 changedWorldVertex = new Vector3(worldVertex.x, node.position.y, worldVertex.z);

                if ((colorMap != null && i < colorMap.Length) && changedWorldVertex.y != vertices[i].y )
                {
                    if(node.type == TerrainNode.TerrainType.ROAD)
                    {
                        colorMap[i] = Color.black;
                        uv2.x = 1f;
                    }
                    else if(node.type == TerrainNode.TerrainType.HOLE)
                    {
                        colorMap[i] = Color.red;
                        uv2.y = 1f;
                    }
                    else if(node.type == TerrainNode.TerrainType.BIGHOLE)
                    {
                        colorMap[i] = Color.magenta;
                        uv2.z = 1f;
                    }
                    else if (node.type == TerrainNode.TerrainType.RAMP)
                    {
                        colorMap[i] = Color.blue;
                        uv2.w = 1f;
                    }
                    textureChanged = true;
                    biomeData[i] = uv1;
                    obstacleData[i] = uv2;
                }


                vertices[i] = transform.InverseTransformPoint(changedWorldVertex);

            }
            
        }

        mesh.vertices = vertices;
        
        mesh.SetUVs(1, biomeData);
        mesh.SetUVs(2, obstacleData);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        if (textureChanged && tileTexture != null)
        {
            tileTexture.SetPixels(colorMap);
            tileTexture.Apply();

            //meshRenderer.material.mainTexture = tileTexture;
        }

    }

    private IEnumerator CheckIsPlayerNearRoutine()
    {
        while(true)
        {
            Vector2 flatPos = new Vector2(transform.position.x + (chunkSize / 2), transform.position.z + (chunkSize / 2));
            Vector2 flatPlayerPos = new Vector2(playerTransform.position.x, playerTransform.position.z);
            Vector2 flatvehiclePos = new Vector2(vehicleTransform.position.x, vehicleTransform.position.z);
            float playerDist = Vector2.Distance(flatPos, flatPlayerPos);
            float vehicleDist = Vector2.Distance(flatPos, flatvehiclePos);

            if (playerDist < chunkSize || vehicleDist < chunkSize)
            {
                meshCollider.enabled = true;
            }
            else
            {
                meshCollider.enabled = false;
            }

            yield return new WaitForSeconds(5);
        }        
    }

    public void ForceColliderUpdate()
    {
        meshCollider.enabled = true;
    }
}
