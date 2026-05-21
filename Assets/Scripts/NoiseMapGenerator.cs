using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class NoiseMapGenerator : MonoBehaviour
{

    public float[] GenerateNoiseMap(float scaleLarge, float scaleSmall, Vector2 offset, Vector2[] uvs, AnimationCurve primaryCurve)
    {
        float[] noiseMap = new float[uvs.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            //noiseMap[i] = Unity.Mathematics.noise.cellular2x2(float2((uvs[i].x + offset.x) * scale, (uvs[i].y + offset.y) * scale)).x;
            float primary = Mathf.PerlinNoise((uvs[i].x + offset.x) / scaleLarge, (uvs[i].y + offset.y) / scaleLarge);
  
            //noiseMap[i] = Unity.Mathematics.math.saturate(noiseMap[i]);

            float detail = Mathf.PerlinNoise((uvs[i].x + offset.x) / scaleSmall, (uvs[i].y + offset.y) / scaleSmall);
            detail -= 0.5f;

            float height = primary;// + detail * 0.05f;

            //height /= 3f + 0.2f;

            noiseMap[i] = height;
    }

        
        
        return noiseMap;
    }

}
