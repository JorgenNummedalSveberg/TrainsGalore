using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        // texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightmMap)
    {
        int width = heightmMap.GetLength(0);
        int height = heightmMap.GetLength(1);

        Color[] colourMap = new Color[width * height];
        for (var i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                colourMap[i*width+j] = Color.Lerp(Color.black, Color.white, heightmMap[i, j]);
            }
        }
        
        return TextureFromColourMap(colourMap, width, height);
    }
}
