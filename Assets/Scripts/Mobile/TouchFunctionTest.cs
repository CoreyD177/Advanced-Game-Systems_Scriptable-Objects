using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchFunctionTest : MonoBehaviour
{
    public Renderer testRend;
    public Color[] testColours = new Color[4];
    public void Start()
    {
        testRend = this.GetComponent<Renderer>();
    }
    public void OnTouchDown()
    {
        testRend.material.color = testColours[0];
    }

    public void OnTouchStay()
    {
        testRend.material.color = testColours[1];
    }
    public void OnTouchUp()
    {
        testRend.material.color = testColours[2];
    }
    public void OnTouchExit()
    {
        testRend.material.color = testColours[3];
    }
}
