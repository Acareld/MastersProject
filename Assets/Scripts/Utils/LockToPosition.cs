using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Simple script: once placed on an object and given another object to lock to will
 * lock the current object to the assigned objects position.
 * 
 * Includes an option for also matching rotation of current object to assigned object.
 */

[ExecuteAlways]
public class LockToPosition : MonoBehaviour
{
    [Header("Components")]
    public Transform objToLockTo;
    public bool shouldLockRotation = false;
    public bool useFixedUpdate = false;
    public float lerpSpeed = 3.0f;
    public bool disableLock = false;

    void Update()
    {
        if (useFixedUpdate) return;
        if (disableLock) return;

        transform.position = objToLockTo.position;
        if (shouldLockRotation) transform.rotation = objToLockTo.rotation;
    }

    void FixedUpdate()
    {
        if (!useFixedUpdate) return;
        if (disableLock) return;
        transform.position = Vector3.Lerp(transform.position, objToLockTo.position, lerpSpeed * Time.deltaTime);

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = objToLockTo.rotation.eulerAngles;

        Quaternion targetRotation = Quaternion.Euler(
        targetEuler.x,   
        currentEuler.y,  
        targetEuler.z   
    );

        if (shouldLockRotation) transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lerpSpeed * Time.deltaTime);
    }
}
