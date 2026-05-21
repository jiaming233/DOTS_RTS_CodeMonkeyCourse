using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineVirtualCamera cinemachineVirtualCamera;

    [SerializeField] private float moveSpeed = 30f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float zoomSpeed = 10f;

    [SerializeField] private float fieldOfViewMin = 20;
    [SerializeField] private float fieldOfViewMax = 60;

    private float targetFieldOfView;

    private void Awake()
    {
        targetFieldOfView = cinemachineVirtualCamera.m_Lens.FieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDir.z += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir.z -= 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir.x += 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir.x += 1f;
        }

        moveDir = Camera.main.transform.forward * moveDir.z + Camera.main.transform.right * moveDir.x;
        moveDir.y = 0f;
        moveDir.Normalize();

        transform.position += moveDir * moveSpeed * Time.deltaTime;


        float rotationAmount = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            rotationAmount = 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotationAmount = -1f;
        }

        transform.eulerAngles += new Vector3(0f, rotationAmount * rotationSpeed * Time.deltaTime, 0f);

        float zoomAmout = 4f;
        if(Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView -= zoomAmout;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView += zoomAmout;
        }
        targetFieldOfView = Mathf.Clamp(targetFieldOfView, fieldOfViewMin, fieldOfViewMax);

        cinemachineVirtualCamera.m_Lens.FieldOfView
            = Mathf.Lerp(cinemachineVirtualCamera.m_Lens.FieldOfView, targetFieldOfView, zoomSpeed * Time.deltaTime);
    }
}
