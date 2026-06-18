
using System.Collections.Generic;
using UnityEngine;




public class RampGenerator : MonoBehaviour
{
    [Header("Hole with Ramp")]
    [SerializeField] private int holeCount = 10;
    [SerializeField] private float minHoleSpacing = 20f;
    [SerializeField] private int trenchRadius;
    [SerializeField] private Vector2 holeDepthRange = new Vector2(1f, 3f);
    [SerializeField] private float maxRampSpawnSlope = 0.08f;
    [SerializeField] private GameObject rampPrefab;
    [SerializeField] private LayerMask groundLayer;

    private struct HoleCandidate
    {
        public Vector2Int key;
        public Vector2 roadForward;
        public float slope;
        public int pathIndex;
    }

    public void SetDifficultySettings(DifficultyState state)
    {
        holeCount = state.trenchSettings.trenchCount;
        minHoleSpacing = state.trenchSettings.minTrenchSpacing;
        trenchRadius = state.trenchSettings.trenchRadius;
        holeDepthRange = state.trenchSettings.trenchDepthRange;
        maxRampSpawnSlope = state.trenchSettings.maxRampSpawnSlope;
    }

    public void PlaceRampHoles(Dictionary<Vector2Int, TerrainNode> nodeDict, List<TerrainNode> path, HashSet<Vector2Int> roadMask, int vertexDist)
    {
        if (path == null || path.Count < 5) return;

        List<Vector2Int> placedPotholes = new List<Vector2Int>();
        List<HoleCandidate> candidates = new List<HoleCandidate>();

        int placed = 0;

        for (int i = 6; i < path.Count - 6; i++)
        {
            Vector2Int key = TerrainNode.NodeKey(path[i]);

            if (!roadMask.Contains(key))
                continue;

            int rampIndex = i - 3;

            if (IsAngleTooSharpInWindow(path, i, 2, 2, 3, 20f))
            {
                Vector3 pos = path[i].position + Vector3.up * 5f;
                //Debug.DrawRay(pos, Vector3.up * 5f, Color.blue, 100f);
                continue;
            }

            if (!IsPathFlatEnough(path, rampIndex, 2, 2, maxRampSpawnSlope))

            {
                Vector3 pos = path[i].position + Vector3.up * 3f;
                //Debug.DrawRay(pos, Vector3.up * 5f, Color.red, 100f);
                continue;
            }

            if (!IsPathFlatEnough(path, i, 2, 3, maxRampSpawnSlope))
            {
                Vector3 pos = path[i].position + Vector3.up * 3f;
                //Debug.DrawRay(pos, Vector3.up * 5f, Color.green, 100f);
                continue;
            }

            Vector3 prev = path[i - 1].position;
            Vector3 current = path[i].position;
            Vector3 next = path[i + 1].position;

            Vector3 roadDir3D = next - prev;
            roadDir3D.y = 0f;

            if (roadDir3D.sqrMagnitude < 0.001f) continue;

            Vector2 roadForward = new Vector2(roadDir3D.x, roadDir3D.z).normalized;

            float distance = Vector3.Distance(path[i - 4].position, path[i + 4].position);
            float heightDiff = Mathf.Abs(path[i + 4].position.y - path[i - 4].position.y);

            float slope = heightDiff / Mathf.Max(distance, 0.001f);

            candidates.Add(new HoleCandidate
            {
                key = key,
                roadForward = roadForward,
                slope = slope,
                pathIndex = i
            });
        }
        PathFinder.Shuffle(candidates);

        foreach (HoleCandidate candidate in candidates)
        {
            if (placed >= holeCount) break;
            //if (candidate.slope > 0.12f) continue;

            bool tooClose = false;

            // debug show valid candidates apart from minholespacing
            Vector3 worldPos = new Vector3(candidate.key.x, nodeDict[candidate.key].position.y, candidate.key.y);
            //Debug.DrawRay(worldPos, Vector3.up * 10f, Color.yellow, 100f);

            foreach (Vector2Int pothole in placedPotholes)
            {
                if (Vector2Int.Distance(pothole, candidate.key) < minHoleSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue;
          
            float depth = Random.Range(holeDepthRange.x, holeDepthRange.y);

            CreateRampHoles(nodeDict, candidate.key, trenchRadius, depth, vertexDist, roadMask, candidate.roadForward);
            SpawnRamp(nodeDict, path, candidate, trenchRadius, depth, vertexDist);

            placedPotholes.Add(candidate.key);
            placed++;
        }

        if (placed < holeCount)
        {
            Debug.LogWarning(
                $"Only placed {placed}/{holeCount} Ramps and holes. Not enough valid road space."
            );
        }

    }

    private void CreateRampHoles(Dictionary<Vector2Int, TerrainNode> nodeDict, Vector2Int center, int radius, float depth, int dist, HashSet<Vector2Int> roadMask, Vector2 roadForward)
    {

        roadForward.Normalize();

        Vector2 roadRight = new Vector2(-roadForward.y, roadForward.x);

        float alongRadius = Mathf.Max(radius, dist);
        float acrossRadius = 5f;

        int nodeRadius = Mathf.CeilToInt(Mathf.Max(alongRadius, acrossRadius) / Mathf.Max(dist, 1f));

        for (int x = -nodeRadius; x <= nodeRadius; x++)
        {
            for (int z = -nodeRadius; z <= nodeRadius; z++)
            {
                Vector2Int key = center + new Vector2Int(x * dist, z * dist);

                if (!nodeDict.ContainsKey(key)) continue;

                if (!roadMask.Contains(key)) continue;

                Vector2 offset = new Vector2(key.x - center.x, key.y - center.y);

                float distance = offset.magnitude;

                /*if (distance > 0.001f)
                {
                    Vector2 dirFromCenter = offset / distance;

                    float roadDirectionAmount = Mathf.Abs(Vector2.Dot(dirFromCenter, roadForward));
                    bool isInRoadDirection = roadDirectionAmount >= 0.7f;
                    bool isFarEnough = distance >= 2f;

                    if (isInRoadDirection && isFarEnough)
                        continue;
                }*/


                float alongRoad = Vector2.Dot(offset, roadForward);
                float acrossRoad = Vector2.Dot(offset, roadRight);

                float along01 = Mathf.Abs(alongRoad) / alongRadius;
                float across01 = Mathf.Abs(acrossRoad) / acrossRadius;

                if (along01 > 1f)
                    continue;

                if (across01 > 1f)
                    continue;

                float alongFalloff = 1f - Mathf.SmoothStep(0f, 1f, along01);
                //float acrossFalloff = 1f - Mathf.SmoothStep(0.85f, 1f, across01);

                float falloff = alongFalloff;

                TerrainNode node = nodeDict[key];

                node.position.y -= depth * falloff;

                node.height = node.position.y;

                node.type = TerrainNode.TerrainType.BIGHOLE;
            }
        }

    }

    private void SpawnRamp(Dictionary<Vector2Int, TerrainNode> nodeDict, List<TerrainNode> path, HoleCandidate candidate, int radius, float depth, int dist)
    {
        Vector2 roadForward = candidate.roadForward.normalized;

        Vector2 rampOffset = roadForward * (-radius + dist);

        Vector2 rampXZ = new Vector2(candidate.key.x, candidate.key.y) + rampOffset;

        Vector2Int rampKey = new Vector2Int(Mathf.RoundToInt(rampXZ.x), Mathf.RoundToInt(rampXZ.y));

        Vector3 rampWorldPosition = path[candidate.pathIndex - 3].position;

        /*if (nodeDict.TryGetValue(rampKey, out TerrainNode rampNode))
        {
            //rampWorldPosition = rampNode.position;
            TerrainNode holeNode = path[candidate.pathIndex - 2];

            rampWorldPosition = new Vector3(rampXZ.x, holeNode.position.y, rampXZ.y);
        }
        else
        {
            TerrainNode holeNode = path[candidate.pathIndex - 2];

            rampWorldPosition = new Vector3(rampXZ.x, holeNode.position.y, rampXZ.y);
        }*/

        // rampWorldPosition.y += depth;

        Debug.DrawRay(rampWorldPosition + Vector3.up * 5, -Vector3.up * 10f, Color.red, 100f);

        RaycastHit groundHit;
        if (!Physics.Raycast(rampWorldPosition + Vector3.up * 5, -Vector3.up, out groundHit, 10f, groundLayer))
        {
            return;
        }

        Vector3 roadForward3D = new Vector3(roadForward.x, 0f, roadForward.y);
        Vector3 roadForwardOnroad = Vector3.ProjectOnPlane(roadForward3D, groundHit.normal).normalized;

        Quaternion rampRotation = Quaternion.LookRotation(roadForwardOnroad, groundHit.normal);

        Instantiate(rampPrefab, rampWorldPosition, rampRotation);

    }

    private bool IsPathFlatEnough(List<TerrainNode> path, int centerIndex, int nodesBefore, int nodesAfter, float maxSlope)
    {
        int start = Mathf.Max(0, centerIndex - nodesBefore);
        int end = Mathf.Min(path.Count - 1, centerIndex + nodesAfter);
        float baseHeight = path[start].position.y;

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int i = start; i < end; i++)
        {
            Vector3 current = path[i].position;
            Vector3 next = path[i + 1].position;

            float distance = Vector3.Distance(current, next);
            float heightDiff = next.y - current.y;

            float uphill = Mathf.Max(0f, heightDiff);

            float slope = Mathf.Abs(heightDiff) / Mathf.Max(distance, 0.001f);

            if (heightDiff > 0f && slope > 0.08f)
            {
                return false;
            }

            if (heightDiff < 0f && slope > 3f)
            {
                return false;
            }
        }

        return true;

        //return maxHeight - minHeight <= 0.6f;
    }

    

    private bool IsAngleTooSharpInWindow(List<TerrainNode> path, int centerIndex, int nodesBefore, int nodesAfter, int offset, float maxAngle)
    {
        int start = Mathf.Max(offset, centerIndex - nodesBefore);
        int end = Mathf.Min(path.Count - 1 - offset, centerIndex + nodesAfter);

        for (int i = start; i <= end; i++)
        {
            if (IsAngleTooSharp(path, i, offset, maxAngle))
                return true;
        }

        return false;
    }

    private bool IsAngleTooSharp(List<TerrainNode> path, int index, int offset, float maxAngle)
    {
        int beforeIndex = index - offset;
        int afterIndex = index + offset;

        if (beforeIndex < 0 || afterIndex >= path.Count)
            return true;

        Vector3 beforeDir = path[index].position - path[beforeIndex].position;
        Vector3 afterDir = path[afterIndex].position - path[index].position;

        beforeDir.y = 0f;
        afterDir.y = 0f;

        if (beforeDir.sqrMagnitude < 0.001f || afterDir.sqrMagnitude < 0.001f)
            return true;

        float angle = Vector3.Angle(beforeDir.normalized, afterDir.normalized);

        return angle > maxAngle;
    }


}
