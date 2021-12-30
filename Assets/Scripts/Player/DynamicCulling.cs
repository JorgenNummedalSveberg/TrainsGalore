using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class DynamicCulling : MonoBehaviour
{
    private Camera _cam;
    private List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();

    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
        _meshRenderers.AddRange(GetRenderers(transform));
        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            meshRenderer.gameObject.AddComponent<Culling>();
        }
    }

    List<MeshRenderer> GetRenderers(Transform item)
    {
        List<MeshRenderer> renderers = new List<MeshRenderer>();
        if (transform.GetComponent<MeshRenderer>() != null)
        {
            renderers.Add(transform.GetComponent<MeshRenderer>());
        }
        foreach (Transform t in item.transform)
        {
            if (t.GetComponent<MeshRenderer>() != null)
            {
                renderers.Add(t.GetComponent<MeshRenderer>());

            }
            renderers.AddRange(GetRenderers(t));
        }

        return renderers;
    }

    // Update is called once per frame
    void Update()
    {
        // foreach (MeshRenderer meshRenderer in _meshRenderers)
        // {
        //     bool hit = Physics.Raycast(_cam.transform.position, meshRenderer.transform.position, out var hitInfo);
        //     meshRenderer.enabled = hit && hitInfo.transform == meshRenderer.transform;
        //     if (hit && hitInfo.transform == meshRenderer.transform)
        //     {
        //         Debug.DrawLine(_cam.transform.position, meshRenderer.transform.position, Color.red);
        //     }
        // }
        
        // if (hit)
        // {
        //     if (hitInfo.collider.transform.GetComponent<MeshRenderer>() != null)
        //     {
        //         Debug.DrawLine(_cam.transform.position, hitInfo.point, Color.red);
        //         hit = _meshRenderers.Contains(hitInfo.collider.transform.GetComponent<MeshRenderer>());
        //     }
        // }
        // foreach (MeshRenderer meshRenderer in _meshRenderers)
        // {
        //     meshRenderer.enabled = hit;
        // }

    }
}
