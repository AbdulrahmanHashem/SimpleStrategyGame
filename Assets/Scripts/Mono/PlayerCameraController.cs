using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private GameObject controller;
    // public Camera playerCamera;
    
    public PlayerInput playerInput;
 
    private const float CameraAngle = 42;

    private Vector2 _mouseLocation;
    private Vector2 _arrows;
    private float _scroll;
    
    private Vector2 _mouseStartPosition;
    private Vector2 _rotateAction;

    private float _leftEdge;
    private float _rightEdge;
    private float _bottomEdge;
    private float _topEdge;
    
    private void Awake()
    {
        playerInput = PlayerInput.Instance;
        
        // Get the screen dimensions
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        _leftEdge = screenWidth * Settings.Instance.edgeThreshold;
        _rightEdge = screenWidth * (1 - Settings.Instance.edgeThreshold);
        _bottomEdge = screenHeight * Settings.Instance.edgeThreshold;
        _topEdge = screenHeight * (1 - Settings.Instance.edgeThreshold);
    }
    
    private void Update()
    {
        if (_scroll != 0)
        {
            if (!(Mathf.Approximately(controller.transform.position.y, Settings.Instance.minZoom) && _scroll > 0 ||
                  Mathf.Approximately(controller.transform.position.y, Settings.Instance.maxZoom) && _scroll < 0))
            {
                Vector3 forwardDirection = Quaternion.Euler(CameraAngle, controller.transform.eulerAngles.y, 0f) * Vector3.forward;

                Vector3 potentialPosition =
                    controller.transform.position + forwardDirection * (_scroll * Settings.Instance.zoomSpeed);

                potentialPosition.y = Mathf.Clamp(potentialPosition.y, Settings.Instance.minZoom, Settings.Instance.maxZoom);

                controller.transform.position = potentialPosition;
            }
        }
        else if (_rotateAction.y != 0)
        {
            float center = _rotateAction.x - _rotateAction.y;
            center /= 5;

            controller.transform.Rotate(0f, Quaternion.Euler(0f, center, 0f).y, 0f);
        }
        else
        {
            Vector3 position = controller.transform.position;

            Vector3 mouseScreenEdgeDirection = MouseScreenEdgeDirection();

            mouseScreenEdgeDirection += new Vector3(_arrows.x, 0f, _arrows.y);
            mouseScreenEdgeDirection.Normalize();
            mouseScreenEdgeDirection = controller.transform.TransformDirection(mouseScreenEdgeDirection);

            position += mouseScreenEdgeDirection;

            if (Settings.Instance.cameraMovementSpeed < 5f)
            {
                Settings.Instance.cameraMovementSpeed = 15f;
            }

            controller.transform.position = Vector3.Lerp(
                controller.transform.position,
                position,
                Settings.Instance.cameraMovementSpeed * Time.deltaTime
            );
        }
    }

    private void OnEnable()
    {
        playerInput.PlayerInputActions.Enable();

        playerInput.PlayerInputActions.Player.MousePosition.performed += MouseLocationPerformed;
        playerInput.PlayerInputActions.Player.Arrows.performed += ArrowsPerformed;
        playerInput.PlayerInputActions.Player.Arrows.canceled += ArrowsCanceled;
        playerInput.PlayerInputActions.Player.Zoom.started += ZoomStarted;
        playerInput.PlayerInputActions.Player.Zoom.canceled += ZoomCanceled;
        playerInput.PlayerInputActions.Player.Rotate.started += RotateStarted;
        playerInput.PlayerInputActions.Player.Rotate.performed += RotatePerformed;
        playerInput.PlayerInputActions.Player.Rotate.canceled += RotateCanceled;
    }

    private void OnDisable()
    {
        playerInput.PlayerInputActions.Disable();

        playerInput.PlayerInputActions.Player.MousePosition.performed -= MouseLocationPerformed;
        playerInput.PlayerInputActions.Player.Arrows.performed -= ArrowsPerformed;
        playerInput.PlayerInputActions.Player.Arrows.canceled -= ArrowsCanceled;
        playerInput.PlayerInputActions.Player.Zoom.started -= ZoomStarted;
        playerInput.PlayerInputActions.Player.Zoom.canceled -= ZoomCanceled;
        playerInput.PlayerInputActions.Player.Rotate.started -= RotateStarted;
        playerInput.PlayerInputActions.Player.Rotate.performed -= RotatePerformed;
        playerInput.PlayerInputActions.Player.Rotate.canceled -= RotateCanceled;
    }

    private Vector3 MouseScreenEdgeDirection()
    {
        // Determine the movement direction based on mouse position
        Vector3 moveDirection = Vector3.zero;
        
        if (_mouseLocation.x < _leftEdge)
        {
            moveDirection.x = -1f;
        }
        else if (_mouseLocation.x > _rightEdge)
        {
            moveDirection.x = 1f;
        }
        
        if (_mouseLocation.y < _bottomEdge)
        {
            moveDirection.z = -1f; // Assuming forward is along the z-axis
        }
        else if (_mouseLocation.y > _topEdge)
        {
            moveDirection.z = 1f;
        }

        moveDirection.Normalize();
        
        return moveDirection;
    }

    private void MouseLocationPerformed(InputAction.CallbackContext context)
    {
        _mouseLocation = context.ReadValue<Vector2>();
    }

    private void ArrowsPerformed(InputAction.CallbackContext context)
    {
        _arrows = context.ReadValue<Vector2>();
    }

    private void ArrowsCanceled(InputAction.CallbackContext context)
    {
        _arrows = Vector2.zero;
    }

    private void ZoomStarted(InputAction.CallbackContext context)
    {
        _scroll = context.ReadValue<Vector2>().y;
    }

    private void ZoomCanceled(InputAction.CallbackContext context)
    {
        _scroll = 0f;
    }

    private void RotateStarted(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
    
        _rotateAction.y = value.x;
    }

    private void RotatePerformed(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        
        _rotateAction.x = value.x;
    }

    private void RotateCanceled(InputAction.CallbackContext context)
    {
        _rotateAction = Vector2.zero;
    }
}
