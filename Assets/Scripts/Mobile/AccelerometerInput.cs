using System.Collections;
using UnityEngine;

public class AccelerometerInput : MonoBehaviour
{
    private void Update()
    {
        transform.Translate(Input.acceleration.x, 0, -Input.acceleration.z);
    }
}
