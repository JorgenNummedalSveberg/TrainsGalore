using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    private GameObject _treeCollection;
    private GameObject _railCollection;
    public bool drawTreesInEditor;
    public bool drawRailsInEditor;
    
    public GameObject rail;
    public int maxRailLength;
    public int railScale;
    public int stepsPer90Degree;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture, TreeData treeData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;

        if (drawTreesInEditor)
        {
            if (_treeCollection != null)
            {
                GameObject tempcol = _treeCollection;
            
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(tempcol);
                };
                #endif
            }

            _treeCollection = treeData.GenerateTrees(true).Item1;
        }

        if (drawRailsInEditor)
        {
            if (_railCollection != null)
            {
                GameObject tempcol = _railCollection;
            
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    DestroyImmediate(tempcol);
                };
                #endif
            }

            _railCollection = DrawRailroad(maxRailLength);
        }
    }
    
    public Vector2 GetPointInSpiral(int t)
    {
        int x = Mathf.RoundToInt(Mathf.Cos(Mathf.Sqrt(t) * Mathf.PI / (2*stepsPer90Degree)) * Mathf.Sqrt(t));
        int y = Mathf.RoundToInt(Mathf.Sin(Mathf.Sqrt(t) * Mathf.PI / (2*stepsPer90Degree)) * Mathf.Sqrt(t));
        return new Vector2(x, y);
    }
    
    public GameObject DrawRailroad(int maxLength)
    {
        List<Vector2> railLine = new List<Vector2>();
        Vector2 newRail = Vector2.zero;
        int i = 0;
        while (railLine.Count < maxLength-1)
        {
            Vector2 lastRail = newRail;
            newRail = GetPointInSpiral(i);

            // Skips same coords, and skips L shapes in favour of diagonals
            if (Mathf.Abs(newRail.x - GetPointInSpiral(i + 2).x) <= 1 &&
                Mathf.Abs(newRail.y - GetPointInSpiral(i + 2).y) <= 1)
            {
                i++;
            } 
            
            if (railLine.Count > 0 && newRail.Equals(lastRail)) continue;

            railLine.Add(newRail);
            i++;
        }

        GameObject railList = new GameObject("railList");
        for (var index = 0; index < railLine.Count; index++)
        {
            Vector2 vector2 = railLine[index];
            GameObject tempRail = Instantiate(rail, railList.transform);
            tempRail.transform.position = new Vector3(vector2.x * railScale, 0, vector2.y * railScale);
        }

        return railList;
    }
}
