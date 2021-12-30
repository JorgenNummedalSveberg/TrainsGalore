using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NoiseData : UpdatableData
{
    public Noise.NormalizeMode NormalizeMode;

    public float noiseScale;
    public int octaves;
    [Range(0,1)] public float persistance;
    public float lacunarity;
    public int mapSeed;
    public Vector2 offset;

    protected override void OnValidate()
    {
        if (octaves < 0)
        {
            octaves = 0;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        base.OnValidate();
    }
}
