using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    public bool rotationbool = false;
    public bool lookaround = false;
    public GameObject city;
    
    private float x;
    private float y;
    private Vector3 rotateValue;
    
    
    // Update is called once per frame
    void Update()
    {
        if (rotationbool)
        {
            Vector3 citypos = city.transform.position;
            transform.RotateAround(citypos,Vector3.up, 100 * Time.deltaTime);
        }
        else if (lookaround)
        {
            y = Input.GetAxis("Mouse X");
            x = Input.GetAxis("Mouse Y");
            rotateValue = new Vector3(x, y * -1, 0);
            transform.eulerAngles = transform.eulerAngles - rotateValue;
        }
    }
}