using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class fpsCount : MonoBehaviour
{
    public Text textBox;
    private float _averagePile;
    private float _updateInterval = 0.25f;
    private float _timer;
    private int _counter;

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        _averagePile += 1f / Time.deltaTime;
        _counter++;
        if (_timer > _updateInterval)
        {
            textBox.text = (_averagePile / _counter).ToString("0.00") + "fps";
            _timer = 0;
            _averagePile = 0;
            _counter = 0;
        }
    }
}
