using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;




public class RampGenerator : MonoBehaviour
{
    [Header("Hole with Ramp")]
    [SerializeField] private int holeCount = 10;
    [SerializeField] private float minHoleSpacing = 20f;
    [SerializeField] private Vector2 holeRadiusRange = new Vector2(1.5f, 3.5f);
    [SerializeField] private Vector2 holeDepthRange = new Vector2(1f, 3f);
    [SerializeField] private GameObject rampPrefab;

    private struct HoleCandidate
    {
        public Vector2Int key;
        public Vector2 roadForward;
        public float slope;
    }

    public void PlaceRampHoles(Dictionary<Vector2Int, TerrainNode> nodeDict, List<TerrainNode> path, HashSet<Vector2Int> roadMask, int vertexDist)
    {
        if (path == null || path.Count < 5) return;

        List<Vector2Int> placedPotholes = new List<Vector2Int>();
        List<HoleCandidate> candidates = new List<HoleCandidate>();

        int placed = 0;

        for (int i = 2; i < path.Count - 2; i++)
        {
            Vector2Int key = TerrainNode.NodeKey(path[i]);

            if (!roadMask.Contains(key))
                continue;

            Vector3 prev = path[i - 1].position;
            Vector3 current = path[i].position;
            Vector3 next = path[i + 1].position;

            Vector3 roadDir3D = next - prev;
            roadDir3D.y = 0f;

            if (roadDir3D.sqrMagnitude < 0.001f) continue;

            Vector2 roadForward = new Vector2(roadDir3D.x, roadDir3D.z).normalized;

            float distance = Vector3.Distance(prev, next);
            float heightDiff = Mathf.Abs(next.y - prev.y);

            float slope = heightDiff / Mathf.Max(distance, 0.001f);

            candidates.Add(new HoleCandidate
            {
                key = key,
                roadForward = roadForward,
                slope = slope
            });
        }
        PathFinder.Shuffle(candidates);

        foreach (HoleCandidate candidate in candidates)
        {
            if (placed >= holeCount) break;
            if (candidate.slope > 0.12f) continue;

            bool tooClose = false;

            foreach (Vector2Int pothole in placedPotholes)
            {
                if (Vector2Int.Distance(pothole, candidate.key) < minHoleSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue;

            float radius = Random.Range(holeRadiusRange.x, holeRadiusRange.y);
            float depth = Random.Range(holeDepthRange.x, holeDepthRange.y);



            CreateRampHoles(nodeDict, candidate.key, radius, depth, vertexDist, roadMask, candidate.roadForward);

            placedPotholes.Add(candidate.key);
            placed++;
        }

        if (placed < holeCount)
        {
            Debug.LogWarning(
                $"Only placed {placed}/{holeCount} potholes. Not enough valid road space."
            );
        }

    }

    private void CreateRampHoles(Dictionary<Vector2Int, TerrainNode> nodeDict, Vector2Int center, float radius, float depth, int dist, HashSet<Vector2Int> roadMask, Vector2 roadForward)
    {

        roadForward.Normalize();

        Vector2 roadRight = new Vector2(-roadForward.y, roadForward.x);


        int nodeRadius = Mathf.CeilToInt(radius / Mathf.Max(dist, 1f));

        for (int x = -nodeRadius; x <= nodeRadius; x++)
        {
            for (int z = -nodeRadius; z <= nodeRadius; z++)
            {
                Vector2Int key = center + new Vector2Int(x * dist, z * dist);

                if (!nodeDict.ContainsKey(key)) continue;

                if (!roadMask.Contains(key)) continue;

                Vector2 offset = new Vector2( key.x - center.x, key.y - center.y);

                float distance = offset.magnitude;

                if (distance > 0.001f)
                {
                    Vector2 dirFromCenter = offset / distance;

                    float roadDirectionAmount = Mathf.Abs(Vector2.Dot(dirFromCenter, roadForward));
                    bool isInRoadDirection = roadDirectionAmount >= 0.7f;
                    bool isFarEnough = distance >= 2f;

                    if (isInRoadDirection && isFarEnough)
                        continue;
                }


                if (distance > radius) continue;

                float t = distance / radius;

                float falloff = 1f - Mathf.SmoothStep(0f, 1f, t);

                TerrainNode node = nodeDict[key];

                node.position.y -= depth * falloff;

                node.height = node.position.y;

                node.type = TerrainNode.TerrainType.BIGHOLE;
            }
        }

    }


}
