
using System.Collections.Generic;
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
}


public class PathFinder : MonoBehaviour
{
    [SerializeField]
    private int difficultyChangeStepsStart = 30;
    [SerializeField]
    private int difficultyChangeStepsEnd = 45;

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

    public void BuildNodeNetwork(Vector3[] vertices, float dist)
    {
        nodes = new TerrainNode[vertices.Length];
        int size = (int) Mathf.Sqrt(vertices.Length);

        for(int i = 0; i < vertices.Length; i++)
        {
            nodes[i] = new TerrainNode();
            nodes[i].index = i;
            nodes[i].position = vertices[i];
            nodes[i].height = vertices[i].y;
            
            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(Mathf.Round(nodes[i].position.x)), Mathf.FloorToInt(Mathf.Round(nodes[i].position.z)));
            if(!nodeDict.ContainsKey(pos))
            {
                nodeDict.Add(pos, nodes[i]);
            }
        }

        for(int i = 0; i < nodes.Length; i++)
        {
            TerrainNode node = nodes[i];
            Vector2Int pos = new Vector2Int(Mathf.FloorToInt(Mathf.Round(nodes[i].position.x)), Mathf.FloorToInt(Mathf.Round(nodes[i].position.z)));
            Vector2Int right = pos + new Vector2Int((int) dist, 0);
            Vector2Int left = pos + new Vector2Int((int) -dist, 0);
            Vector2Int up = pos + new Vector2Int(0, (int) dist);
            Vector2Int down = pos + new Vector2Int(0, (int) -dist);

            if (nodeDict.ContainsKey(right))
            {
                node.neighbours.Add(nodeDict[right]);
            }
            if(nodeDict.ContainsKey(left))
            {
                node.neighbours.Add(nodeDict[left]);
            }
            if(nodeDict.ContainsKey(up))
            {
                node.neighbours.Add(nodeDict[up]);
            }
            if(nodeDict.ContainsKey(down))
            {
                node.neighbours.Add(nodeDict[down]);
            }
        }

        startNode = nodeDict[startPosition];
        targetNode = nodeDict[targetPosition];
    }

    public void AStar()
    {
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            TerrainNode current = openSet[0];

            foreach(TerrainNode node in openSet)
            {
                if(node.FCost < current.FCost)
                {
                    current = node;
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
            {
                DrawPath();
                return;
            }

            foreach(TerrainNode neighbour in current.neighbours)
            {
                if(closedSet.Contains(neighbour))
                {
                    continue;
                }

                float moveCost = current.gCost + Vector3.Distance(current.position, neighbour.position);

                float heightDifference = neighbour.height - current.height;

                float uphillModifier = 10f;

                for(int i = 0; i < difficultyPhases.Length; i++)
                {
                    if (current.stepsFromStart >= difficultyPhases[i].startStep && current.stepsFromStart < difficultyPhases[i].endStep)
                    {
                        uphillModifier = 0f;
                        break;
                    }
                    else if (current.stepsFromStart >= difficultyChangeStepsEnd)
                    {
                        uphillModifier = 10f;
                    }
                }

                if (heightDifference > 0)
                {
                    moveCost += heightDifference * uphillModifier;
                }

               // moveCost += Mathf.Abs(neighbour.height - current.height) * 10f;

                if(moveCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = moveCost;
                    neighbour.hCost = Heuristic(neighbour, targetNode);

                    neighbour.parent = current;

                    neighbour.stepsFromStart = current.stepsFromStart + 1;

                    if(!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }

    private float Heuristic(TerrainNode a, TerrainNode target)
    {
        return Vector3.Distance(a.position, target.position);
    }

    private void DrawPath()
    {
        List<TerrainNode> path = new List<TerrainNode>();

        TerrainNode current = targetNode;

        while (current != startNode)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();

        for(int i = 0; i < path.Count - 1; i++)
        {
            bool isDifficult = false;
            for(int j = 0; j < difficultyPhases.Length; j++)
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

}
