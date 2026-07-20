using UnityEngine;
using Vehicle;

public class DataCollector : MonoBehaviour
{
    private DamageData currentDamageData;
    private int resetCount;
    private System.Diagnostics.Stopwatch stopWatch;
    private int currentSegmentIndex;

    private float resetWeight = 0.15f;
    private float damageWeight = 0.2f;
    private float timeWeight = 0.2f;

    [SerializeField]
    private AnimationCurve resetScoreCurve = new AnimationCurve(
        new Keyframe(0f, 1.00f),
        new Keyframe(1f, 0.75f),
        new Keyframe(2f, 0.50f),
        new Keyframe(3f, 0.25f),
        new Keyframe(4f, 0.00f)
    );

    void Start()
    {
        stopWatch = new System.Diagnostics.Stopwatch();
    }
    public void SetData(DamageData damageData, int nResets)
    {
        currentDamageData = damageData;
        resetCount = nResets;
    }

    public void StartTimeEvaluation()
    {
        stopWatch.Start();
    }

    public Difficulty Evaluate(Difficulty previousDiff, int segmentIndex)
    {
        if(segmentIndex == -1)
        {
            // medium difficulty first segment
            return Difficulty.VERYHARD;
        }
        currentSegmentIndex = segmentIndex;

        double seconds = stopWatch.Elapsed.TotalSeconds;
        stopWatch.Stop();
        stopWatch.Reset();

        Debug.Log("Time spent in segment: " + seconds);

        return Difficulty.VERYHARD;
    }

    private void WriteCSV()
    {

    }

    private float EvaluateResets(Difficulty previousDifficulty)
    {
        return Mathf.Clamp01(resetScoreCurve.Evaluate(resetCount)) * resetWeight;   
    }

    private float EvaluateTime(Difficulty previousDifficulty)
    {
        // clean veryeasy run : 203s
        // clean veryhard run : 140s
        return 0f;
    }

    private float EvaluateDamage(Difficulty previousDifficulty)
    {
        return 0;
    }
    

}
