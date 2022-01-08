using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode
    {
        Local,
        Global
    };
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale,
        int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode,
        RailRoadData railRoadData)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }
        
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x-halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y- halfHeight + octaveOffsets[i].y) / scale * frequency;
    
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY)*2-1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        // //Generating railroad
        // List<float> roadHeights = new List<float>();
        // float[,] heightMapOffsets = new float[mapWidth, mapHeight];
        //
        // if (!railRoadData.Turn)
        // {
        //     for (int y = 0; y < mapHeight; y++)
        //     {
        //         roadHeights.Add(noiseMap[y, railRoadData.RailStart]);
        //     }
        // }
        // else
        // {
        //     for (int y = 0; y < mapHeight - railRoadData.TurnDistance + railRoadData.Width/2; y++)
        //     {
        //         roadHeights.Add(noiseMap[y, railRoadData.RailStart]);
        //     }
        //     
        //     for (int x = 0; x < railRoadData.RailStart  + railRoadData.Width/2; x++)
        //     {
        //         roadHeights.Add(noiseMap[railRoadData.RailStart, x]);
        //     }
        // }
        //
        // float average = roadHeights.Average();
        // for (int y = 0; y < roadHeights.Count  && (!railRoadData.Turn || y < mapHeight - railRoadData.TurnDistance  + railRoadData.Width/2); y++)
        // {
        //     float averageDistance = roadHeights[y] - average;
        //     for (int x = 0; x < railRoadData.Width; x++)
        //     {
        //         float newHeight;
        //         int flatOffset = Mathf.CeilToInt((railRoadData.Width - railRoadData.FlatSize) / 2f);
        //         float middleHeight = roadHeights[y] - averageDistance * railRoadData.Normalization;
        //         float middleDistance = noiseMap[y, x + (railRoadData.RailStart) - railRoadData.Width/2] - middleHeight;
        //         if (x < flatOffset)
        //         {
        //             newHeight = noiseMap[y, x + (railRoadData.RailStart) - railRoadData.Width/2] - middleDistance*x/flatOffset;
        //         }
        //         else if (x < flatOffset + railRoadData.FlatSize)
        //         {
        //             newHeight = middleHeight;
        //         }
        //         else
        //         {
        //             newHeight = noiseMap[y, x + (railRoadData.RailStart) - railRoadData.Width/2] - (middleDistance - middleDistance*(x-flatOffset-railRoadData.FlatSize)/flatOffset);
        //         }
        //         heightMapOffsets[y, x + (railRoadData.RailStart) - railRoadData.Width/2] = newHeight - noiseMap[y, x + (railRoadData.RailStart) - railRoadData.Width/2];
        //     }
        // }
        //
        // roadHeights.Clear();
        // if (railRoadData.Turn)
        // {
        //     for (int x = 0; x < railRoadData.RailStart  + railRoadData.Width/2; x++)
        //     {
        //         roadHeights.Add(noiseMap[mapWidth - railRoadData.TurnDistance, x]);
        //     }
        //     
        //     for (int x = 0; x < railRoadData.RailStart  + railRoadData.Width/2; x++)
        //     {
        //         
        //         float averageDistance = roadHeights[x] - average;
        //         int yRailStart = mapHeight - railRoadData.TurnDistance;
        //         for (int y = 0; y < railRoadData.Width; y++)
        //         {
        //             float newHeight;
        //             int flatOffset = Mathf.CeilToInt((railRoadData.Width - railRoadData.FlatSize) / 2f);
        //             float middleHeight = roadHeights[x] - averageDistance * railRoadData.Normalization;
        //             float middleDistance = noiseMap[y + yRailStart - railRoadData.Width/2, x] - middleHeight;
        //             if (y < flatOffset)
        //             {
        //                 newHeight = noiseMap[y + yRailStart - railRoadData.Width/2, x] - middleDistance*y/flatOffset;
        //             }
        //             else if (y < flatOffset + railRoadData.FlatSize)
        //             {
        //                 newHeight = middleHeight;
        //             }
        //             else
        //             {
        //                 newHeight = noiseMap[y + yRailStart - railRoadData.Width/2, x] - (middleDistance - middleDistance*(y-flatOffset-railRoadData.FlatSize)/flatOffset);
        //             }
        //             
        //             bool alreadyWritten = heightMapOffsets[y + yRailStart - railRoadData.Width / 2, x] != 0;
        //             heightMapOffsets[y + yRailStart - railRoadData.Width/2, x] += newHeight - noiseMap[y + yRailStart - railRoadData.Width/2, x];
        //             if (alreadyWritten)
        //             {
        //                 heightMapOffsets[y + yRailStart - railRoadData.Width / 2, x] /= 2;
        //             }
        //         }
        //     }
        // }
        //
        // for (int x = 0; x < mapHeight; x++)
        // {
        //     for (int y = 0; y < mapWidth; y++)
        //     {
        //         noiseMap[x, y] += heightMapOffsets[x, y];
        //     }
        // }
        
        return noiseMap;
    }

    public static float[,] GenerateTreeNoise(int mapWidth, int mapHeight, int seed, Vector2 offset, float scale)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);
        
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        
        
        float offsetX = prng.Next(-100000, 100000) + offset.x;
        float offsetY = prng.Next(-100000, 100000) - offset.y;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float noiseHeight = 0;
                float sampleX = (x-halfWidth + offsetX) / scale;
                float sampleY = (y- halfHeight + offsetY) / scale;
    
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY)*2-1;
                noiseHeight += perlinValue;
                
                noiseMap[x, y] = noiseHeight;
            }
        }
        
        return noiseMap;
    }
}
