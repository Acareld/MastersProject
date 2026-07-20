using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class NoiseMapGenerator : MonoBehaviour
{

    public int octaves = 5;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public int seed;
    
    public float[] ScheduleNoiseJob(float scaleLarge, Vector2 offset, Vector2[] uvs)
    {
        NativeArray<float2> nativeUvs = new NativeArray<float2>(uvs.Length, Allocator.TempJob);
        NativeArray<float> nativeNoiseMap = new NativeArray<float>(uvs.Length, Allocator.TempJob);

        for (int i = 0; i < uvs.Length; i++)
        {
            nativeUvs[i] = new float2(uvs[i].x, uvs[i].y);
        }

        NoiseMapJob job = new NoiseMapJob
        {
            uvs = nativeUvs,
            noiseMap = nativeNoiseMap,

            scale = scaleLarge,
            offset = new float2(offset.x, offset.y),

            octaves = octaves,
            persistence = persistence,
            lacunarity = lacunarity
        };

        JobHandle handle = job.ScheduleParallelByRef(uvs.Length, 64, default);

        handle.Complete();

        float[] result = nativeNoiseMap.ToArray();

        nativeUvs.Dispose();
        nativeNoiseMap.Dispose();

        return result;
    }


    public float[] GenerateNoiseMap(float scaleLarge, float scaleSmall, Vector2 offset, Vector2[] uvs)
    {
        float[] noiseMap = new float[uvs.Length];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int i = 0; i < uvs.Length; i++)
        {
            float x = uvs[i].x + offset.x;
            float y = uvs[i].y + offset.y;

            float height = PerlinOctaves(x,y,scaleLarge, octaves, persistence, lacunarity);

            if(height > maxNoiseHeight)
            {
               maxNoiseHeight = height;
            }
            else if(height < minNoiseHeight)
            {
                minNoiseHeight = height;
            }
            noiseMap[i] = height;
        }

        for(int i = 0; i < uvs.Length; i++)
        {
            noiseMap[i] = Mathf.InverseLerp(-1, 1, noiseMap[i]);
        }
        
        return noiseMap;
    }

    private float PerlinOctaves(float x, float y, float baseScale, int octaves, float persistence, float lacunarity)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxAmplitude = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / baseScale * frequency;
            float sampleY = y / baseScale * frequency;

            float noise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

            total += noise * amplitude;
            maxAmplitude += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total;
    }

}
