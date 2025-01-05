using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using RaycastHit = Unity.Physics.RaycastHit;

public class UnitSelectionManager : MonoBehaviour
{
    public PlayerInput playerInput;
    
    public static UnitSelectionManager Instance { get; private set; }
    
    public int playerTeam;
    public int player;
    
    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;
    public event EventHandler<NativeArray<Entity>> OnUnitsSelected;

    public Camera playerCamera;
    private bool _dragging;
    private Vector2 _selectionStartMousePos;
    
    private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
    
    private void Awake()
    {
        Instance = this;
        playerInput = PlayerInput.Instance;
        playerCamera = Camera.main;
    }

    private void OnEnable()
    {
        playerInput.PlayerInputActions.Player.LClickDrag.started += LClickStartDrag;
        playerInput.PlayerInputActions.Player.LClickDrag.performed += LClickPerformedDrag;
        playerInput.PlayerInputActions.Player.LClickDrag.canceled += LClickCanceledDrag;
        playerInput.PlayerInputActions.Player.RClick.canceled += RClickCanceled;
    }

    private void OnDisable()
    {
        playerInput.PlayerInputActions.Player.LClickDrag.started -= LClickStartDrag;
        playerInput.PlayerInputActions.Player.LClickDrag.performed -= LClickPerformedDrag;
        playerInput.PlayerInputActions.Player.LClickDrag.canceled -= LClickCanceledDrag;
        playerInput.PlayerInputActions.Player.RClick.canceled -= RClickCanceled;
    }

    private void RClickCanceled(InputAction.CallbackContext obj)
    {
        DeselectAll();
        OnUnitsSelected?.Invoke(null, new NativeArray<Entity>(0, Allocator.Temp));
    }

    private void LClickStartDrag(InputAction.CallbackContext context)
    {
        _selectionStartMousePos =
            playerInput.PlayerInputActions.Player.MousePosition.ReadValue<Vector2>();
    }

    private void LClickPerformedDrag(InputAction.CallbackContext context)
    {
        if (Vector2.Distance(_selectionStartMousePos,
                playerInput.PlayerInputActions.Player.MousePosition.ReadValue<Vector2>()) >= 30f)
        {
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
            _dragging = true;
        }
    }

    private void LClickCanceledDrag(InputAction.CallbackContext context)
    {
        if (!_dragging)
        {
            if (playerInput.isPointerOverUI)
            {
                return;
            }
            
            OnClickAction();
        }
        else
        {
            BoxMultiSelect();
        }
    }
    
    private void DeselectAll()
    {
        // Deselect All
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Selected>()
            .Build(EntityManager);

        NativeArray<Entity> selectedEntities = entityQuery.ToEntityArray(Allocator.Temp);
        NativeArray<Selected> entitiesSelectedComponents = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
        
        for (int i = 0; i < selectedEntities.Length; i++)
        {
            EntityManager.SetComponentEnabled<Selected>(selectedEntities[i], false);
            Selected selected = entitiesSelectedComponents[i];
            selected.OnDeselected = true;
            
            EntityManager.SetComponentData(selectedEntities[i], selected);
        }
    }
    
    private void OnClickAction()
    {
        EntityQuery physicsWorldSingletonQuery = EntityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        PhysicsWorldSingleton physicsWorldSingleton = physicsWorldSingletonQuery.GetSingleton<PhysicsWorldSingleton>();
            
        UnityEngine.Ray cameraRay = playerCamera.ScreenPointToRay(playerInput.PlayerInputActions.Player.MousePosition.ReadValue<Vector2>());
            
        RaycastInput raycastInput = new RaycastInput
        {
            Start = cameraRay.GetPoint(0f),
            End = cameraRay.GetPoint(9999f),
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << Assets.UNITS_LAYER,
                GroupIndex = 0
            }
        };
        
        if (physicsWorldSingleton.CollisionWorld.CastRay(raycastInput, out RaycastHit hit)
            && EntityManager.HasComponent<Unit>(hit.Entity))
        {
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Unit, Selected>()
                .Build(EntityManager);
            
            NativeArray<Entity> entityUnits = entityQuery.ToEntityArray(Allocator.Temp);
            
            Unit unit = EntityManager.GetComponentData<Unit>(hit.Entity);
            
            if (unit.Team != playerTeam && unit.Team != Assets.Neutral && entityUnits.Length > 0)
            {
                // Attack
                Debug.Log("Attack Enemy");
            }
            else if (unit.Owner == player)
            {
                SingleSelect(hit);
            }
            else
            {
                // Show highlight info
                Debug.Log("Show Highlight Info");
            }
        }
        else
        {
            MoveOnClickCommand();
        }
    }

    private void SingleSelect(RaycastHit hit)
    {
        DeselectAll();
        
        EntityManager.SetComponentEnabled<Selected>(hit.Entity, true);
        
        Selected selected = EntityManager.GetComponentData<Selected>(hit.Entity);
        selected.OnSelected = true;
        // selected.OnDeselected = false;
        
        EntityManager.SetComponentData(hit.Entity, selected);
        
        NativeArray<Entity> entities = new NativeArray<Entity>(1, Allocator.Temp);
        entities[0] = hit.Entity;
        
        OnUnitsSelected?.Invoke(this, entities);
    }

    private void BoxMultiSelect()
    {
        DeselectAll();
        
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<LocalTransform, Unit>()
            .WithPresent<Selected>()
            .Build(EntityManager);
                
        // Select what is inside the rect
        NativeArray<Entity> entityUnits = entityQuery.ToEntityArray(Allocator.Temp);
        NativeArray<LocalTransform> localTransformArray =
            entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        NativeList<Entity> entities = new NativeList<Entity>(Allocator.Temp);
        
        Rect selectionArea = GetSelectionArea();
        for (int i = 0; i < localTransformArray.Length; i++)
        {
            LocalTransform unitLocalTransform = localTransformArray[i];
            Vector2 unitScreenPos = playerCamera.WorldToScreenPoint(unitLocalTransform.Position);
            
            if (selectionArea.Contains(unitScreenPos))
            {
                EntityManager.SetComponentEnabled<Selected>(entityUnits[i], true);
                
                Selected selected = EntityManager.GetComponentData<Selected>(entityUnits[i]);
                selected.OnSelected = true;
                // selected.OnDeselected = false;
                
                EntityManager.SetComponentData(entityUnits[i], selected);
                
                entities.Add(entityUnits[i]);
            }
        }

        OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        OnUnitsSelected?.Invoke(this, entities.AsArray());
        
        _dragging = false;
    }
    
    private void MoveOnClickCommand()
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>()
            .Build(EntityManager);
        
        NativeArray<Entity> entityUnits = entityQuery.ToEntityArray(Allocator.Temp);
        if (entityUnits.Length < 1)
            return;
        
        NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

        Vector3 clickPosition = playerInput.GetPosition();
        
        if (clickPosition == Vector3.zero)
            return;
        
        NativeArray<float3> positions = MathematicalUtils.ArrangeUnitsInRings(
            clickPosition,
            unitMoverArray.Length,
            EntityManager.GetComponentData<LocalTransform>(entityUnits[0]).Scale);
                
        for (int i = 0; i < unitMoverArray.Length; i++)
        {
            UnitMover unitMover = unitMoverArray[i];
            unitMover.TargetPosition = positions[i];
            unitMoverArray[i] = unitMover;
        }
        
        entityQuery.CopyFromComponentDataArray(unitMoverArray);
    }
    
    public Rect GetSelectionArea()
    {
        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(_selectionStartMousePos.x, playerInput.mousePosition.x),
            Mathf.Min(_selectionStartMousePos.y, playerInput.mousePosition.y)
        );
        
        Vector2 upperRightCorner = new Vector2(
            Mathf.Max(_selectionStartMousePos.x, playerInput.mousePosition.x),
            Mathf.Max(_selectionStartMousePos.y, playerInput.mousePosition.y)
        );
        
        return new Rect(
            lowerLeftCorner.x, 
            lowerLeftCorner.y, 
            upperRightCorner.x - lowerLeftCorner.x, 
            upperRightCorner.y - lowerLeftCorner.y);
    }
}
