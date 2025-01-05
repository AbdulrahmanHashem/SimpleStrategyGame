using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class HealthAuthoring : MonoBehaviour
{
    public int healthAmount;
    public int maxHealthAmount;
    public Transform centerOfMass; 
}

class HealthAuthoringBaker : Baker<HealthAuthoring>
{
    public override void Bake(HealthAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Health
        {
            HealthAmount = authoring.healthAmount,
            MaxHealthAmount = authoring.maxHealthAmount,
            CenterOfMass = authoring.centerOfMass.localPosition,
            OnHealthChanged = true
        });
    }
}

public struct Health : IComponentData
{
    public int HealthAmount;
    public int MaxHealthAmount;
    public float3 CenterOfMass; 
    
    public bool OnHealthChanged;
}