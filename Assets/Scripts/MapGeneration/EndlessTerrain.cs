using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public static EndlessTerrain Current;
    
    private const float ViewerMoveThreshold = 25f;
    private const float SqrViewerMoveThreshold = ViewerMoveThreshold * ViewerMoveThreshold;
    
    public LODInfo[] detailLevels;
    public static float MaxViewDst;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 ViewerPosition;
    private Vector2 _viewerPositionOld;
    private static MapGenerator _mapGenerator;
    private int _chunkSize;
    public int visibleChunks;
    public int chunkCount;

    private Dictionary<Vector2, TerrainChunk> _terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    static private List<TerrainChunk> _loadedChunks = new List<TerrainChunk>();

    public int chunkAddProgress;
    public int meshGenProgress;
    public int treeGenProgress;
    public bool isDone;
    
    private void Awake()
    {
        Current = this;
    }
    
    void Start()
    {
        _mapGenerator = FindObjectOfType<MapGenerator>();

        MaxViewDst = detailLevels[detailLevels.Length - 1].threshold;
        _chunkSize = MapGenerator.MapChunkSize -1;
        visibleChunks = Mathf.RoundToInt(MaxViewDst / _chunkSize);
        
        UpdateVisibleChunks();
    }

    private void Update()
    {
        ViewerPosition = new Vector2(viewer.position.x, viewer.position.z)/_mapGenerator.TerrainData.uniformScale;
        if ((_viewerPositionOld - ViewerPosition).sqrMagnitude > SqrViewerMoveThreshold)
        {
            _viewerPositionOld = ViewerPosition;
            UpdateVisibleChunks();
        }

        chunkCount = Mathf.RoundToInt(Mathf.Pow(visibleChunks * 2 + 1, 2));
        
        if (chunkAddProgress == chunkCount && meshGenProgress == chunkCount && treeGenProgress == chunkCount)
        {
            isDone = true;
        }
    }

    public void UpdateVisibleChunks()
    {
        chunkAddProgress = 0;
        for (int i = 0; i < _loadedChunks.Count; i++)
        {
            _loadedChunks[i].SetVisible(false);
        }
        _loadedChunks.Clear();
        int currentChunkCoordX = Mathf.RoundToInt(ViewerPosition.x / _chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(ViewerPosition.y / _chunkSize);

        int count = 0;
        for (int yOffset = -visibleChunks; yOffset <= visibleChunks; yOffset++)
        {
            for (int xOffset = -visibleChunks; xOffset <= visibleChunks; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (_terrainChunks.ContainsKey (viewedChunkCoord)) {
                    _terrainChunks [viewedChunkCoord].Update();
                } else {
                    _terrainChunks.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, _chunkSize, detailLevels, transform, mapMaterial));
                }

                count++;
                chunkAddProgress = count;

            }
        }
    }

    public class TerrainChunk
    {
        private GameObject _meshObject;
        private Vector2 _position;
        private Bounds _bounds;
        
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        public GameObject trees;
        
        private LODInfo[] _detailLevels;
        private LODMesh[] _lodMeshes;

        private MapData _mapData;
        private bool _mapDataRecieved;
        private int _previousLODIndex = -1;
        private int index;

        private bool meshDone;
        private bool treeDone;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            _detailLevels = detailLevels;
            _position = coord * size;
            _bounds = new Bounds(_position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(_position.x, 0, _position.y);

            _meshObject = new GameObject("Terrain Chunk");
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
            _meshFilter = _meshObject.AddComponent<MeshFilter>();
            _meshCollider = _meshObject.AddComponent<MeshCollider>();
            
            _meshRenderer.material = material;
            _meshObject.transform.position = positionV3 * _mapGenerator.TerrainData.uniformScale;
            _meshObject.transform.parent = parent;
            _meshObject.transform.localScale = Vector3.one * _mapGenerator.TerrainData.uniformScale;
            SetVisible(false);
            
            _lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                _lodMeshes[i] = new LODMesh(detailLevels[i].lod, Update);
            }
            
            _mapGenerator.RequestMapData(_position, OnMapDataRecieved);
        }

        private void OnMapDataRecieved(MapData mapData)
        {
            _mapData = mapData;
            _mapDataRecieved = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.ColourMap, MapGenerator.MapChunkSize,
                MapGenerator.MapChunkSize);
            _meshRenderer.material.mainTexture = texture;
            Update();
        }

        public void Update()
        {
            if (_mapDataRecieved)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(ViewerPosition));
                bool visible = viewerDstFromNearestEdge <= MaxViewDst;
    
                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < _detailLevels.Length -1; i++)
                    {
                        if (viewerDstFromNearestEdge > _detailLevels[i].threshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    index = lodIndex;
    
                    if (lodIndex != _previousLODIndex)
                    {
                        LODMesh lodMesh = _lodMeshes[lodIndex];
                        if (lodMesh.hasMesh && lodMesh.hasTrees)
                        {
                            if (!treeDone)
                            {
                                Current.treeGenProgress += 1;
                                treeDone = true;
                            }
                            _previousLODIndex = lodIndex;
                            _meshFilter.mesh = lodMesh.mesh;
                            _meshCollider.sharedMesh = lodMesh.mesh;

                            trees = lodMesh.trees;
                        } else if (lodMesh.hasMesh)
                        {
                            if (!meshDone)
                            {
                                Current.meshGenProgress += 1;
                                meshDone = true;
                            }
                            
                            if (!lodMesh.hasRequestedTrees)
                            {
                                lodMesh.RequestTrees(lodMesh.MeshData, _position);
                            }
                        } else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(_mapData);
                        }
                    }
                    _loadedChunks.Add (this);
                }
                
                SetVisible(visible);
            }
            
        }

        public void SetVisible(bool visible)
        {
            _meshObject.SetActive(visible);
            
            // Don't load trees if the chunk is not visible, or if LOD is over 0
            bool treeActive = !(!visible || index > 0);
            if (trees != null)
            {
                trees.SetActive(treeActive);
            }
            
        }

        public bool IsVisible()
        {
            return _meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public MeshData MeshData;
        public GameObject trees;
        public bool hasRequestedMesh;
        public bool hasRequestedTrees;
        public bool hasMesh;
        public bool hasTrees;
        public int treeCount;
        public List<Vector3> treesLeft;
        private int _lod;
        private Action updateCallback;

        public LODMesh(int lod, Action updateCallback)
        {
            _lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            MeshData = meshData;
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            _mapGenerator.RequestMeshData(mapData, _lod, OnMeshDataRecieved);
        }

        void OnTreeDataRecieved(TreeData treeData)
        {
            var result = treeData.GenerateTrees();
            trees = result.Item1;
            hasTrees = true;
            updateCallback();
        }

        public void RequestTrees(MeshData meshData, Vector2 chunkCoord)
        {
            hasRequestedTrees = true;
            _mapGenerator.RequestTreeData(meshData, chunkCoord, OnTreeDataRecieved );
        }
    }
    [Serializable]
    public struct LODInfo
    {
        public int lod;
        public float threshold;
        
    }
}
