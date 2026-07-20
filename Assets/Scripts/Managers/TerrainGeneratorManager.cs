using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[System.Serializable]
public struct RoadSeamSample
{
    public Vector2Int offsetFromConnector;
    public float height;
    public TerrainNode.TerrainType type;
}

[System.Serializable]
public struct RoadConnector
{
    public Vector3 worldPosition;
    public float height;
    public Vector3 direction;
    public int roadRadius;
    public bool isValid;
    public List<TileGeneration> overlapTiles;
    public List<TerrainNode> lastNodes;
    public List<RoadSeamSample> seamSamples;
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
    private int visibleOffset = 400;

    private RoadConnector lastExitRoadConnector;
    private bool bHasExitRoadConnector = false;
    private TerrainNode lastExitNode;

    private List<TerrainGenerator> playableGenerators = new List<TerrainGenerator>();
    private List<TerrainGenerator> visibleGenerators = new List<TerrainGenerator>();

    // Update is called once per frame
    void Update()
    {
        if(bGenerateNextSegment)
        {
            bGenerateNextSegment = false;
        }
    }

    private IEnumerator GenerateNextSegmentCoroutine()
    {

        for(int i = 0; i < 1; i++)
        {
            Vector3 playableGenPosition = new Vector3(nextOffset, 0, 0);
            Vector3 visibleGenPositionR = new Vector3(nextOffset, 0, -visibleOffset);
            Vector3 visibleGenPositionL = new Vector3(nextOffset, 0, visibleOffset);

            GameObject terrainGen = Instantiate(terrainGeneratorPrefab, playableGenPosition, Quaternion.identity);
            GameObject terrainGenR = Instantiate(terrainGeneratorPrefab, visibleGenPositionR, Quaternion.identity);
            GameObject terrainGenL = Instantiate(terrainGeneratorPrefab, visibleGenPositionL, Quaternion.identity);


            TerrainGenerator gen = terrainGen.GetComponent<TerrainGenerator>();

            TerrainGenerator visibleGenR = terrainGenR.GetComponent<TerrainGenerator>();
            TerrainGenerator visibleGenL = terrainGenL.GetComponent<TerrainGenerator>();

            if (playableGenerators.Count == 0)
            {
                Vector3 playableGenPositionB = new Vector3(nextOffset - generatorOffset, 0, 0);
                Vector3 visibleGenPositionBR = new Vector3(nextOffset - generatorOffset, 0, -visibleOffset);
                Vector3 visibleGenPositionBL = new Vector3(nextOffset - generatorOffset, 0, visibleOffset);

                GameObject terrainGenB = Instantiate(terrainGeneratorPrefab, playableGenPositionB, Quaternion.identity);
                GameObject terrainGenBR = Instantiate(terrainGeneratorPrefab, visibleGenPositionBR, Quaternion.identity);
                GameObject terrainGenBL = Instantiate(terrainGeneratorPrefab, visibleGenPositionBL, Quaternion.identity);

                TerrainGenerator visibleGenB = terrainGenB.GetComponent<TerrainGenerator>();
                TerrainGenerator visibleGenBR = terrainGenBR.GetComponent<TerrainGenerator>();
                TerrainGenerator visibleGenBL = terrainGenBL.GetComponent<TerrainGenerator>();

                yield return visibleGenB.GenerateVisibleAndWait(false);
                yield return visibleGenBR.GenerateVisibleAndWait(false);
                yield return visibleGenBL.GenerateVisibleAndWait(false);

                visibleGenerators.Add(visibleGenB);
                visibleGenerators.Add(visibleGenBR);
                visibleGenerators.Add(visibleGenBL);
            }

            yield return visibleGenR.GenerateVisibleAndWait(false);
            yield return visibleGenL.GenerateVisibleAndWait(false);

            visibleGenerators.Add(visibleGenR);
            visibleGenerators.Add(visibleGenL);

            if (bHasExitRoadConnector)
            {
                gen.SetEntryConnector(lastExitRoadConnector);
            }

            yield return gen.GenerateAndWait(true);

            playableGenerators.Add(gen);

            if (playableGenerators.Count >= 4)
            {
                playableGenerators[0].Purge();
                playableGenerators.RemoveAt(0);

                if (visibleGenerators.Count == 11)
                {
                    // first purge, need to remove additional starting generators
                    for (int j = 0; j < 5; j++)
                    {
                        visibleGenerators[j].Purge();
                    }
                    visibleGenerators.RemoveRange(0, 5);
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        visibleGenerators[j].Purge();
                    }
                    visibleGenerators.RemoveRange(0, 2);
                }
            }

            

            nextOffset += generatorOffset;
        }
    }

    public IEnumerator GenerateNextPathCoroutine(DifficultyState state)
    {
        yield return new WaitForSeconds(4);

        TerrainGenerator gen = playableGenerators[playableGenerators.Count - 1];

        gen.SetDifficultySettings(state);
        yield return gen.GeneratePathCoroutine();

        RoadConnector connector = gen.GetExitConnector();

        bHasExitRoadConnector = true;
        lastExitRoadConnector = connector;
    }

    public void GenerateNextTerrainSegment()
    {
        StartCoroutine(GenerateNextSegmentCoroutine());
    }

    public void GenerateNextPath(DifficultyState state)
    {
        StartCoroutine(GenerateNextPathCoroutine(state));
    }

    public Vector3 GetLastRespawnPoint()
    {
        return playableGenerators[playableGenerators.Count - 1].GetRespawnPoint();
    }

    public Vector3 GetCurrentRespawnPoint(out int generatorIndex)
    {
        generatorIndex = 1;
        if (playableGenerators.Count > 1)
        {
            if (playableGenerators[playableGenerators.Count - 1].IsPathGenerated())
            {
                return playableGenerators[playableGenerators.Count - 1].GetRespawnPoint();
            }
            generatorIndex = 2;
            return playableGenerators[playableGenerators.Count - 2].GetRespawnPoint();
        }
        return playableGenerators[playableGenerators.Count - 1].GetRespawnPoint();
    }

    public void ForceColliderUpdate(int generatorIndex)
    {
        playableGenerators[playableGenerators.Count - generatorIndex].ForceColliderUpdate();
    }


}
