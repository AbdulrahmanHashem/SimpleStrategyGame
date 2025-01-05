using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(ShootAttackSystem))]
partial struct BulletSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        
        foreach ((RefRW<LocalTransform> bulletTransform,
                     RefRO<Bullet> bullet,
                     RefRO<Target> target,
                     Entity entity)
                 in SystemAPI.Query<
                     RefRW<LocalTransform>,
                     RefRO<Bullet>,
                     RefRO<Target>>().WithEntityAccess())
        {
            if (!state.EntityManager.Exists(target.ValueRO.TargetEntity))
            {
                entityCommandBuffer.DestroyEntity(entity); 
                continue;
            }
            
            if (SystemAPI.GetComponentRO<Health>(target.ValueRO.TargetEntity).ValueRO.HealthAmount < 1)
            {
                entityCommandBuffer.DestroyEntity(entity);  
                continue;
            }
            
            Health health = SystemAPI.GetComponent<Health>(target.ValueRO.TargetEntity);
            LocalTransform targetTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.TargetEntity);
            
            float3 targetCenterOfMass = targetTransform.TransformPoint(health.CenterOfMass);
            // float3 targetCenterOfMass = LocalTransform.FromPosition(health.CenterOfMass).Position;
            float3 moveDirection = targetCenterOfMass - bulletTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);
            
            bulletTransform.ValueRW.Position += moveDirection * bullet.ValueRO.Speed * SystemAPI.Time.DeltaTime;
            
            quaternion bulletRotation = quaternion.LookRotation(moveDirection, Vector3.up);
            bulletTransform.ValueRW.Rotation = bulletRotation;
            
            float hitDistanceSq = 0.05f;
            if (math.distancesq(bulletTransform.ValueRO.Position, targetCenterOfMass) < hitDistanceSq)
            {
                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.TargetEntity);
                targetHealth.ValueRW.HealthAmount -= bullet.ValueRO.DamageAmount;
                targetHealth.ValueRW.OnHealthChanged = true;
                
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }
}
