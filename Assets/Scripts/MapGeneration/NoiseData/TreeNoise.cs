using UnityEngine;

[CreateAssetMenu]
public class TreeNoise : UpdatableData
{
    public Vector2 treeRange;
    [Range(0f, 1f)] public float treeDensity;
    public float noiseScale;
    public int mapWidth;
    public int mapHeight;
    public int seed;
}
