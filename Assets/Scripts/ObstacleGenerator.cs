using System.Collections.Generic;
using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{

    private PotholeGenerator potholeGenerator;
    private RampGenerator rampGenerator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        potholeGenerator = GetComponent<PotholeGenerator>();
        rampGenerator = GetComponent<RampGenerator>();
    }

    public void GenerateObstacles(DifficultyState state, Dictionary<Vector2Int, TerrainNode> nodeDict, List<TerrainNode> path, HashSet<Vector2Int> roadMask, int vertexDist)
    {
        potholeGenerator.SetDifficultySettings(state);
        rampGenerator.SetDifficultySettings(state);

        potholeGenerator.PlacePotholes(nodeDict, path, roadMask, vertexDist);
        rampGenerator.PlaceRampHoles(nodeDict, path, roadMask, vertexDist);
    }
}
