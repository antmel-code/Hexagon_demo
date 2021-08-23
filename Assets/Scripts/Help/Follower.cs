using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField]
    bool keepOffset;

    Vector3 offset = Vector3.zero;

    public void SetTarget(Transform target, bool keepOffset = false)
    {
        if (keepOffset)
        {
            offset = transform.position - target.position;
        }
        else
        {
            offset = Vector3.zero;
        }
        this.target = target;
        this.keepOffset = keepOffset;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (target != null)
        {
            SetTarget(target, keepOffset);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
