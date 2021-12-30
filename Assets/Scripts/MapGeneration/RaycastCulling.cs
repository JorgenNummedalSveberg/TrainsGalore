using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class RaycastCulling : MonoBehaviour
{
    public Camera cam;
    private List<GameObject> _hitLastFrame = new List<GameObject>();
    public int cullingDivision = 9;
    
    private NativeArray<RaycastCommand> _raycastCommands;
    private NativeArray<RaycastHit> _raycastHits;
    private JobHandle _jobHandle;
    private bool firstRun = true;
    

    private void Awake()
    {
        int rayCount = 0;
        for (int x = 0; x < Screen.width/ (cullingDivision*cam.aspect); x++)
        {
            for (int y = 0; y < Screen.height/cullingDivision; y++)
            {
                rayCount++;
            }
        }

        _raycastCommands = new NativeArray<RaycastCommand>(rayCount, Allocator.Persistent);
        _raycastHits = new NativeArray<RaycastHit>(rayCount, Allocator.Persistent);
    }

    void Update()
    {
        if (firstRun || _jobHandle.IsCompleted)
        {
            firstRun = false;
            
            List<GameObject> hitThisFrame = new List<GameObject>();
            for (int i = 0; i < _raycastHits.Length; i++)
            {
                var hitInfo = _raycastHits[i];
                if (hitInfo.transform != null && hitInfo.transform.GetComponent<MeshRenderer>() != null)
                {
                    hitThisFrame.Add(hitInfo.transform.gameObject);
                    hitInfo.transform.GetComponent<MeshRenderer>().enabled = true;
                    Debug.DrawLine(transform.position, hitInfo.point, Color.red);
                }
            }
                    
            int index = 0;
            for (int x = 0; x < Screen.width/ (cullingDivision*cam.aspect); x++)
            {
                for (int y = 0; y < Screen.height/cullingDivision; y++)
                {
                    Ray ray = cam.ScreenPointToRay(new Vector3(x*cullingDivision*cam.aspect, y*cullingDivision, 0));
                    _raycastCommands[index] = new RaycastCommand(ray.origin, ray.direction);
                    index++;
                }
            }
            _jobHandle = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastHits, 1);
                    
            for (int i = 0; i < _hitLastFrame.Count; i++)
            {
                if (!hitThisFrame.Contains(_hitLastFrame[i]) && _hitLastFrame[i].GetComponent<MeshRenderer>() != null)
                {
                    _hitLastFrame[i].GetComponent<MeshRenderer>().enabled = false;
                }
            }
        
            _hitLastFrame = hitThisFrame;
        }

        
    }

    private void OnValidate()
    {
        if (cullingDivision < 1)
        {
            cullingDivision = 1;
        }
    }
    
    struct ThreadInfo<T>
    {
        public readonly Action<T> Callback;
        public readonly T Parameter;

        public ThreadInfo(Action<T> callback, T parameter)
        {
            Callback = callback;
            Parameter = parameter;
        }
    }
}
