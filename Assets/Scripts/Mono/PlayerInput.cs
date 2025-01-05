using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }
    
    public PlayerInputActions PlayerInputActions;
    
    public bool isPointerOverUI;
    
    public Vector2 mousePosition;
    
    private void Awake()
    {
        PlayerInputActions = new PlayerInputActions();

        Instance = this;
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
        
        PlayerInputActions.Player.MousePosition.performed += MousePositionChangePerformed;
    }
    
    private void OnDisable()
    {
        PlayerInputActions.Disable();
        
        PlayerInputActions.Player.MousePosition.performed -= MousePositionChangePerformed;
    }
    
    public void SetPointerOverUI(bool state)
    {
        isPointerOverUI = state;
    }
    
    private void MousePositionChangePerformed(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
    }
}
