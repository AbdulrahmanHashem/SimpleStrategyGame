using Unity.Entities;
using UnityEngine;

class HealthVisualAuthoring : MonoBehaviour
{
    public GameObject healthBarGameObject;
    public GameObject unitGameObject;
}

class HealthVisualAuthoringBaker : Baker<HealthVisualAuthoring>
{
    public override void Bake(HealthVisualAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new HealthBar
        {
            HealthBarEntity = GetEntity(authoring.healthBarGameObject, TransformUsageFlags.NonUniformScale),
            UnitEntity = GetEntity(authoring.unitGameObject, TransformUsageFlags.Dynamic),
        });
    }
}

public struct HealthBar : IComponentData
{
    public Entity HealthBarEntity;
    public Entity UnitEntity;
}