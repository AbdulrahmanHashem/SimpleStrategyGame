using System;
using UnityEngine;

public class MouseWorldPosition : MonoBehaviour
{
    public static MouseWorldPosition Instance { get; private set; }
    
    public PlayerInputActions PlayerInputActions;
    
    private void Awake()
    {
        Instance = this;
        PlayerInputActions = new PlayerInputActions();
    }

    public Vector3 GetPosition()
    {
        Ray ray = Camera.main!.ScreenPointToRay(PlayerInputActions.Player.MousePosition.ReadValue<Vector2>());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private void OnEnable()
    {
        PlayerInputActions.Enable();
    }

    private void OnDisable()
    {
        PlayerInputActions.Disable();
    }
}
