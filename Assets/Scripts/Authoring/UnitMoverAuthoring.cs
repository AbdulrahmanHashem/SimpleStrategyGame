using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitMoverAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;
    public float3 targetPosition;
}

public class UnitMoverAuthoringBaker : Baker<UnitMoverAuthoring>
{
    public override void Bake(UnitMoverAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        
        AddComponent(entity, new UnitMover
        {
            MoveSpeed = authoring.moveSpeed,
            RotateSpeed = authoring.rotateSpeed,
            TargetPosition = authoring.targetPosition,
        });
    }
}

public struct UnitMover : IComponentData
{
    public float MoveSpeed;
    public float RotateSpeed;
    public float3 TargetPosition;
}