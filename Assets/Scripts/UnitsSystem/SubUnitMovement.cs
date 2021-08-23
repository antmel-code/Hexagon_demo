using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubUnitMovement : MonoBehaviour
{
    public System.Action onDestinationReached = () => {};

    

    [SerializeField]
    float maxHorizontalSpeed = 10f;

    [SerializeField]
    float horizontalAcceleration = 1f;

    [SerializeField]
    float stoppingDistance = 1f;

    Vector3 velocity = Vector3.zero;

    Vector3 destination;

    bool isStoped = true;

    Queue<Vector3> path = new Queue<Vector3>();

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        destination = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();

        CheckGround();
       
        UpdateRotation();
    }

    void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up* 1000, Vector3.down, out hit))
        {
            transform.position = hit.point;
        }
    }

    void UpdateMovement()
    {
        // Movement
        transform.Translate(velocity * Time.deltaTime, Space.World);

        // If destination reached
        if (!isStoped && Vector3.ProjectOnPlane(destination - transform.position, Vector3.up).magnitude < stoppingDistance)
        {
            onDestinationReached.Invoke();
            if (path.Count > 0)
            {
                MoveTo(path.Dequeue());
            }
            else
            {
                isStoped = true;
            }

        }

        // Acceleration
        if (!isStoped || velocity.magnitude != 0)
        {
            Vector3 distanceVector = destination - transform.position;

            Vector3 horizontalDistanceVector = Vector3.ProjectOnPlane(distanceVector, Vector3.up);
            //float horizonlatSpeed = Mathf.Min(maxHorizontalSpeed, Mathf.Clamp(horizontalDistanceVector.magnitude - stoppingDistance, 0, maxHorizontalSpeed) / Time.deltaTime);
            float horizonlatSpeed = horizontalDistanceVector.magnitude > stoppingDistance ? maxHorizontalSpeed : 0;
            Vector3 horizontalDesiredVelocity = Vector3.Normalize(horizontalDistanceVector) * horizonlatSpeed;

            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);

            Vector3 neededHorizontalAcceleration = (horizontalDesiredVelocity - horizontalVelocity) / Time.deltaTime;
            if (neededHorizontalAcceleration.magnitude > horizontalAcceleration)
            {
                neededHorizontalAcceleration = Vector3.Normalize(neededHorizontalAcceleration) * horizontalAcceleration;
            }

            Vector3 neededAcceleration = neededHorizontalAcceleration;
            velocity += neededAcceleration * Time.deltaTime;
        }
    }

    void UpdateRotation()
    {
        transform.LookAt(transform.position + velocity);
    }

    public void AddCheckpoint(Vector3 position)
    {
        path.Enqueue(position);
        if (isStoped)
        {
            MoveTo(path.Dequeue());
            isStoped = false;
        }
    }

    void MoveTo(Vector3 position)
    {
        destination = position;
    }
}
