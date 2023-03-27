using SVS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CameraMovement cameraMovement;

    public InputManager inputManager;

    private void Start()
    {
        inputManager.OnMouseClick += HandleMouseclick;
    }

    private void HandleMouseclick(Vector3Int position)
    {
        Debug.Log(position);
    }


    private void Update()
    {
        cameraMovement.MoveCamera(new Vector3(inputManager.CameraMovementVector.x,0, inputManager.CameraMovementVector.y));
    }
}
