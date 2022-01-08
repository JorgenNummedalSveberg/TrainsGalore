using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControls : MonoBehaviour
{
    public float movementSpeed = 10;
    public float creativeMultiplier = 20;
    public float runningMultiplier = 2;
    public float gravity = -19.62f;
    public float jumpHeight = 2;

    public bool creative;
    
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode creativeKey = KeyCode.V;
    
    private Vector3 _velocity = Vector3.zero;

    private bool _onGround;
    private bool _hittingHead;
    private float groundDistance = 0.4f;
    private float roofDistance = 0.4f;
    public Transform groundCheck;
    public Transform roofCheck;
    public LayerMask checkMask;

    public float turnSpeed;
    private Rigidbody _rb;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Checks();
        if (creative)
        {
            CreativeControls();
        }
        else
        {
            PlayerControls();
        }
    }

    private void Checks()
    {
        CheckGround();
        CheckRoof();

        if (Input.GetKeyDown(creativeKey))
        {
            creative = !creative;
        }
    }

    private void CheckGround()
    {
        Collider[] floorColliders = Physics.OverlapSphere(groundCheck.position, groundDistance, checkMask);
        List<Collider> floorNonTriggers = new List<Collider>();
        foreach (Collider overlapCollider in floorColliders)
        {
            if (!overlapCollider.isTrigger)
            {
                floorNonTriggers.Add(overlapCollider);
            }
        }
        _onGround = floorNonTriggers.Count > 0;
    }

    private void CheckRoof()
    {
        Collider[] roofTriggers = Physics.OverlapSphere(roofCheck.position, roofDistance, checkMask);
        List<Collider> roofNonTriggers = new List<Collider>();
        foreach (Collider overlapCollider in roofTriggers)
        {
            if (!overlapCollider.isTrigger)
            {
                roofNonTriggers.Add(overlapCollider);
            }
        }
        _hittingHead = roofNonTriggers.Count > 0;
    }

    private void PlayerControls()
    {
        if (_onGround && _velocity.y < 0)
        {
            // _velocity.y = -2f;
            _velocity.y = -0f;
        } else if (_hittingHead && _velocity.y > 0)
        {
            _velocity.y = 0;
        }

        if (Input.GetButton("Jump") && _onGround)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        _velocity.y += gravity * Time.deltaTime;
        
        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");
        // Vector3 movement = new Vector3(1, 0, 0) * hor + new Vector3(0, 0, 1) * ver;
        Vector3 movement = transform.right * hor + transform.forward * ver;
        movement *= movementSpeed;
        if (Input.GetKey(runKey))
        {
            movement *= runningMultiplier;
        }
        _velocity.x = movement.x;
        _velocity.z = movement.z;
    }

    private void CreativeControls()
    {
        _velocity = Vector3.zero;
        if (Input.GetKey(crouchKey))
        {
            _velocity.y = -creativeMultiplier;
        }

        if (Input.GetButton("Jump"))
        {
            _velocity.y = creativeMultiplier;
        }

        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");

        var transform1 = transform;
        _velocity += transform1.right * hor * creativeMultiplier + transform1.forward * ver * creativeMultiplier;
        if (Input.GetKey(runKey))
        {
            _velocity *= 2;
        }
    }

    private void FixedUpdate()
    {
        transform.Translate(_velocity * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        // if (transform.parent == null)
        // {
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 0), turnSpeed*Time.deltaTime);
        // }
    }
}
