
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;


public class TerrainNode
{
    public int index;
    public Vector3 position;
    public float height;
    public List<TerrainNode> neighbours = new List<TerrainNode>();

    public float gCost;
    public float hCost;

    public float FCost => gCost + hCost;


    public TerrainNode parent;

    public TerrainType type = TerrainType.TERRAIN;

 



    public enum TerrainType
    {
        TERRAIN,
        ROAD,
        HOLE,
        BIGHOLE,
        RAMP
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(position + ": ");
        foreach (TerrainNode node in neighbours)
        {
            sb.Append(node.position + ", ");
        }
        return sb.ToString();
    }

    public static Vector2Int NodeKey(TerrainNode node)
    {
        return new Vector2Int(
            Mathf.RoundToInt(node.position.x),
            Mathf.RoundToInt(node.position.z)
        );
    }
}

[System.Serializable]
public class DifficultyPhase
{
    public float startProgress;
    public float endProgress;
    public float uphillModifier;
    public float targetSlope;
    public float slopeMatchWeight;


    public DifficultyPhase(float startProgress, float endProgress, float uphillModifier, float targetSlope, float slopeMatchWeight)
    {
        this.startProgress = startProgress;
        this.endProgress = endProgress;
        this.uphillModifier = uphillModifier;
        this.targetSlope = targetSlope;
        this.slopeMatchWeight = slopeMatchWeight;
    }

    public static DifficultyPhase GetDefault()
    {
        return new DifficultyPhase(0f, 0f, 10f, 0.08f, 2f);
    }


}


public class PathFinder : MonoBehaviour
{
    [SerializeField]
    private int roadRadiusInNodes = 1;

    [SerializeField]
    private Vector2Int difficultyPhasesSizeConstraints = new Vector2Int(5, 25);

    [SerializeField]
    private Vector2Int difficultyPhasesGapConstraints = new Vector2Int(5, 25);

    [SerializeField]
    private List<DifficultyPhase> difficultyPhases;

    private List<TerrainNode> path;

    private HashSet<Vector2Int> roadMask;


    [SerializeField]
    Vector2Int startPosition;

    [SerializeField]
    Vector2Int targetPosition;

    [SerializeField] private int seamSampleRadius = 10;
    [SerializeField] private int seamSampleDepthBehind = 6;
    [SerializeField] private int seamSampleDepthAhead = 1;

    public TerrainNode[] nodes;

    public Dictionary<Vector2Int, TerrainNode> nodeDict = new Dictionary<Vector2Int, TerrainNode>();

    public TerrainNode startNode;
    public TerrainNode targetNode;

    public List<TerrainNode> exitNodes = new List<TerrainNode>();


    private List<TerrainNode> openSet = new List<TerrainNode>();
    private HashSet<TerrainNode> closedSet = new HashSet<TerrainNode>();

    private int dist;

    private ObstacleGenerator obstacleGenerator;

    private DifficultyState difficultyState;

    private RoadConnector exitRoadConnector;

    private int nodesPerFrame = 9000;

    private float msPerFrame = 6f;

    private TerrainNode triggerPoint;
    private TerrainNode terrainTriggerPoint;

    private int gridMinX;
    private int gridMinZ;
    private int nodeSpacing;
   
    void Awake()
    {
        obstacleGenerator = GetComponent<ObstacleGenerator>();
    }

    public IEnumerator BuildNodeNetworkRoutine(Vector3[] vertices, float dist)
    {
        openSet.Clear();
        closedSet.Clear();
        nodeDict.Clear();

        this.dist = (int)dist;
        nodes = new TerrainNode[vertices.Length];
        int size = (int)Mathf.Sqrt(vertices.Length);

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minZ = int.MaxValue;
        int maxZ = int.MinValue;

        int processedThisFrame = 0;

        for (int i = 0; i < vertices.Length; i++)
        {
            nodes[i] = new TerrainNode();
            nodes[i].index = i;
            nodes[i].position = vertices[i];
            nodes[i].height = vertices[i].y;

            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(Mathf.Round(nodes[i].position.x)), Mathf.FloorToInt(Mathf.Round(nodes[i].position.z)));
            if (!nodeDict.ContainsKey(pos))
            {
                nodeDict.Add(pos, nodes[i]);

                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minZ) minZ = pos.y;
                if (pos.y > maxZ) maxZ = pos.y;
            }

            processedThisFrame++;

            if (processedThisFrame >= nodesPerFrame)
            {
                processedThisFrame = 0;
                yield return null;
            }
        }

        processedThisFrame = 0;

        int d = this.dist * 4;

        for (int i = 0; i < nodes.Length; i++)
        {
            TerrainNode node = nodes[i];
            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(Mathf.Round(nodes[i].position.x)), Mathf.FloorToInt(Mathf.Round(nodes[i].position.z)));
            

            TryAddNeighbour(node, pos + new Vector2Int(d, 0));
            TryAddNeighbour(node, pos + new Vector2Int(-d, 0));
            TryAddNeighbour(node, pos + new Vector2Int(0, d));
            TryAddNeighbour(node, pos + new Vector2Int(0, -d));

            TryAddNeighbour(node, pos + new Vector2Int(d, d));
            TryAddNeighbour(node, pos + new Vector2Int(-d, d));
            TryAddNeighbour(node, pos + new Vector2Int(d, -d));
            TryAddNeighbour(node, pos + new Vector2Int(-d, -d));
        

            processedThisFrame++;

            if (processedThisFrame >= nodesPerFrame)
            {
                processedThisFrame = 0;
                yield return null;
            }
        }

        int centerZ = Mathf.FloorToInt(((maxZ - minZ) / 2) + minZ);
        int centerX = Mathf.FloorToInt(((maxX -  minX) / 2) + minX);

        if (gameObject.transform.position.x == 0) startNode = nodeDict[new Vector2Int(centerX + 80, centerZ)];

        targetNode = nodeDict[new Vector2Int(maxX, centerZ)];

        nodeSpacing = Mathf.Max(1, this.dist);
        gridMinX = minX;
        gridMinZ = minZ;
    }

    private void TryAddNeighbour(TerrainNode node, Vector2Int key)
    {
        if(nodeDict.TryGetValue(key ,out TerrainNode neighbour))
        {
            node.neighbours.Add(neighbour);
        }
    }

    public void SetEntryConnector(RoadConnector entryConnector)
    {
        TerrainNode closest = null;
        float dist = float.MaxValue;

        foreach (TerrainNode node in nodeDict.Values)
        {
            float distance = Vector2.Distance(new Vector2(node.position.x, node.position.z), new Vector2(entryConnector.worldPosition.x, entryConnector.worldPosition.z));

            if (distance < dist)
            {
                dist = distance;
                closest = node;
            }
        }

        if (closest != null)
        {
            startNode = closest;
            Debug.Log(entryConnector.worldPosition);
        }

        exitNodes = entryConnector.lastNodes;
    }

    public void ForceStartHeight(float height)
    {
        if (startNode == null)
        {
            return;
        }

        startNode.position.y = height;
        startNode.height = height;
    }



    public void SetDifficultySettings(DifficultyState state)
    {
        difficultyPhases = state.slopePhases;
        difficultyState = state;
    }

    public TerrainNode GetExitNode()
    {
        return path[path.Count - 1];
    }
    public void SetExitNode(TerrainNode node)
    {
       
    }

    public IEnumerator AStarRoutine(System.Action<Dictionary<Vector2Int, TerrainNode>> onComplete)
    {

        foreach(TerrainNode node in nodeDict.Values)
        {
            node.gCost = float.PositiveInfinity;
            node.hCost = 0f;
            node.parent = null;
        }

        openSet.Clear();
        closedSet.Clear();

        startNode.gCost = 0f;
        startNode.hCost = Heuristic(startNode, targetNode);
        startNode.parent = null;

        openSet.Add(startNode);

        int totalIterations = 0;

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        while (openSet.Count > 0)
        {
            totalIterations++;
            if (totalIterations % 1000 == 0)
            {
                //Debug.Log($"A* iterations: {totalIterations}, openSet: {openSet.Count}, closedSet: {closedSet.Count}");
            }

            TerrainNode current = openSet[0];

            foreach (TerrainNode node in openSet)
            {
                if (node.FCost < current.FCost)
                {
                    current = node;
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
            {
                Dictionary<Vector2Int, TerrainNode> result = DrawPath();
                onComplete?.Invoke(result);
                yield break;
            }

            foreach (TerrainNode neighbour in current.neighbours)
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                float distance = HorizontalDistance(current, neighbour); 
                float moveCost = current.gCost + distance; 
                float heightDifference = neighbour.height - current.height; 
                DifficultyPhase phase = UphillModifier(current.position);
                float slope = heightDifference / Mathf.Max(distance, 0.001f);

                if (heightDifference > 0) 
                { 
                    moveCost += heightDifference * phase.uphillModifier; 
                    
                    float slopeMatchCost = Mathf.Abs(slope - phase.targetSlope) * phase.slopeMatchWeight; 

                    float slopeShortFall = Mathf.Max(0f, phase.targetSlope - slope); 
                    slopeMatchCost = phase.slopeMatchWeight * slopeShortFall; 
                    moveCost += slopeMatchCost; 
                } 
                else 
                { 
                    moveCost += Mathf.Abs(heightDifference) * phase.uphillModifier;
                    moveCost += (-slope) * 5 * distance;
                }

                if (moveCost < neighbour.gCost)
                {
                    neighbour.gCost = moveCost;
                    neighbour.hCost = Heuristic(neighbour, targetNode);

                    neighbour.parent = current;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }

            if (stopwatch.Elapsed.TotalMilliseconds >= msPerFrame)
            {
                stopwatch.Restart();
                yield return null;
            }


        }
        Debug.LogError(
    $"A* found no path. " +
    $"Start: {TerrainNode.NodeKey(startNode)}, " +
    $"Target: {TerrainNode.NodeKey(targetNode)}, " +
    $"Closed nodes: {closedSet.Count}, " +
    $"Total nodes: {nodeDict.Count}",
    this);
        onComplete?.Invoke(new Dictionary<Vector2Int, TerrainNode>());
    }

    private float HorizontalDistance(TerrainNode a, TerrainNode b)
    {
        return Vector2.Distance(new Vector2(a.position.x, a.position.z), new Vector2(b.position.x, b.position.z));
    }


    private float Heuristic(TerrainNode start, TerrainNode target)
    {
        return HorizontalDistance(start, target) * 0.15f;
    }

    private float ProgressAlongPath(Vector3 current, Vector3 start, Vector3 target)
    {
        Vector3 path = target - start;
        Vector3 fromStart = current - start;

        float lengthSquared = path.sqrMagnitude;

        if (lengthSquared <= 0.0001f)
        {
            return 0f;
        }

        float progress = Vector3.Dot(fromStart, path) / lengthSquared;

        return Mathf.Clamp01(progress);
    }

    private DifficultyPhase UphillModifier(Vector3 position)
    {
        float progress = ProgressAlongPath(position, startNode.position, targetNode.position);

        for (int i = 0; i < difficultyPhases.Count; i++)
        {
            if (progress >= difficultyPhases[i].startProgress &&
                progress < difficultyPhases[i].endProgress)
            {
                return difficultyPhases[i];
            }
        }

        return DifficultyPhase.GetDefault();
    }

    private struct HeightBlend
    {
        public float heightSum;
        public float weightSum;
        public float distanceFromCenter;
    }

    private void AddRoadHeight(Dictionary<Vector2Int, HeightBlend> roadHeights, Vector2Int key, float height, float weight, float distanceFromCenter)
    {
        if (!roadHeights.ContainsKey(key))
        {
            roadHeights[key] = new HeightBlend()
            {
                distanceFromCenter = float.PositiveInfinity
            };
        }

        HeightBlend blend = roadHeights[key];

        blend.heightSum += height * weight;
        blend.weightSum += weight;
        blend.distanceFromCenter = Mathf.Min(distanceFromCenter, blend.distanceFromCenter);

        roadHeights[key] = blend;
    }

    public bool TryGetSurfaceType(Vector3 worldPosition, out TerrainNode.TerrainType surfaceType)
    {
        surfaceType = TerrainNode.TerrainType.TERRAIN;

        if(nodeDict == null || nodeDict.Count == 0)
        {
            Debug.Log("NodeDict null");
            return false;
        }

        int snappedX = gridMinX + Mathf.RoundToInt((worldPosition.x - gridMinX) / nodeSpacing) * nodeSpacing;
        int snappedZ = gridMinZ + Mathf.RoundToInt((worldPosition.z - gridMinZ) / nodeSpacing) * nodeSpacing;

        Vector2Int key = new Vector2Int(snappedX, snappedZ);

        if(!nodeDict.TryGetValue(key, out TerrainNode node))
        {
            Debug.Log("No node found");
            return false;
        }

        surfaceType = node.type;
        return true;
    }

    private void CollectRoadBrushWorld(Vector3 centerPosition, float centerHeight, Dictionary<Vector2Int, HeightBlend> roadHeights)
    {
        Vector2Int center = new Vector2Int(Mathf.RoundToInt(centerPosition.x), Mathf.RoundToInt(centerPosition.z));

        int totalRadius = roadRadiusInNodes + 1;


        for (int x = -totalRadius; x <= totalRadius; x++)
        {
            for (int z = -totalRadius; z <= totalRadius; z++)
            {
                float distanceFromCenter = Mathf.Sqrt(x * x + z * z);

                if (distanceFromCenter > totalRadius)
                    continue;

                Vector2Int key = center + new Vector2Int(x * dist, z * dist);

                if (!nodeDict.ContainsKey(key))
                    continue;

                float weight;

                if (distanceFromCenter <= roadRadiusInNodes)
                {
                    float t = distanceFromCenter / roadRadiusInNodes;
                    weight = Mathf.Lerp(1f, 0.6f, t);
                }
                else
                {
                    float shoulderT = distanceFromCenter - roadRadiusInNodes;
                    weight = Mathf.Lerp(0.4f, 0f, shoulderT);
                }

                weight = Mathf.SmoothStep(0f, 1f, weight);

                AddRoadHeight(roadHeights, key, centerHeight, weight, distanceFromCenter);
            }
        }
    }

    public RoadConnector GetExitRoadConnector()
    {
        return exitRoadConnector;
    }

    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private float GetSmoothedPathHeight(List<TerrainNode> path, int index, int radius = 2)
    {
        float sum = 0f;
        int count = 0;

        int start = Mathf.Max(0, index - radius);
        int end = Mathf.Min(path.Count - 1, index + radius);

        for (int i = start; i <= end; i++)
        {
            sum += path[i].position.y;
            count++;
        }

        return sum / count;
    }


    private Dictionary<Vector2Int, TerrainNode> DrawPath()
    {
        path = new List<TerrainNode>();

        TerrainNode current = targetNode;

        while (current != startNode)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Add(startNode);
      
        path.Reverse();

        path = RebuildPathWithIntermediateNodes(path);

        //Dictionary<Vector2Int, float> smoothedPathHeights = SmoothPathHeights(path);

        Dictionary<Vector2Int, HeightBlend> roadHeights = new Dictionary<Vector2Int, HeightBlend>();

        float roadBrushSpacing = dist;

        for (int i = 0; i < path.Count - 1; i++)
        {
            TerrainNode cur = path[i];
            TerrainNode next = path[i + 1];

            //Debug.Log("Path Node index " + i + ": " + cur);

            cur.type = TerrainNode.TerrainType.ROAD;
            next.type = TerrainNode.TerrainType.ROAD;

            bool isDifficult = false;

            float progress = ProgressAlongPath(path[i].position, startNode.position, targetNode.position);

            if(progress >= 0.75 && triggerPoint == null)
            {
                triggerPoint = cur;
            }
            if(progress >= 0.1 && terrainTriggerPoint == null)
            {
                terrainTriggerPoint = cur;
            }

            for (int j = 0; j < difficultyPhases.Count; j++)
            {
                if (progress >= difficultyPhases[j].startProgress &&
                    progress < difficultyPhases[j].endProgress)
                {
                    isDifficult = true;
                    break;
                }
            }

            roadRadiusInNodes = isDifficult ? 4 : 6;

            float aHeight = GetSmoothedPathHeight(path, i, 2);
            float bHeight = GetSmoothedPathHeight(path, i + 1, 2);

            float segmentLength = Vector3.Distance(cur.position, next.position);

            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(segmentLength / roadBrushSpacing));

            CollectRoadBrush(cur, cur.height, roadHeights);

            for (int s = 0; s <= sampleCount; s++)
            {
                float t = s / (float)sampleCount;

                Vector3 samplePosition = Vector3.Lerp(cur.position, next.position, t);
                float sampleHeight = Mathf.Lerp(aHeight, bHeight, t);

                //CollectRoadBrushWorld(samplePosition, sampleHeight, roadHeights);
            }

            if (i < path.Count - 1)
            {

                //if (isDifficult)
                //{
                //    Debug.DrawLine(path[i].position, path[i + 1].position, Color.green, 100f);
                //}
                //else
                //{
                //    Debug.DrawLine(path[i].position, path[i + 1].position, Color.red, 100f);
                //}
            }
        }

        roadMask = new HashSet<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, HeightBlend> entry in roadHeights)
        {
            HeightBlend blend = entry.Value;

            if (blend.weightSum <= 0f)
                continue;

            TerrainNode node = nodeDict[entry.Key];

            float averageHeight = blend.heightSum / blend.weightSum;

            float blendStrength = Mathf.Clamp01(blend.weightSum);

            float finalHeight = Mathf.Lerp(node.position.y, averageHeight, blendStrength);

            node.position.y = finalHeight;
            node.height = finalHeight;

            float roadTypeRadius = roadRadiusInNodes - 1;

            if (blend.distanceFromCenter <= roadTypeRadius)
            {
                node.type = TerrainNode.TerrainType.ROAD;
                roadMask.Add(entry.Key);
            }
            else
            {
                node.type = TerrainNode.TerrainType.TERRAIN;
            }

            //node.type = TerrainNode.TerrainType.ROAD;
            //roadMask.Add(entry.Key);
        }

        obstacleGenerator.GenerateObstacles(difficultyState, nodeDict, path, roadMask, dist);

        StoreExitConnector(path);

        return nodeDict;
    }

    private List<TerrainNode> RebuildPathWithIntermediateNodes(List<TerrainNode> coarsePath)
    {
        List<TerrainNode> densePath = new List<TerrainNode>();

        if (coarsePath == null || coarsePath.Count == 0)
            return densePath;

        int nodeSpacing = Mathf.Max(1, dist);
        HashSet<Vector2Int> addedKeys = new HashSet<Vector2Int>();

        void AddNode(TerrainNode node)
        {
            if (node == null)
                return;

            Vector2Int key = TerrainNode.NodeKey(node);

            if (addedKeys.Contains(key))
                return;

            densePath.Add(node);
            addedKeys.Add(key);
        }

        for (int i = 0; i < coarsePath.Count - 1; i++)
        {
            TerrainNode from = coarsePath[i];
            TerrainNode to = coarsePath[i + 1];

            AddNode(from);

            Vector2Int fromKey = TerrainNode.NodeKey(from);
            Vector2Int toKey = TerrainNode.NodeKey(to);

            int dx = toKey.x - fromKey.x;
            int dz = toKey.y - fromKey.y;

            int steps = Mathf.Max(
                Mathf.Abs(dx),
                Mathf.Abs(dz)
            ) / nodeSpacing;

            steps = Mathf.Max(1, steps);

            for (int s = 1; s <= steps; s++)
            {
                float t = s / (float)steps;

                int x = Mathf.RoundToInt(Mathf.Lerp(fromKey.x, toKey.x, t));
                int z = Mathf.RoundToInt(Mathf.Lerp(fromKey.y, toKey.y, t));

                Vector2Int key = new Vector2Int(x, z);

                if (nodeDict.TryGetValue(key, out TerrainNode intermediateNode))
                {
                    AddNode(intermediateNode);
                }
            }
        }

        AddNode(coarsePath[coarsePath.Count - 1]);

        return densePath;
    }

    private void StoreExitConnector(List<TerrainNode> path)
    {
        if (path == null || path.Count < 2)
        {
            exitRoadConnector = new RoadConnector
            {
                isValid = false
            };

            return;
        }

        TerrainNode last = path[path.Count - 1];
        TerrainNode previous = path[path.Count - 2];

        Vector3 direction = (last.position - previous.position).normalized;

        List<TerrainNode> lastNodes = new List<TerrainNode>();
        lastNodes.Add(previous);
        lastNodes.Add(path[path.Count - 3]);

        exitRoadConnector = new RoadConnector
        {
            worldPosition = last.position,
            height = last.position.y,
            direction = direction,
            roadRadius = 4,
            isValid = true,
            lastNodes = lastNodes,
            seamSamples = CollectExitSeamSamples(last, previous)
        };
    }

    private void CollectRoadBrush(TerrainNode centerNode, float centerHeight, Dictionary<Vector2Int, HeightBlend> roadHeights)
    {
        Vector2Int center = TerrainNode.NodeKey(centerNode);

        int totalRadius = roadRadiusInNodes + 1;

        for (int x = -totalRadius; x <= totalRadius; x++)
        {
            for (int z = -totalRadius; z <= totalRadius; z++)
            {
                float distanceFromCenter = Mathf.Sqrt(x * x + z * z);

                if (distanceFromCenter > totalRadius)
                    continue;

                Vector2Int key = center + new Vector2Int(x * dist, z * dist);

                if (!nodeDict.ContainsKey(key))
                    continue;

                float weight;

                if (distanceFromCenter <= roadRadiusInNodes)
                {

                    float t = distanceFromCenter / roadRadiusInNodes;


                    weight = Mathf.Lerp(1f, 0.6f, t);
                }
                else
                {

                    float shoulderT = distanceFromCenter - roadRadiusInNodes;


                    weight = Mathf.Lerp(0.4f, 0f, shoulderT);
                }

                weight = Mathf.SmoothStep(0f, 1f, weight);

                AddRoadHeight(roadHeights, key, centerHeight, weight, distanceFromCenter);
            }
        }
    }

    public Vector3 GetTriggerPointPosition()
    {
        if(triggerPoint == null)
        {
            Debug.Log("NO TriggerPoint");
            return Vector3.zero;
        }
        return triggerPoint.position;
    }

    public Vector3 GetTerrainTriggerPointPosition()
    {
        if (terrainTriggerPoint == null)
        {
            Debug.Log("NO TerrainTriggerPoint");
            return Vector3.zero;
        }
        return terrainTriggerPoint.position;
    }

    public Vector3 GetRespawnPoint()
    {
        return path[0].position;
    }



    // OLD SHIT
    // -----------------------------------------------------------------------------------------------------
    public TerrainNode BuildNodeNetwork(Vector3[] vertices, float dist)
    {

        openSet.Clear();
        closedSet.Clear();
        nodeDict.Clear();

        this.dist = (int)dist;
        nodes = new TerrainNode[vertices.Length];
        int size = (int)Mathf.Sqrt(vertices.Length);

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minZ = int.MaxValue;
        int maxZ = int.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            nodes[i] = new TerrainNode();
            nodes[i].index = i;
            nodes[i].position = vertices[i];
            nodes[i].height = vertices[i].y;
            nodes[i].gCost = float.PositiveInfinity;

            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(Mathf.Round(nodes[i].position.x)), Mathf.FloorToInt(Mathf.Round(nodes[i].position.z)));
            if (!nodeDict.ContainsKey(pos))
            {
                nodeDict.Add(pos, nodes[i]);

                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minZ) minZ = pos.y;
                if (pos.y > maxZ) maxZ = pos.y;
            }
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            TerrainNode node = nodes[i];
            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(Mathf.Round(nodes[i].position.x)), Mathf.FloorToInt(Mathf.Round(nodes[i].position.z)));
            Vector2Int right = pos + new Vector2Int((int)dist, 0);
            Vector2Int left = pos + new Vector2Int((int)-dist, 0);
            Vector2Int up = pos + new Vector2Int(0, (int)dist);
            Vector2Int down = pos + new Vector2Int(0, (int)-dist);

            Vector2Int topRight = pos + new Vector2Int((int)dist, (int)dist);
            Vector2Int topLeft = pos + new Vector2Int((int)-dist, (int)dist);
            Vector2Int bottomRight = pos + new Vector2Int((int)dist, (int)-dist);
            Vector2Int bottomLeft = pos + new Vector2Int((int)-dist, (int)-dist);

            if (nodeDict.ContainsKey(right))
            {
                node.neighbours.Add(nodeDict[right]);
            }
            if (nodeDict.ContainsKey(left))
            {
                node.neighbours.Add(nodeDict[left]);
            }
            if (nodeDict.ContainsKey(up))
            {
                node.neighbours.Add(nodeDict[up]);
            }
            if (nodeDict.ContainsKey(down))
            {
                node.neighbours.Add(nodeDict[down]);
            }
            if (nodeDict.ContainsKey(topRight))
            {
                node.neighbours.Add(nodeDict[topRight]);
            }
            if (nodeDict.ContainsKey(topLeft))
            {
                node.neighbours.Add(nodeDict[topLeft]);
            }
            if (nodeDict.ContainsKey(bottomRight))
            {
                node.neighbours.Add(nodeDict[bottomRight]);
            }
            if (nodeDict.ContainsKey(bottomLeft))
            {
                node.neighbours.Add(nodeDict[bottomLeft]);
            }
        }

        int centerZ = Mathf.FloorToInt(((maxZ - minZ) / 2) + minZ);

        if (gameObject.transform.position.x == 0) startNode = nodeDict[new Vector2Int(minX, centerZ)];

        targetNode = nodeDict[new Vector2Int(maxX, centerZ)];

        return startNode;

        //DebugDrawTerrainSlopes();
        //BuildDifficultyPhases();
    }

    public Dictionary<Vector2Int, TerrainNode> AStar()
    {
        
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            TerrainNode current = openSet[0];

            foreach (TerrainNode node in openSet)
            {
                if (node.FCost < current.FCost)
                {
                    current = node;
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
            {
                return DrawPath();
            }

            foreach (TerrainNode neighbour in current.neighbours)
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                float distance = Vector3.Distance(current.position, neighbour.position);

                float moveCost = current.gCost + distance;

                float heightDifference = neighbour.height - current.height;

                DifficultyPhase phase = UphillModifier(current.position);

                if (heightDifference > 0)
                {
                    moveCost += heightDifference * phase.uphillModifier;
                }

                float slope = Mathf.Abs(heightDifference) / Mathf.Max(distance, 0.001f);

                float slopeMatchCost = Mathf.Abs(slope - phase.targetSlope) * phase.slopeMatchWeight;

                moveCost += slopeMatchCost;

                if (moveCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = moveCost;
                    neighbour.hCost = Heuristic(neighbour, targetNode);

                    neighbour.parent = current;


                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }


        }
        return new Dictionary<Vector2Int, TerrainNode>();
    }

    

    

    private Dictionary<Vector2Int, float> SmoothPathHeights(List<TerrainNode> path)
    {
        Dictionary<Vector2Int, float> smoothedHeights = new Dictionary<Vector2Int, float>();

        for (int i = 0; i < path.Count; i++)
        {
            float sum = path[i].position.y;
            int count = 1;

            if (i > 0)
            {
                sum += path[i - 1].position.y;
                count++;
            }

            if (i < path.Count - 1)
            {
                sum += path[i + 1].position.y;
                count++;
            }

            smoothedHeights[TerrainNode.NodeKey(path[i])] = sum / count;
        }

        return smoothedHeights;
    }

   

    public void ApplyEntrySeam(RoadConnector connector)
    {
        if (!connector.isValid || connector.seamSamples == null)
        {
            return;
        }

        if (startNode == null)
        {
            return;
        }

        Vector2Int entryKey = TerrainNode.NodeKey(startNode);

        foreach (RoadSeamSample sample in connector.seamSamples)
        {
            Vector2Int key = entryKey + sample.offsetFromConnector;

            if (!nodeDict.TryGetValue(key, out TerrainNode node))
            {
                continue;
            }

            Vector3 position = node.position;
            position.y = sample.height;

            node.position = position;
            node.height = sample.height;
            node.type = sample.type;
        }
    }

    

    private List<RoadSeamSample> CollectExitSeamSamples(TerrainNode last, TerrainNode previous)
    {
        List<RoadSeamSample> samples = new List<RoadSeamSample>();

        if (roadMask == null)
        {
            return samples;
        }

        Vector2Int connectorKey = TerrainNode.NodeKey(last);

        Vector2 forward = new Vector2(
            last.position.x - previous.position.x,
            last.position.z - previous.position.z
        ).normalized;

        int radius = Mathf.Max(seamSampleRadius, roadRadiusInNodes + 2);

        foreach (Vector2Int key in roadMask)
        {
            if (!nodeDict.TryGetValue(key, out TerrainNode node))
            {
                continue;
            }

            Vector2Int offset = key - connectorKey;

            Vector2 offset2 = new Vector2(offset.x, offset.y);

            float forwardDistance = Vector2.Dot(offset2, forward);

            if (forwardDistance < -seamSampleDepthBehind)
            {
                continue;
            }

            if (forwardDistance > seamSampleDepthAhead)
            {
                continue;
            }

            if (offset2.magnitude > radius)
            {
                continue;
            }

            samples.Add(new RoadSeamSample
            {
                offsetFromConnector = offset,
                height = node.height,
                type = node.type
            });
        }

        Vector2Int centerOffset = Vector2Int.zero;

        samples.Add(new RoadSeamSample
        {
            offsetFromConnector = centerOffset,
            height = last.height,
            type = last.type
        });

        return samples;
    }

    // DEBUG
    private void DebugDrawTerrainSlopes(float duration = 100f)
    {
        foreach (TerrainNode node in nodeDict.Values)
        {
            foreach (TerrainNode neighbour in node.neighbours)
            {
                float distance = Vector3.Distance(node.position, neighbour.position);
                float heightDifference = neighbour.height - node.height;

                float slope = Mathf.Abs(heightDifference) / Mathf.Max(distance, 0.001f);

                Color color = GetSlopeColor(slope);

                Debug.DrawLine(
                    node.position + Vector3.up * 0.2f,
                    neighbour.position + Vector3.up * 0.2f,
                    color,
                    duration
                );
            }
        }
    }



    private Color GetSlopeColor(float slope)
    {
        if (slope < 0.05f)
        {
            return Color.blue;      // very flat
        }

        if (slope < 0.15f)
        {
            return Color.green;     // gentle
        }

        if (slope < 0.30f)
        {
            return Color.yellow;    // noticeable slope
        }

        if (slope < 0.60f)
        {
            return new Color(1f, 0.5f, 0f); // orange
        }

        return Color.red;           // very steep
    }

}
