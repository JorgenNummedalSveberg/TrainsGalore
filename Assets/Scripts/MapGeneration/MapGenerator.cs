using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using DefaultNamespace;
using Object = UnityEngine.Object;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh
    };

    public GameObject tree;
    public DrawMode drawMode;

    public TerrainData TerrainData;
    public NoiseData NoiseData;
    public TreeNoise TreeNoise;
    public RailRoadData RailRoadData;

    private bool railsNotDone;

    public GameObject railRoadPrefab;

    public bool makeTrainPath;
    
    public const int MapChunkSize = 241;
    [Range(0, 6)] public int previewLOD;
    
    public bool autoUpdate;

    private Queue<MapThreadInfo<TreeData>> _treeDataThreadInfoQueue = new Queue<MapThreadInfo<TreeData>>();
    private Queue<MapThreadInfo<MapData>> _mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> _meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();


    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }
    
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.HeightMap));
        } else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.ColourMap, MapChunkSize, MapChunkSize));
        } else if (drawMode == DrawMode.Mesh)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.HeightMap, TerrainData.meshHeightMultiplier,
                TerrainData.meshHeightCurve, previewLOD);
            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.ColourMap, MapChunkSize, MapChunkSize);
            TreeData treeData = TreeGenerator.GenerateTreeData(meshData, Vector2.zero, NoiseData.offset,
                TreeNoise, NoiseData.mapSeed, tree);
            display.DrawMesh(meshData, texture, treeData);
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate { MapDataThread(centre, callback); };
        new Thread(threadStart).Start();
    }

    private void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (_mapDataThreadInfoQueue)
        {
            _mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    private void OnValidate()
    {
        if (TreeNoise != null)
        {
            TreeNoise.OnValuesUpdated -= OnValuesUpdated;
            TreeNoise.OnValuesUpdated += OnValuesUpdated;
        }
        if (TerrainData != null)
        {
            TerrainData.OnValuesUpdated -= OnValuesUpdated;
            TerrainData.OnValuesUpdated += OnValuesUpdated;
        }

        if (NoiseData != null)
        {
            NoiseData.OnValuesUpdated -= OnValuesUpdated;
            NoiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (RailRoadData != null)
        {
            RailRoadData.OnValuesUpdated -= OnValuesUpdated;
            RailRoadData.OnValuesUpdated += OnValuesUpdated;
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callback); };
        new Thread(threadStart).Start();
    }
    private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData =
            MeshGenerator.GenerateTerrainMesh(mapData.HeightMap, TerrainData.meshHeightMultiplier, TerrainData.meshHeightCurve, lod);
        lock (_meshDataThreadInfoQueue)
        {
            _meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    public void RequestTreeData(MeshData meshData, Vector2 chunkCoord, Action<TreeData> callback)
    {
        ThreadStart threadStart = delegate { TreeDataThread(meshData, chunkCoord, callback); };
        new Thread(threadStart).Start();
    }

    private void TreeDataThread(MeshData meshData, Vector2 chunkCoord, Action<TreeData> callback)
    {
        TreeData treeData = TreeGenerator.GenerateTreeData(meshData, chunkCoord, NoiseData.offset, TreeNoise, NoiseData.mapSeed, tree);
        lock (_treeDataThreadInfoQueue)
        {
            _treeDataThreadInfoQueue.Enqueue(new MapThreadInfo<TreeData>(callback, treeData));
        }
    }

    private void Update()
    {
        
        if (_mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < _mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = _mapDataThreadInfoQueue.Dequeue();
                threadInfo.Callback(threadInfo.Parameter);
            }
        }
        if (_meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < _meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = _meshDataThreadInfoQueue.Dequeue();
                threadInfo.Callback(threadInfo.Parameter);
            }
        }

        if (_treeDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < _treeDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<TreeData> threadInfo = _treeDataThreadInfoQueue.Dequeue();
                threadInfo.Callback(threadInfo.Parameter);
            }
        }

        if (EndlessTerrain.Current.isDone && makeTrainPath && railsNotDone)
        {
            Debug.Log("here");
            railsNotDone = true;
        
            if (!RailRoadData.Turn)
            {
                for (int y = 0; y < MapChunkSize; y++)
                {
                    int x = RailRoadData.RailStart;
                    Instantiate(railRoadPrefab);
                    railRoadPrefab.transform.position = new Vector3(x, 0, y);
                }
            }
            else
            {
                for (int y = 0; y < MapChunkSize - RailRoadData.TurnDistance + RailRoadData.Width/2; y++)
                {
                    int x = RailRoadData.RailStart;
                    Instantiate(railRoadPrefab);
                    railRoadPrefab.transform.position = new Vector3(x, 0, y);
                }
            
                for (int x = 0; x < RailRoadData.RailStart  + RailRoadData.Width/2; x++)
                {
                    int y = RailRoadData.TurnDistance;
                    Instantiate(railRoadPrefab);
                    railRoadPrefab.transform.position = new Vector3(x, 0, y);                }
            }
        }
    }

    private MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize,NoiseData.mapSeed, NoiseData.noiseScale, NoiseData.octaves, NoiseData.persistance, NoiseData.lacunarity, centre + NoiseData.offset, NoiseData.NormalizeMode, RailRoadData);

        Color[] colourMap = new Color[MapChunkSize * MapChunkSize];
        for (int y = 0; y < MapChunkSize; y++) {
			for (int x = 0; x < MapChunkSize; x++) {
				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < TerrainData.regions.Length; i++) {
					if (currentHeight >= TerrainData.regions [i].height) {
						colourMap [y * MapChunkSize + x] = TerrainData.regions [i].colour;
					}
                    else
                    {
                        break;
                    }
				}
			}
		}

        return new MapData(noiseMap, colourMap);
    }


    struct MapThreadInfo<T>
    {
        public readonly Action<T> Callback;
        public readonly T Parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            Callback = callback;
            Parameter = parameter;
        }
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData
{
    public readonly float[,] HeightMap;
    public readonly Color[] ColourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        HeightMap = heightMap;
        ColourMap = colourMap;
    }
}
