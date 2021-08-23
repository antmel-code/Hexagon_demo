using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCameraController : MonoBehaviour
{

    [SerializeField]
    Transform swivel;
    [SerializeField]
    Transform stick;

    [SerializeField, Range(1, 100)]
    float minMoveSpeed = 40;

    [SerializeField, Range(1, 100)]
    float maxMoveSpeed = 100;

    float zoom = 1f;

    float targetZoom = 1f;

    [SerializeField, Range(1, 10)]
    float zoomSpeed = 5f;

    [SerializeField, Range(0, -1000)]
    float maxStickZoom = -10;

    [SerializeField, Range(0, -1000)]
    float minStickZoom = -200;

    [SerializeField, Range(0, 90)]
    float maxSwivelZoom = 45;

    [SerializeField, Range(0, 90)]
    float minSwivelZoom = 90;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }

        UpdateZoom();
    }

    void UpdateZoom()
    {
        zoom = Mathf.Lerp(zoom, targetZoom, zoomSpeed * Time.deltaTime);

        float stickZoom = Mathf.Lerp(minStickZoom, maxStickZoom, zoom);
        float swivelZoom = Mathf.Lerp(minSwivelZoom, maxSwivelZoom, zoom);
        stick.localPosition = new Vector3(0, 0, stickZoom);
        swivel.localRotation = Quaternion.Euler(swivelZoom, 0, 0);
    }

    void AdjustZoom(float delta)
    {
        targetZoom = Mathf.Clamp01(zoom + delta);
    }

    void AdjustPosition(float xDelta, float zDelta)
    {
        
        Vector3 direction = new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float moveSpeed = Mathf.Lerp(maxMoveSpeed, minMoveSpeed, zoom);
        transform.position += direction * damping * moveSpeed * Time.deltaTime;
    }

    bool GetCursorPosition(out Vector3 position)
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            position = hit.point;
            return true;
        }

        position = Vector3.zero;
        return false;
    }
}
