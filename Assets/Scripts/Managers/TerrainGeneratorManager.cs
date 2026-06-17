using UnityEngine;

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
        gen.Generate();
        nextOffset += generatorOffset;
    }

}
