using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationOffseter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        animator.Play("metarig|Eat", -1, Random.Range(0f, 1f));
    }

    
}
