using System.Collections.Generic;
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

    void Awake()
    {
        pathFinder = GetComponent<PathFinder>();
    }

    void Update()
    {
        /*if(shouldRegenerate)
        {
            Cleanup();
            GenerateMap();
            shouldRegenerate = false;
        }*/
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

    public void Generate()
    {
        Cleanup();
        GenerateMap();
        FindPath();
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

    private void GenerateMap()
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        // add band of overlapTiles to the new segment
        if(bHasEntryConnector)
        {
            tiles.AddRange(entryConnector.overlapTiles);
            foreach(TileGeneration tGen in entryConnector.overlapTiles)
            {
                Debug.Log("Added old vertices");
                totalVertices.AddRange(tGen.GetWorldVertices());
            }
        }

        for(int x = 0; x < mapWidth; x++)
        {
            for(int z = 0; z < mapDepth; z++)
            {
                Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + x * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + z * tileDepth);
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                TileGeneration tileGen = tile.GetComponent<TileGeneration>();
                tiles.Add(tileGen);
                totalVertices.AddRange(tileGen.Regenerate());
            }
        } 
    }

    private void FindPath()
    {
        float dist = Vector2.Distance(new Vector2(totalVertices[0].x, totalVertices[0].z), new Vector2(totalVertices[1].x, totalVertices[1].z));

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
        Debug.Log("NodeDict size: " + nodeDict.Count);

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
            
            if (tile.gameObject.transform.position.x == gameObject.transform.position.x + 390)
            {
                
                exitConnector.overlapTiles.Add(tile);
            }
        }
        Debug.DrawRay(exitConnector.worldPosition, Vector3.up * 20f, Color.green, 100f);
    }


}
