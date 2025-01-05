using Unity.Entities;
using UnityEngine;

class TargetAuthoring : MonoBehaviour
{
    public GameObject target;
}

class TargetAuthoringBaker : Baker<TargetAuthoring>
{
    public override void Bake(TargetAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Target
        {
            TargetEntity = GetEntity(authoring.target, TransformUsageFlags.Dynamic),
        });
    }
}

public struct Target : IComponentData
{
    public Entity TargetEntity;
}