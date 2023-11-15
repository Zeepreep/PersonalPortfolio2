using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraScript : MonoBehaviour
{
    public float lookingSpeed = 3;
    public Transform orientation;
    
    void Update()
    {
        cameraMovement();
    }

    void cameraMovement()
    {
        float zInput = 0;

        
            if (Input.GetKey(KeyCode.Q))
            {
                zInput++;
            } 
            
            if (Input.GetKey(KeyCode.E))
            {
                zInput--;
            }
        
        float rotationZ = transform.localEulerAngles.z + zInput * lookingSpeed;

        float rotationX = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * lookingSpeed;
        float rotationY = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * lookingSpeed;

        Mathf.Clamp(rotationX, -90f, 90f);

        Mathf.Clamp(rotationZ, -30f, 30f);
            
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
        orientation.rotation = Quaternion.Euler(0, rotationY, 0);


    }
}