using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DefaultNamespace
{
    public class TreeGenerator
    {
        public static TreeData GenerateTreeData(MeshData meshData, Vector2 chunkCoord, Vector2 offset, TreeNoise treeNoise, int seed, GameObject treePrefab)
        {
            System.Random prng = new System.Random(seed);
            TreeData treeData = new TreeData(chunkCoord, treePrefab);
            int mapSize = Mathf.RoundToInt(Mathf.Sqrt(meshData.TrueHeights.Length));
            float[,] noiseMap = Noise.GenerateTreeNoise(mapSize, mapSize, seed, chunkCoord + offset, treeNoise.noiseScale);

            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    int i = y + x * mapSize;
                    if (meshData.TrueHeights[i] < treeNoise.treeRange.x || meshData.TrueHeights[i] > treeNoise.treeRange.y) continue;
                    float densityChance = prng.Next(0, 1000) / 1000f;
                    if (densityChance > treeNoise.treeDensity) continue;
                    float spawnChance = noiseMap[x, y];
                    float spawnNumber = prng.Next(0, 1000) / 1000f;
                    if (spawnNumber > spawnChance) continue;
                    treeData._treePositions.Add(meshData.Vertices[i]*10);
                }
            }
            // for (int i = 0; i < meshData.Vertices.Length; i++)
            // {
            //     if (meshData.TrueHeights[i] < treeNoise.treeRange.x || meshData.TrueHeights[i] > treeNoise.treeRange.y) continue;
            //     if (prng.Next(0, 1000) > treeNoise.treeDensity*1000) continue;
            //     treeData._treePositions.Add(meshData.Vertices[i]*10);
            // }
            
            return treeData;
        }

    }
}


public struct TreeData
{
    public List<Vector3> _treePositions;
    private readonly Vector2 _chunkCoord;
    private readonly GameObject _treePrefab;

    public List<Vector3> _treesToMake;

    public TreeData(Vector2 chunkCoord, GameObject treePrefab)
    {
        _chunkCoord = chunkCoord;
        _treePrefab = treePrefab;
        _treePositions = new List<Vector3>();
        _treesToMake = new List<Vector3>();
    }

    #if UNITY_EDITOR
    public void EditorInstantiateTrees(GameObject treeCollection)
    {
        if (_treesToMake.Count > 0 && treeCollection != null)
        {
            int treeCount = Math.Min(_treesToMake.Count, 100);
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 thing = _treesToMake[0];
                GameObject tree = Object.Instantiate(_treePrefab);
                tree.SetActive(true);
                Transform treeTransform = tree.transform;
                treeTransform.position = thing;
                treeTransform.SetParent(treeCollection.transform);
                _treesToMake.Remove(thing);
            }
            if (_treesToMake.Count > 0)
            {
                TreeData tmpThis = this;
                UnityEditor.EditorApplication.delayCall+=()=>
                {
                    tmpThis.EditorInstantiateTrees(treeCollection);
                };
            }
        }
    }
    #endif

    public (GameObject, TreeData) GenerateTrees(bool showOnStart = false)
    {
        GameObject treeCollection = new GameObject("TreeCollection");
        treeCollection.SetActive(false);
        
        if (Application.isPlaying)
        {
            _treesToMake.AddRange(_treePositions);
            foreach (Vector3 vector3 in _treesToMake)
            {
                GameObject tree = Object.Instantiate(_treePrefab);
                tree.SetActive(true);
                Transform treeTransform = tree.transform;
                treeTransform.position = vector3;
                treeTransform.SetParent(treeCollection.transform);
            }
        }
        else
        {
            #if UNITY_EDITOR
            _treesToMake.AddRange(_treePositions);
            EditorInstantiateTrees(treeCollection);
            #endif
        }
        
        treeCollection.transform.position = new Vector3(_chunkCoord.x * 10, 0, _chunkCoord.y * 10);

        treeCollection.SetActive(showOnStart);
        return (treeCollection, this);
    }
}