using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RailChecker : MonoBehaviour
{

    public GameObject frontCheck;
    public GameObject backCheck;

    public Vector3 railVector = Vector3.zero;
    private List<Collider> _frontRails = new List<Collider>();
    private List<Collider> _backRails = new List<Collider>();

    private Collider _currentFront;
    private Collider _currentBack;
    
    MovingSpace movingSpace;
    
    // Start is called before the first frame update
    void Start()
    {
        movingSpace = transform.parent.GetComponent<MovingSpace>();
    }

    private void FrontCheck()
    {
        Collider[] colliders = new Collider[10];
        Physics.OverlapBoxNonAlloc(frontCheck.transform.position, new Vector3(0.1f, 1f, 0.1f), colliders);
        foreach (Collider col in colliders)
        {
            if (col != null && col.CompareTag("Rail") && !_frontRails.Contains(col))
            {
                _frontRails.Add(col);
                _currentFront = col;
            }
        }
        
        for (var i = 0; i < _frontRails.Count; i++)
        {
            if (!colliders.Contains(_frontRails[i]))
            {
                _frontRails.Remove(_frontRails[i]);
            }
        }

        if (_frontRails.Count == 0)
        {
            _currentFront = null;
        }
    }

    private void BackCheck()
    {
        Collider[] colliders = new Collider[10];
        Physics.OverlapBoxNonAlloc(backCheck.transform.position, new Vector3(0.1f, 1f, 0.1f), colliders);
        foreach (Collider col in colliders)
        {
            if (col != null && col.CompareTag("Rail") && !_backRails.Contains(col))
            {
                _backRails.Add(col);
                _currentBack = col;
            }
        }
        
        for (var i = 0; i < _backRails.Count; i++)
        {
            if (!colliders.Contains(_backRails[i]))
            {
                _backRails.Remove(_backRails[i]);
            }
        }

        if (_backRails.Count == 0)
        {
            _currentBack = null;
        }
    }

    private void TurnTrain()
    {
        if (_currentFront != null)
        {
            Vector3 railToFront = frontCheck.transform.position - _currentFront.transform.position;
            railVector = Vector3.Project(railToFront, _currentFront.transform.right) + _currentFront.transform.position;
            transform.parent.rotation = Quaternion.LookRotation(railVector-backCheck.transform.position) * Quaternion.Euler(0, -90f, 0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(railVector, 1f);
    }

    public static Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
    {
        lineDir.Normalize();//this needs to be a unit vector
        var v = pnt - linePnt;
        var d = Vector3.Dot(v, lineDir);
        return linePnt + lineDir * d;
    }

    // Update is called once per frame
    void Update()
    {
        FrontCheck();
        BackCheck();

        TurnTrain();
        if (_frontRails.Count == 0)
        {
            if (movingSpace.passiveSpeed != 0)
            {
                if (movingSpace.passiveSpeed >= 0.1)
                {
                    movingSpace.passiveSpeed -= Mathf.Max(0.1f, movingSpace.acceleration * Time.deltaTime);
                } else if (movingSpace.passiveSpeed <= -0.1)
                {
                    movingSpace.passiveSpeed += Mathf.Max(0.1f, movingSpace.acceleration * Time.deltaTime);
                }
                else
                {
                    movingSpace.passiveSpeed = 0;
                }
            }
        }
        else
        {
            if (movingSpace.passiveSpeed < movingSpace.speed)
            {
                movingSpace.passiveSpeed += movingSpace.acceleration * Time.deltaTime;
            } else if (movingSpace.passiveSpeed > movingSpace.speed)
            {
                movingSpace.passiveSpeed = movingSpace.speed;
            }
        }
    }
}
