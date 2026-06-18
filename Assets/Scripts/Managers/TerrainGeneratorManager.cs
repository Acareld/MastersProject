using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoadConnector
{
    public Vector3 worldPosition;
    public float height;
    public Vector3 direction;
    public int roadRadius;
    public bool isValid;
    public List<TileGeneration> overlapTiles;
}

public class TerrainGeneratorManager : MonoBehaviour
{
    [SerializeField]
    private GameObject terrainGeneratorPrefab;

    [SerializeField]
    private bool bGenerateNextSegment = false;

    // current hardcoded offset for the generator
    [SerializeField]
    private int generatorOffset = 400;

    private int nextOffset = 0;

    private RoadConnector lastExitRoadConnector;
    private bool bHasExitRoadConnector = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(bGenerateNextSegment)
        {
           // GenerateNextSegment();
            bGenerateNextSegment = false;
        }
    }

    public void GenerateNextSegment(DifficultyState state)
    { 
        Vector3 position = new Vector3(nextOffset, 0, 0);
        GameObject terrainGen = Instantiate(terrainGeneratorPrefab, position, Quaternion.identity);
        TerrainGenerator gen = terrainGen.GetComponent<TerrainGenerator>();
        gen.SetDifficultySettings(state);

        if(bHasExitRoadConnector)
        {
            gen.SetEntryConnector(lastExitRoadConnector);
            
        }

        gen.Generate();

        bHasExitRoadConnector = true;
        lastExitRoadConnector = gen.GetExitConnector();

        nextOffset += generatorOffset;
    }

}
