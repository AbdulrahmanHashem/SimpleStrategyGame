using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthVisualSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Vector3 cameraForward = Vector3.zero;
        if (Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
        }
        
        foreach ((
                RefRW<LocalTransform> localTransform ,
                RefRO<HealthBar> healthBar) 
            in SystemAPI.Query<
                RefRW<LocalTransform>, 
                RefRO<HealthBar>>())
        {
            LocalTransform parentTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.UnitEntity);
            if (localTransform.ValueRO.Scale == 1f)
            {
                localTransform.ValueRW.Rotation = 
                    parentTransform.InverseTransformRotation(
                        quaternion.LookRotation(cameraForward, math.up()));
            }
            
            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.UnitEntity);

            if (health.OnHealthChanged)
            {
                float healthNorm = ((float)health.HealthAmount / health.MaxHealthAmount) / 10;

                if (healthNorm == 0.1f)
                {
                    localTransform.ValueRW.Scale = 0f;
                }
                else
                {
                    localTransform.ValueRW.Scale = 1f;
                }
            
                RefRW<PostTransformMatrix> healthBarTransform = 
                    SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.HealthBarEntity);

                float3 currentTransform = healthBarTransform.ValueRO.Value.Scale();
                healthBarTransform.ValueRW.Value = 
                    float4x4.Scale(
                        healthNorm,
                        currentTransform.y,
                        currentTransform.z);
            }
        }
    }
}
