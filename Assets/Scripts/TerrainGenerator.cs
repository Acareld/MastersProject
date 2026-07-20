using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private int mapWidth, mapDepth;

    [SerializeField]
    private GameObject tilePrefab;

    private List<Vector3> totalVertices = new List<Vector3>();

    private List<TileGeneration> tiles = new List<TileGeneration>();

    private PathFinder pathFinder;

    [SerializeField]
    private bool shouldRegenerate = false;

    // Difficulty Settings
    private DifficultyState difficultyState;

    private RoadConnector entryConnector;
    private RoadConnector exitConnector;

    private bool bHasEntryConnector = false;

    [SerializeField] private int tilesGeneratedPerFrame = 40;
    [SerializeField] private int tilesAppliedPerFrame = 40;

    [SerializeField] private int quadsPerSide = 100;
    [SerializeField] private float chunkSize = 100f;

    [SerializeField] private GameObject triggerPointPrefab;
    [SerializeField] private GameObject terrainTriggerPointPrefab;

    private Coroutine generationCoroutine;

    private bool firstGeneration = false;

    private Vector3 respawnPoint;
    private bool bPathGenerated = false;

    private Dictionary<Vector2Int, TerrainNode> nodeDict = null;

    public void Generate()
    {
        if(generationCoroutine != null)
        {
            StopCoroutine(generationCoroutine);
        }

        generationCoroutine = StartCoroutine(GenerateAndWait(true));
    }

    public IEnumerator GenerateAndWait(bool firstGen)
    {
        if (generationCoroutine != null)
        {
            StopCoroutine(generationCoroutine);
            generationCoroutine = null;
        }

        firstGeneration = firstGen;

        yield return GeneratePlayableCoroutine();
    }

    public IEnumerator GenerateVisibleAndWait(bool isPlayable)
    {
        if (generationCoroutine != null)
        {
            StopCoroutine(generationCoroutine);
            generationCoroutine = null;
        }
        yield return GenerateVisibleCoroutine(isPlayable);
    }
   

    void Awake()
    {
        pathFinder = GetComponent<PathFinder>();
    }

    void Update()
    {
        if(shouldRegenerate)
        {
            pathFinder = GetComponent<PathFinder>();
            Generate();
            shouldRegenerate = false;
        }
    }

    public void SetDifficultySettings(DifficultyState state)
    {
        difficultyState = state;
    }

    public void SetEntryConnector(RoadConnector connector)
    {
        entryConnector = connector;
        bHasEntryConnector = true;
    }

    public RoadConnector GetExitConnector()
    {
        return exitConnector;
    }

    private IEnumerator GenerateCoroutine()
    {
        Cleanup();

        yield return GenerateMapCoroutine(true);

        yield return null;

        

        yield return BuildPathCoroutine(result =>
        {
            nodeDict = result;
        });

        yield return null;

        yield return ApplyVerticesCoroutine(nodeDict);

        BuildExitConnector();

        generationCoroutine = null;
    }

    public IEnumerator GeneratePathCoroutine()
    {

        yield return BuildPathCoroutine(result =>
        {
            nodeDict = result;
        });

        yield return null;

        yield return ApplyVerticesCoroutine(nodeDict);

        BuildExitConnector();

        generationCoroutine = null;
    }

    // Only generate visible terrain, NO roads or pathfinding
    private IEnumerator GenerateVisibleCoroutine(bool isPlayable)
    {
        Cleanup();

        yield return GenerateMapCoroutine(isPlayable);


        generationCoroutine = null;
    }

    private IEnumerator GeneratePlayableCoroutine()
    {
        Cleanup();

        yield return GenerateMapCoroutine(true);

        yield return null;

        float dist = Vector2.Distance(
            new Vector2(totalVertices[0].x, totalVertices[0].z),
            new Vector2(totalVertices[1].x, totalVertices[1].z)
        );

        Vector3[] vertices = totalVertices.ToArray();

        yield return pathFinder.BuildNodeNetworkRoutine(vertices, dist);

        

        generationCoroutine = null;
    }

    private IEnumerator GenerateMapCoroutine(bool isPlayable)
    {
        /*Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = Mathf.RoundToInt(tileSize.x);
        int tileDepth = Mathf.RoundToInt(tileSize.z);*/

        int tileWidth = Mathf.RoundToInt(chunkSize);
        int tileDepth = Mathf.RoundToInt(chunkSize);

        // add band of overlapTiles to the new segment
        if (bHasEntryConnector)
        {
            tiles.AddRange(entryConnector.overlapTiles);
            foreach(TileGeneration tile in tiles)
            {
                
                Vector3[] exitVerts = tile.GetExitVerticesAroundWorldPoint(entryConnector.worldPosition, (transform.position.x - 400) + mapWidth * chunkSize);
                totalVertices.AddRange(exitVerts);
            }
            
        }

        int generatedThisFrame = 0;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapDepth; z++)
            {
                Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + x * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + z * tileDepth);
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                TileGeneration tileGen = tile.GetComponent<TileGeneration>();
                TerrainSurfaceLookup surface = tile.GetComponent<TerrainSurfaceLookup>();
                if(surface == null)
                {
                    tile.AddComponent<TerrainSurfaceLookup>();
                }
                surface.SetGenerator(this);

                tiles.Add(tileGen);
                totalVertices.AddRange(tileGen.Regenerate(isPlayable, quadsPerSide, chunkSize));

                generatedThisFrame++;

                if(generatedThisFrame >= tilesGeneratedPerFrame)
                {
                    generatedThisFrame = 0;
                    yield return null;
                }
            }
        }
    }

    private IEnumerator BuildPathCoroutine(System.Action<Dictionary<Vector2Int, TerrainNode>> onComplete)
    {
        
        pathFinder.SetDifficultySettings(difficultyState);

        if (bHasEntryConnector)
        {
            pathFinder.SetEntryConnector(entryConnector);
            pathFinder.ForceStartHeight(entryConnector.height);
            //pathFinder.ApplyEntrySeam(entryConnector);
        }


        yield return pathFinder.AStarRoutine(result =>
        {
            nodeDict = result;
        });

        if (firstGeneration)
        {
            GameManager gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
           // gm.Spawn();
        }

        SpawnTriggerPoint(pathFinder.GetTriggerPointPosition());
        SpawnTerrainTriggerPoint(pathFinder.GetTerrainTriggerPointPosition());
        respawnPoint = pathFinder.GetRespawnPoint();
        bPathGenerated = true;

        onComplete?.Invoke(nodeDict);
    }

    private void SpawnTriggerPoint(Vector3 position)
    {
        Instantiate(triggerPointPrefab, position, Quaternion.identity);
    }

    private void SpawnTerrainTriggerPoint(Vector3 position)
    {
        Instantiate(terrainTriggerPointPrefab, position, Quaternion.identity);
    }

    // --------------------------------------------------------

    private Dictionary<Vector2Int, TerrainNode> BuildPath()
    {
        float dist = Vector2.Distance(new Vector2(totalVertices[0].x, totalVertices[0].z), new Vector2(totalVertices[1].x, totalVertices[1].z));

        //Debug.Log("Vertex Dist: " + dist);

        pathFinder.BuildNodeNetwork(totalVertices.ToArray(), dist);
        pathFinder.SetDifficultySettings(difficultyState);

        if (bHasEntryConnector)
        {
            pathFinder.SetEntryConnector(entryConnector);
            pathFinder.ForceStartHeight(entryConnector.height);
        }

        Dictionary<Vector2Int, TerrainNode> nodeDict = pathFinder.AStar();

        if(firstGeneration)
        {
            GameManager gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
            gm.Spawn();
        }

        return nodeDict;
    }

    

    private IEnumerator ApplyVerticesCoroutine(Dictionary<Vector2Int, TerrainNode> nodeDict)
    {
        int appliedThisFrame = 0;

        foreach(TileGeneration tile in tiles)
        {
            tile.ApplyVertexHeight(nodeDict);

            appliedThisFrame++;

            if(appliedThisFrame >= tilesAppliedPerFrame)
            {
                appliedThisFrame = 0;
                yield return null;
            }
        }
    }

    private void BuildExitConnector()
    {
        exitConnector = pathFinder.GetExitRoadConnector();
        exitConnector.overlapTiles = new List<TileGeneration>();

        // horrible hardcoded add of overlaping/seam tiles
        // please end me if i have to see this again
        // bro just put in a field XXXXX
        foreach (TileGeneration tile in tiles)
        {
            if (tile.gameObject.transform.position.x == gameObject.transform.position.x + 300)
            {
               exitConnector.overlapTiles.Add(tile);
            }
        }

        //Debug.DrawRay(exitConnector.worldPosition, Vector3.up * 20f, Color.green, 100f);
    }

    public void GenerateSynchronous()
    {
        Cleanup();
        //GenerateMap();
        FindPath();
    }

    public void Purge()
    {
        foreach(TileGeneration tile in tiles)
        {
            if (tile != null)
            {
                GameObject.Destroy(tile.gameObject);
            }
            
        }
        Cleanup();
    }

    public bool IsPathGenerated()
    {
        return bPathGenerated;
    }

    public Vector3 GetRespawnPoint()
    {
        return respawnPoint;
    }

    public void ForceColliderUpdate()
    {
        foreach(TileGeneration tile in tiles)
        {
            tile.ForceColliderUpdate();
        }
    }

    private void Cleanup()
    {
        //foreach (TileGeneration tilegen in tiles)
        //{
        //    Destroy(tilegen.gameObject);
        //}
        tiles.Clear();
        totalVertices.Clear();
    }

   

    private void FindPath()
    {
        float dist = Vector2.Distance(new Vector2(totalVertices[0].x, totalVertices[0].z), new Vector2(totalVertices[1].x, totalVertices[1].z));
        Debug.Log("Vertex Dist: " + dist);
        //if(bHasEntryConnector)
        //{
        //    pathFinder.AddLastRoadVertices(entryConnector.lastRoadVertices);
        //}

        pathFinder.BuildNodeNetwork(totalVertices.ToArray(), dist);
        pathFinder.SetDifficultySettings(difficultyState);

        if (bHasEntryConnector)
        {
            pathFinder.SetEntryConnector(entryConnector);
            pathFinder.ForceStartHeight(entryConnector.height);
        }

        Dictionary<Vector2Int, TerrainNode> nodeDict = pathFinder.AStar();

        foreach (TileGeneration tile in tiles)
        {
            tile.ApplyVertexHeight(nodeDict);
        }


        exitConnector = pathFinder.GetExitRoadConnector();
        exitConnector.overlapTiles = new List<TileGeneration>();
        // horrible hardcoded add of overlaping/seam tiles
        // please end me if i have to see this again
        foreach (TileGeneration tile in tiles)
        {
            
            if (tile.gameObject.transform.position.x == gameObject.transform.position.x + 490)
            {
                
                exitConnector.overlapTiles.Add(tile);
            }
        }
        Debug.DrawRay(exitConnector.worldPosition, Vector3.up * 20f, Color.green, 100f);
    }

    public bool TryGetSurfaceType(Vector3 worldPosition, out TerrainNode.TerrainType surfaceType)
    {
        if(pathFinder == null)
        {
            Debug.Log("Pathfinder null");
            surfaceType = TerrainNode.TerrainType.TERRAIN;
            return false;
        }

        return pathFinder.TryGetSurfaceType(worldPosition, out surfaceType);
    }


}
