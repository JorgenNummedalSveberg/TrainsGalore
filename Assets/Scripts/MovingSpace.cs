using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class MovingSpace : MonoBehaviour
{
    public float speed;

    public float passiveSpeed;

    public float acceleration;

    private float _hor;
    private float _ver;
    private float _up;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<BoxCollider>();
    }

    public void Move(Vector3 movement)
    {
        transform.Translate(movement, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 8)
        {
            FindLastTransformInsideTrain(other.transform).parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != 8)
        {
            FindLastTransformInsideTrain(other.transform).parent = null;
        }
    }

    private void Update()
    {
        _hor = (Input.GetKey(KeyCode.U) ? 1 : 0) +
              (Input.GetKey(KeyCode.J) ? -1 : 0);
        _ver = (Input.GetKey(KeyCode.H) ? 1 : 0) +
              (Input.GetKey(KeyCode.K) ? -1 : 0);
        _up = (Input.GetKey(KeyCode.O) ? 1 : 0) +
             (Input.GetKey(KeyCode.L) ? -1 : 0);
    }
    
    private Transform FindLastTransformInsideTrain(Transform other)
    {
        Transform finalTransform = other;
        if (finalTransform.parent != null && finalTransform.parent != transform)
        {
            finalTransform = FindLastTransformInsideTrain(finalTransform.parent);
        }

        return finalTransform;
    }


    void FixedUpdate()
    {
        var transform1 = transform;
        var right = transform1.right;
        Vector3 movement = right * _hor + transform1.forward * _ver + transform1.up * _up;
        movement *= speed * Time.deltaTime;
        // Vector3 movement = new Vector3(hor, up, ver) * speed * Time.deltaTime;
        Move(movement + right * (passiveSpeed * Time.deltaTime));
    }

}
