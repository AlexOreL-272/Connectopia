using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;

    [SerializeField] private LayerMask placementLayermask;

    private Vector3 lastPosition;

    // private void Update()
    // {
    //     if (Input.GetButton("Fire1"))
    //         OnClicked?.Invoke();
    //     if (Input.GetKeyDown(KeyCode.Escape))
    //         OnExit?.Invoke();
    // }
    //
    // public event Action OnClicked, OnExit;

    // public bool IsPointerOverUI()
    //     => EventSystem.current.IsPointerOverGameObject();

    public Vector3 GetSelectedMapPosition()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        var ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, placementLayermask)) lastPosition = hit.point;
        return lastPosition;
    }
}