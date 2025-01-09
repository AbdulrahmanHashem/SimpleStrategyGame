using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(ShootAttackSystem))]
partial struct BulletSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        
        NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
        
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

            bulletTransform.ValueRW.Position += bullet.ValueRO.Direction * bullet.ValueRO.Speed * SystemAPI.Time.DeltaTime;
            
            
            float distance = math.distance(bullet.ValueRO.ShootPosition.Position, bulletTransform.ValueRO.Position);
            
            if (distance > bullet.ValueRO.MaxRange)
            {
                entityCommandBuffer.DestroyEntity(entity);
            }
            
            Vector3 middlePosition = Vector3.Lerp(bullet.ValueRO.ShootPosition.Position, bulletTransform.ValueRO.Position, 0.5f);
            quaternion bulletRotation = quaternion.LookRotation(bullet.ValueRO.Direction, Vector3.up);
            if (distance <= 0)
            {
                distance = 0;
            }
            else
            {
                distance /= 2;
            }
            
            float3 collisionBoxDim = new float3(
                bullet.ValueRO.ExplosionRadius / 2,
                bullet.ValueRO.ExplosionRadius / 2,
                distance);
            
            distanceHits.Clear();
            if (collisionWorld.OverlapBox(
                    middlePosition, 
                    bulletRotation,
                    collisionBoxDim,
                    ref distanceHits,
                    new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << Assets.UNITS_LAYER,
                        GroupIndex = 0
                    }))
            {
                foreach (var hit in distanceHits)
                {
                    if (hit.Entity == bullet.ValueRO.Shooter)
                        continue;
                    
                    if (!SystemAPI.HasComponent<Unit>(hit.Entity))
                        continue;

                    RefRW<Unit> hitUnit = SystemAPI.GetComponentRW<Unit>(hit.Entity);
                    RefRW<Unit> shooterUnit = SystemAPI.GetComponentRW<Unit>(bullet.ValueRO.Shooter);
                    if (hitUnit.ValueRO.Team == shooterUnit.ValueRO.Team)
                        continue;
                    
                    RefRW<Health> hitHealth = SystemAPI.GetComponentRW<Health>(hit.Entity);
                    hitHealth.ValueRW.HealthAmount -= bullet.ValueRO.DamageAmount;
                    hitHealth.ValueRW.OnHealthChanged = true;
                
                    entityCommandBuffer.DestroyEntity(entity);
                }
            }
        }
    }
}
