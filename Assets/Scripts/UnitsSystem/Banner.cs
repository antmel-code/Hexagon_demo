using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Follower))]
public class Banner : MonoBehaviour
{
    Follower follower;

    void Awake()
    {
        follower = GetComponent<Follower>();
    }

    public void SetOwner(Transform owner)
    {
        follower.SetTarget(owner);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
