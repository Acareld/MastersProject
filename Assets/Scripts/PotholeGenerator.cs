using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class PotholeGenerator : MonoBehaviour
{
    [Header("Potholes")]
    [SerializeField] private int potholeCount = 10;
    [SerializeField] private float minPotholeSpacing = 20f;
    [SerializeField] private Vector2 potholeRadiusRange = new Vector2(0.5f, 3.5f);
    [SerializeField] private Vector2 potholeDepthRange = new Vector2(0.4f, 1.2f);


    public void SetDifficultySettings(DifficultyState state)
    {
        potholeCount = state.potholeSettings.potholeCount;
        minPotholeSpacing = state.potholeSettings.minPotholeSpacing;
        potholeRadiusRange = state.potholeSettings.potholeRadiusRange;
        potholeDepthRange = state.potholeSettings.potholeDepthRange;
    }

    public void PlacePotholes(Dictionary<Vector2Int, TerrainNode> nodeDict, List<TerrainNode> path, HashSet<Vector2Int> roadMask, int vertexDist)
    {
        if (path == null || path.Count < 5) return;

        List<Vector2Int> placedPotholes = new List<Vector2Int>();
        int placed = 0;

        float skipDistance = 10f;

        List<Vector2Int> candidates = new List<Vector2Int>();

        foreach (Vector2Int node in roadMask)
        {
            Vector3 pos = nodeDict[node].position;

            if (Vector3.Distance(pos, path[0].position) >= skipDistance)
            {
                candidates.Add(node);
            }
        }

        PathFinder.Shuffle(candidates);
        foreach (Vector2Int candidate in candidates)
        {
            if (placed >= potholeCount) break;

            bool tooClose = false;

            foreach (Vector2Int pothole in placedPotholes)
            {
                if (Vector2Int.Distance(pothole, candidate) < minPotholeSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue;

            float radius = Random.Range(potholeRadiusRange.x, potholeRadiusRange.y);
            float depth = Random.Range(potholeDepthRange.x, potholeDepthRange.y);

            CreatePothole(nodeDict, candidate, radius, depth, vertexDist, roadMask);

            placedPotholes.Add(candidate);
            placed++;
        }

        if (placed < potholeCount)
        {
            Debug.LogWarning(
                $"Only placed {placed}/{potholeCount} potholes. Not enough valid road space."
            );
        }
    }
    private void CreatePothole(Dictionary<Vector2Int, TerrainNode> nodeDict, Vector2Int center, float radius, float depth, int dist, HashSet<Vector2Int> roadMask)
    {
        int nodeRadius = Mathf.CeilToInt(radius / Mathf.Max(dist, 1f));     

        for (int x = -nodeRadius; x <= nodeRadius; x++)
        {
            for (int z = -nodeRadius; z <= nodeRadius; z++)
            {
                Vector2Int key = center + new Vector2Int(x * dist, z * dist);

                if (!nodeDict.ContainsKey(key)) continue;

                if (!roadMask.Contains(key)) continue;

                float distance = Vector2Int.Distance(center, key);

                if (distance > radius) continue;

                float t = distance / radius;

                float falloff = 1f - Mathf.SmoothStep(0f, 1f, t);

                TerrainNode node = nodeDict[key];

                node.position.y -= depth * falloff;

                node.height = node.position.y;

                node.type = TerrainNode.TerrainType.HOLE;
            }
        }
    }

    
}
