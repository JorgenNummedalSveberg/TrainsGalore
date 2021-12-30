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
    public bool drawTreesInEditor;

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
    }
}
