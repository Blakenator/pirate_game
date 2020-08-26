using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float lookSpeed = 5f;
    float xRotation = 0f;
    float yRotation = 0f;
    public float horizontalLimit = 90f;
    public float verticalLimit = 90f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);
        yRotation = Mathf.Clamp(yRotation, -horizontalLimit, horizontalLimit);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
