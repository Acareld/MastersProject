using System.Collections.Generic;
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pathFinder = GetComponent<PathFinder>();

        GenerateMap();
    }

    private void GenerateMap()
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

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

        FindPath();
    }

    private void FindPath()
    {
        float dist = Vector2.Distance(new Vector2(totalVertices[0].x, totalVertices[0].z), new Vector2(totalVertices[1].x, totalVertices[1].z));

         pathFinder.BuildNodeNetwork(totalVertices.ToArray(), dist);
         Dictionary<Vector2Int, TerrainNode> nodeDict = pathFinder.AStar();

        foreach(TileGeneration tile in tiles)
        {
            tile.ApplyVertexHeight(nodeDict);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
