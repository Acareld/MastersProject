using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct NoiseMapJob : IJobFor
{
    [ReadOnly] public NativeArray<float2> uvs;

    public NativeArray<float> noiseMap;

    public float scale;
    public float2 offset;
    public int octaves;
    public float persistence;
    public float lacunarity;

    public void Execute(int index)
    {
        float x = uvs[index].x + offset.x;
        float y = uvs[index].y + offset.y;

        float height = PerlinOctaves(x, y);

        noiseMap[index] = height * 0.5f + 0.5f;
    }

    private float PerlinOctaves(float x, float y)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxAmplitude = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequency;
            float sampleY = y / scale * frequency;

            float noiseValue = noise.cnoise(new float2(sampleX, sampleY));

            total += noiseValue * amplitude;
            maxAmplitude += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxAmplitude;
    }
}
