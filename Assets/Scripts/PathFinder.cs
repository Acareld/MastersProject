
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering.UI;


public class TerrainNode
{
    public int index;
    public Vector3 position;
    public float height;
    public List<TerrainNode> neighbours = new List<TerrainNode>();

    public float gCost;
    public float hCost;

    public float FCost => gCost + hCost;

    public int stepsFromStart;

    public TerrainNode parent;

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
}

[System.Serializable]
public class DifficultyPhase
{
    public int startStep;
    public int endStep;
    public float modifier;

    public DifficultyPhase(int startStep, int endStep, float modifier)
    {
        this.startStep = startStep;
        this.endStep = endStep;
        this.modifier = modifier;
    }
}


public class PathFinder : MonoBehaviour
{
    [SerializeField]
    private int roadRadiusInNodes = 1;

    [SerializeField]
    private int slopeDifficultyPhases = 1;

    [SerializeField]
    private int roadWidthDifficultyPhases = 1;

    [SerializeField]
    private Vector2Int difficultyPhasesSizeConstraints = new Vector2Int(5, 25);

    [SerializeField]
    private Vector2Int difficultyPhasesGapConstraints = new Vector2Int(5, 25);

    [SerializeField]
    private DifficultyPhase[] difficultyPhases;

    [SerializeField]
    Vector2Int startPosition;

    [SerializeField]
    Vector2Int targetPosition;

    public TerrainNode[] nodes;

    public Dictionary<Vector2Int, TerrainNode> nodeDict = new Dictionary<Vector2Int, TerrainNode>();

    public TerrainNode startNode;
    public TerrainNode targetNode;

    private List<TerrainNode> openSet = new List<TerrainNode>();
    private HashSet<TerrainNode> closedSet = new HashSet<TerrainNode>();

    private int dist;

    public void BuildNodeNetwork(Vector3[] vertices, float dist)
    {

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

        startNode = nodeDict[new Vector2Int(minX, minZ)];
        targetNode = nodeDict[new Vector2Int(maxX, maxZ)];
       
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

                float moveCost = current.gCost + Vector3.Distance(current.position, neighbour.position);

                float heightDifference = neighbour.height - current.height;

                float uphillModifier = 10f;

                for (int i = 0; i < difficultyPhases.Length; i++)
                {
                    if (current.stepsFromStart >= difficultyPhases[i].startStep && current.stepsFromStart < difficultyPhases[i].endStep)
                    {
                        uphillModifier = 0f;
                        break;
                    }
                    else if (current.stepsFromStart >= difficultyPhases[i].endStep)
                    {
                        uphillModifier = 10f;
                    }
                }

                if (heightDifference > 0)
                {
                    moveCost += heightDifference * uphillModifier;
                }

                // moveCost += Mathf.Abs(neighbour.height - current.height) * 10f;

                if (moveCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = moveCost;
                    neighbour.hCost = Heuristic(neighbour, targetNode);

                    neighbour.parent = current;

                    neighbour.stepsFromStart = current.stepsFromStart + 1;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        return new Dictionary<Vector2Int, TerrainNode>();
    }

    private float Heuristic(TerrainNode a, TerrainNode target)
    {
        return Vector3.Distance(a.position, target.position);
    }

    private Vector2Int NodeKey(TerrainNode node)
    {
        return new Vector2Int(
            Mathf.RoundToInt(node.position.x),
            Mathf.RoundToInt(node.position.z)
        );
    }

    private void AddRoadHeight(
    Dictionary<Vector2Int, List<float>> roadHeights,
    Vector2Int key,
    float height
)
    {
        if (!roadHeights.ContainsKey(key))
        {
            roadHeights[key] = new List<float>();
        }

        roadHeights[key].Add(height);
    }

    private void CollectRoadBrush(TerrainNode centerNode, float centerHeight, Dictionary<Vector2Int, List<float>> roadHeights)
    {
        Vector2Int center = NodeKey(centerNode);

        for (int x = -roadRadiusInNodes; x <= roadRadiusInNodes; x++)
        {
            for (int z = -roadRadiusInNodes; z <= roadRadiusInNodes; z++)
            {
                Vector2Int key = center + new Vector2Int(x * dist, z * dist);

                if (!nodeDict.ContainsKey(key))
                    continue;

                AddRoadHeight(roadHeights, key, centerHeight);
            }
        }
    }

    private Dictionary<Vector2Int, float> SmoothPathHeights(List<TerrainNode> path)
    {
        Dictionary<Vector2Int, float> smoothedHeights =
            new Dictionary<Vector2Int, float>();

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

            smoothedHeights[NodeKey(path[i])] = sum / count;
        }

        return smoothedHeights;
    }

    private void GenerateSlopeDifficulties(int maxStepSize)
    {
        difficultyPhases = new DifficultyPhase[difficultyPhasesSizeConstraints.y];
        int currentStep = 0;
        for(int i = 0; i < difficultyPhases.Length; i++)
        {
            int size = Random.Range(difficultyPhasesSizeConstraints.x, difficultyPhasesSizeConstraints.y);
            int start = Random.Range(currentStep + difficultyPhasesGapConstraints.x, currentStep + difficultyPhasesGapConstraints.y);

            currentStep = start + size;
            if(currentStep > maxStepSize)
            {
                return;
            }
            difficultyPhases[i] = new DifficultyPhase(start, start + size, 0f);
        }
    }

    private Dictionary<Vector2Int, TerrainNode> DrawPath()
    {
        List<TerrainNode> path = new List<TerrainNode>();

        TerrainNode current = targetNode;

        while (current != startNode)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Add(startNode);
        path.Reverse();

        Dictionary<Vector2Int, float> smoothedPathHeights = SmoothPathHeights(path);

        Dictionary<Vector2Int, List<float>> roadHeights = new Dictionary<Vector2Int, List<float>>();

       // GenerateSlopeDifficulties(path[path.Count - 1].stepsFromStart);

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int key = NodeKey(path[i]);
            float height = smoothedPathHeights[key];
            bool isDifficult = false;
            for (int j = 0; j < difficultyPhases.Length; j++)
            {
                if (path[i].stepsFromStart >= difficultyPhases[j].startStep && path[i].stepsFromStart < difficultyPhases[j].endStep)
                {
                    isDifficult = true;
                    break;
                }
                else
                {
                    isDifficult = false;
                }

            }

            if(isDifficult)
            {
                roadRadiusInNodes = 1;
            }
            else
            {
                roadRadiusInNodes = 2;
            }
            CollectRoadBrush(path[i], height, roadHeights);

            if (i < path.Count - 1)
            {
                
                if (isDifficult)
                {
                    Debug.DrawLine(path[i].position, path[i + 1].position, Color.green, 100f);
                }
                else
                {
                    Debug.DrawLine(path[i].position, path[i + 1].position, Color.red, 100f);
                }
            }
        }

        foreach (KeyValuePair<Vector2Int, List<float>> entry in roadHeights)
        {
            float sum = 0f;

            foreach (float height in entry.Value)
            {
                sum += height;
            }

            float averageHeight = sum / entry.Value.Count;

            nodeDict[entry.Key].position.y = averageHeight;
        }

        return nodeDict;  
    }

}
