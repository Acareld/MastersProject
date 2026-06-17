using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Difficulty difficulty;

    private TerrainGeneratorManager generatorManager;

    [SerializeField]
    private bool bShouldGenerate = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generatorManager = GameObject.FindWithTag("TerrainGeneratorManager").GetComponent<TerrainGeneratorManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(bShouldGenerate)
        {
            DifficultyState state = DifficultyManager.instance.GetDifficultySettings(difficulty);
            generatorManager.GenerateNextSegment(state);
            bShouldGenerate = false;
        }
    }
}
