using System;
using UnityEngine;

namespace Player
{
    public class Culling : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private Mesh _mesh;
        
        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _mesh = _meshFilter.mesh;
        }

        private void OnBecameVisible()
        {
            _meshFilter.mesh = _mesh;
        }

        private void OnBecameInvisible()
        {
            _meshFilter.mesh = null;
        }
    }
}