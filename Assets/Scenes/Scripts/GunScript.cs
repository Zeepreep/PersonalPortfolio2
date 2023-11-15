using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : MonoBehaviour
{
    public Transform cameraPosition;
    public Transform cameraRotation;
    private Vector3 gunOffset;
    public float rotationLerpSpeed = 5.0f;
    public bool applyRotationLag = true; 

    private Vector3 initialPosition;

    void Start()
    {
        gunOffset = transform.position - cameraPosition.transform.position;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = cameraPosition.transform.position + cameraRotation.transform.rotation * gunOffset;

        if (applyRotationLag)
        {
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, cameraRotation.transform.rotation, Time.deltaTime * rotationLerpSpeed);
            transform.rotation = newRotation;
        }
        else
        {
            transform.rotation = cameraRotation.transform.rotation;
        }

        transform.position = Vector3.Slerp(transform.position, targetPosition, 1);
    }

    public void SetRotationLag(bool enableRotationLag)
    {
        applyRotationLag = enableRotationLag;
    }
}