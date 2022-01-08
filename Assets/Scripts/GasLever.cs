using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GasLever : Interactable
{
    private MovingSpace train;
    public GameObject leverHand;
    
    [Range(0, 360)] public float rotation;

    public float maxSpeed;
    public int gears;
    private int speedIndex;
    private bool accelerating = true;
    
    public float turnSpeed = 10;
    private Vector3 rotationVector;

    public GameObject leverBase;
    public Text _speedGague;

    private Camera _cam;

    
    // Start is called before the first frame update
    void Start()
    {
        train = transform.parent.GetComponent<MovingSpace>();
        rotationVector = new Vector3(0, 0, rotation / 2);
        _cam = Camera.main;
    }

    public override void Interact(GameObject player)
    {
        base.Interact(player);
        if (accelerating)
        {
            speedIndex++;
            if (speedIndex >= gears)
            {
                accelerating = false;
            }
        }
        else
        {
            speedIndex--;
            if (speedIndex == 0)
            {
                accelerating = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // _speedGague.transform.position = _cam.WorldToScreenPoint(leverBase.transform.position);
        // _speedGague.text = (speedIndex * maxSpeed / gears).ToString();
        rotationVector = new Vector3(0, 0, rotation/2 - speedIndex * rotation / gears);
        float step = turnSpeed * Time.deltaTime;
        train.speed = speedIndex*maxSpeed/gears;
        leverHand.transform.rotation = Quaternion.RotateTowards(leverHand.transform.rotation, Quaternion.Euler(leverHand.transform.parent.eulerAngles + rotationVector), step);
    }
}
