using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPosRot : MonoBehaviour
{

    private Vector3 tempPos, currentPos, tempRot;
    private Quaternion currentRot;

    public Transform copyFrom;
    public Vector3 positionOffset = Vector3.zero;

    public bool copyPos = false;
    public bool lockXPos = false, lockYPos = false, lockZPos = false;
    public Vector3 lockedPosition = Vector3.zero;
    public bool copyRot = false;
    public bool lockXRot = false, lockYRot = false, lockZRot = false;
    public Vector3 lockedRotation = Vector3.zero;

    void Awake()
    {
        if (copyFrom == null)
        {
            Debug.LogWarning("CopyPosRot: No transform given of which the position and rotation should be copied for " + gameObject.name);
            if (transform.parent != null)
                copyFrom = transform.parent;
            else
                copyFrom = transform;
        }
    }

    // Use this for initialization
    void Start()
    {
        if (copyFrom == null)
            return;

        currentPos = copyFrom.position;
        tempPos = Vector3.zero;

        currentRot = copyFrom.rotation;
        tempRot = Vector3.zero;

        lockedPosition = transform.position;
        lockedRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (copyFrom == null)
            return;

        currentPos = copyFrom.position + positionOffset;

        tempPos.x = lockXPos ? lockedPosition.x : currentPos.x;
        tempPos.y = lockYPos ? lockedPosition.y : currentPos.y;
        tempPos.z = lockZPos ? lockedPosition.z : currentPos.z;

        //transform.position = Vector3.zero;

        currentRot = copyFrom.rotation;
        tempRot.x = lockXRot ? lockedRotation.x : currentRot.eulerAngles.x;
        tempRot.y = lockYRot ? lockedRotation.y : currentRot.eulerAngles.y;
        tempRot.z = lockZRot ? lockedRotation.z : currentRot.eulerAngles.z;

        if(copyRot)
            transform.rotation = Quaternion.Euler(tempRot);

        if(copyPos)
            transform.position = tempPos;
    }
}
