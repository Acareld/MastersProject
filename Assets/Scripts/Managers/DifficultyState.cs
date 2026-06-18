using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct PotholeDifficultySettings
{
    public int potholeCount;
    public float minPotholeSpacing;
    public Vector2 potholeRadiusRange;
    public Vector2 potholeDepthRange;
}

[System.Serializable]
public struct TrenchDifficultySettings
{
    public int trenchCount;
    public float minTrenchSpacing;
    public int trenchRadius;
    public Vector2 trenchDepthRange;
    public float maxRampSpawnSlope;
}

[System.Serializable]
public enum Difficulty
{
    VERYEASY,
    EASY,
    MEDIUM,
    HARD,
    VERYHARD
}

[CreateAssetMenu(fileName = "DifficultyState", menuName = "Scriptable Objects/DifficultyState")]
public class DifficultyState : ScriptableObject
{
    [SerializeField] public Difficulty difficulty;
    [SerializeField] public List<DifficultyPhase> slopePhases;
    [SerializeField] public PotholeDifficultySettings potholeSettings;
    [SerializeField] public TrenchDifficultySettings trenchSettings;
}
