using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }
    
    public int unitLayer;

    private void Awake()
    {
        Instance = this;
    }

    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;
    
    bool _dragging;
    private Vector2 _selectionStartMousePos;
    private Vector2 _mousePosition;
    
    private EntityManager _entityManager;

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        MouseWorldPosition.Instance.PlayerInputActions.Player.LClickDrag.started += LClickStartDrag;
        MouseWorldPosition.Instance.PlayerInputActions.Player.LClickDrag.performed += LClickPerformedDrag;
        MouseWorldPosition.Instance.PlayerInputActions.Player.LClickDrag.canceled += LClickCanceledDrag;

        MouseWorldPosition.Instance.PlayerInputActions.Player.MousePosition.performed += MousePositionChangePerformed;
    }
    
    void LClickStartDrag(InputAction.CallbackContext context)
    {
        _selectionStartMousePos =
            MouseWorldPosition.Instance.PlayerInputActions.Player.MousePosition.ReadValue<Vector2>();
    }
    
    void MousePositionChangePerformed(InputAction.CallbackContext context)
    {
        _mousePosition = context.ReadValue<Vector2>();
    }
    
    void LClickPerformedDrag(InputAction.CallbackContext context)
    {
        if (Vector2.Distance(_selectionStartMousePos,
                MouseWorldPosition.Instance.PlayerInputActions.Player.MousePosition.ReadValue<Vector2>()) >= 30f)
        {
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
            _dragging = true;
        }
    }
    
    void LClickCanceledDrag(InputAction.CallbackContext context)
    {
        if (!_dragging)
        {
            ClickSelectOrMoveCommand();
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
            .Build(_entityManager);

        NativeArray<Entity> selectedEntities = entityQuery.ToEntityArray(Allocator.Temp);
        NativeArray<Selected> entitiesSelectedComponents = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
        
        for (int i = 0; i < selectedEntities.Length; i++)
        {
            _entityManager.SetComponentEnabled<Selected>(selectedEntities[i], false);
            Selected selected = entitiesSelectedComponents[i];
            selected.OnDeselected = true;
            
            _entityManager.SetComponentData(selectedEntities[i], selected);
        }
    }
    
    private void ClickSelectOrMoveCommand()
    {
        EntityQuery physicsWorldSingletonQuery = _entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        PhysicsWorldSingleton physicsWorldSingleton = physicsWorldSingletonQuery.GetSingleton<PhysicsWorldSingleton>();
            
        UnityEngine.Ray cameraRay = Camera.main!.ScreenPointToRay(MouseWorldPosition.Instance.PlayerInputActions.Player.MousePosition.ReadValue<Vector2>());
            
        RaycastInput raycastInput = new RaycastInput
        {
            Start = cameraRay.GetPoint(0f),
            End = cameraRay.GetPoint(9999f),
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << unitLayer,
                GroupIndex = 0
            }
        };
            
        if (physicsWorldSingleton.CollisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit hit)
            && _entityManager.HasComponent<Unit>(hit.Entity))
        {
            DeselectAll();
            _entityManager.SetComponentEnabled<Selected>(hit.Entity, true);
            
            Selected selected = _entityManager.GetComponentData<Selected>(hit.Entity);
            selected.OnSelected = true;
            selected.OnDeselected = false;
            
            _entityManager.SetComponentData(hit.Entity, selected);
        }
        else
        {
            MoveOnClickCommand();
        }
    }
    
    private void MoveOnClickCommand()
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>()
            .Build(World.DefaultGameObjectInjectionWorld.EntityManager);
        
        NativeArray<Entity> entityUnits = entityQuery.ToEntityArray(Allocator.Temp);
        if (entityUnits.Length < 1)
            return;
        
        NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
        
        NativeArray<float3> positions = ArrangeUnitsInRings(
            MouseWorldPosition.Instance.GetPosition(),
            unitMoverArray.Length,
            _entityManager.GetComponentData<LocalTransform>(entityUnits[0]).Scale);
                
        for (int i = 0; i < unitMoverArray.Length; i++)
        {
            UnitMover unitMover = unitMoverArray[i];
            unitMover.TargetPosition = positions[i];
            unitMoverArray[i] = unitMover;
        }

        entityQuery.CopyFromComponentDataArray(unitMoverArray);
    }

    private void BoxMultiSelect()
    {
        DeselectAll();
        
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<LocalTransform, Unit>()
            .WithPresent<Selected>()
            .Build(_entityManager);
                
        // Select what is inside the rect
        NativeArray<Entity> entityUnits = entityQuery.ToEntityArray(Allocator.Temp);
        NativeArray<LocalTransform> localTransformArray =
            entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        Rect selectionArea = GetSelectionArea();
        for (int i = 0; i < localTransformArray.Length; i++)
        {
            LocalTransform unitLocalTransform = localTransformArray[i];
            Vector2 unitScreenPos = Camera.main!.WorldToScreenPoint(unitLocalTransform.Position);
            if (selectionArea.Contains(unitScreenPos))
            {
                World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentEnabled<Selected>(entityUnits[i],
                    true);
                
                Selected selected = _entityManager.GetComponentData<Selected>(entityUnits[i]);
                selected.OnSelected = true;
                selected.OnDeselected = false;
            
                _entityManager.SetComponentData(entityUnits[i], selected);
            }
        }

        OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        _dragging = false;
    }
    
    public Rect GetSelectionArea()
    {
        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(_selectionStartMousePos.x, _mousePosition.x),
            Mathf.Min(_selectionStartMousePos.y, _mousePosition.y)
        );
        
        Vector2 upperRightCorner = new Vector2(
            Mathf.Max(_selectionStartMousePos.x, _mousePosition.x),
            Mathf.Max(_selectionStartMousePos.y, _mousePosition.y)
        );
        
        return new Rect(
            lowerLeftCorner.x, 
            lowerLeftCorner.y, 
            upperRightCorner.x - lowerLeftCorner.x, 
            upperRightCorner.y - lowerLeftCorner.y);
    }
    
    /// <summary>
    /// Arranges units: first one at the center (targetPosition), then subsequent units in concentric rings.
    /// Each ring is placed to avoid overlap, with a margin around each unit and each ring.
    /// </summary>
    /// <param name="targetPosition">The center position where the first unit goes.</param>
    /// <param name="count">Total number of units to place (including the center one).</param>
    /// <param name="unitSize">The diameter of each unit's footprint.</param>
    /// <param name="allocator">Memory allocator for the NativeArray.</param>
    /// <returns>A NativeArray of float3 positions for the units.</returns>
    public NativeArray<float3> ArrangeUnitsInRings(float3 targetPosition, int count, float unitSize, Allocator allocator = Allocator.Temp)
    {
        if (count <= 0)
            return new NativeArray<float3>(0, allocator);

        // We'll store all positions
        var positions = new NativeArray<float3>(count, allocator);

        // Place the first unit at the center
        positions[0] = targetPosition;
        if (count == 1)
            return positions; // Only one unit requested

        int unitsPlaced = 1;
        int unitsLeft = count - 1;

        // Margins and spacing calculations
        float ringMargin = unitSize * 0.5f;    // Margin added between rings
        float unitSpacing = unitSize * 1.5f;   // Each unit on the ring needs unitSize + 0.2*unitSize = 1.2 * unitSize of circumference space

        // Calculate the first ring radius:
        // first ring radius = center unit radius + ring unit radius + ring margin = 0.5*unitSize + 0.5*unitSize + 0.5*unitSize = 1.5 * unitSize
        float ringRadius = 1.5f * unitSize;

        // Place remaining units in rings
        while (unitsLeft > 0)
        {
            float circumference = 2f * math.PI * ringRadius;
            int unitsOnThisRing = (int)math.floor(circumference / unitSpacing);

            // Ensure at least one unit can be placed
            if (unitsOnThisRing < 1)
                unitsOnThisRing = 1;

            // If we have fewer units left than fits on the ring, adjust
            if (unitsOnThisRing > unitsLeft)
                unitsOnThisRing = unitsLeft;

            float angleStep = 2f * math.PI / unitsOnThisRing;

            // Place units evenly on this ring
            for (int i = 0; i < unitsOnThisRing; i++)
            {
                float angle = i * angleStep;
                float x = targetPosition.x + ringRadius * math.cos(angle);
                float z = targetPosition.z + ringRadius * math.sin(angle);
                // Keep the same Y as the target position
                positions[unitsPlaced] = new float3(x, targetPosition.y, z);
                unitsPlaced++;
                unitsLeft--;
            }

            // Increase radius for the next ring:
            // next ring radius increment = unitSize + ringMargin
            ringRadius += (unitSize + ringMargin);
        }

        return positions;
    }
    
}
