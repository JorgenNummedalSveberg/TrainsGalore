using UnityEngine;

[CreateAssetMenu]

public class RailRoadData : UpdatableData
{
    public int Width;
    public float Height;
    [Range(0, 1f)] public float Normalization;
    [Range(0, 1f)] public float Flatness;
    public int FlatSize;
    [Range(0, 240)] public int RailStart;
    public bool Turn;
    [Range(0, 240)]public int TurnDistance;

    protected override void OnValidate()
    {
        base.OnValidate();

        if (Width < 0)
        {
            Width = 0;
        }
        if (FlatSize < 0)
        {
            FlatSize = 0;
        } else if (FlatSize > Width)
        {
            FlatSize = Width;
        }

        if (RailStart < Width/2)
        {
            RailStart = Width/2;
        } else if (RailStart > 240 - Width / 2)
        {
            RailStart = 240 - Width / 2;
        }
    }
}