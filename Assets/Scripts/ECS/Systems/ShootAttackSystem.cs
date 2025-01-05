using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(DetectorSystem))]
partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        
        foreach (
            (
                RefRO<LocalTransform> localTransform,
                RefRW<ShootAttack> shootAttack,
                RefRW<Target> target
            )
            in SystemAPI.Query
            <
                RefRO<LocalTransform>,
                RefRW<ShootAttack>, 
                RefRW<Target>
            >() 
                // .WithEntityAccess()
            )
        {
            if (target.ValueRO.TargetEntity == Entity.Null)
            {
                continue;
            }
            
            if (!state.EntityManager.Exists(target.ValueRO.TargetEntity))
            {
                continue;
            }
            
            shootAttack.ValueRW.Time -= SystemAPI.Time.DeltaTime;
            if (shootAttack.ValueRW.Time > 0f)
            {
                continue;
            }
            
            shootAttack.ValueRW.Time = shootAttack.ValueRW.TimerMax;
            
            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.TargetEntity);
            
            float distance = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);
            
            if (distance <= shootAttack.ValueRO.AttackDistance)
            {
                Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.BulletPrefabEntity);
                float3 bulletSpawn = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.BulletStartPosition);
                LocalTransform transform = LocalTransform.FromPosition(bulletSpawn);
                SystemAPI.SetComponent(bulletEntity, transform);
                
                RefRW<Bullet> bullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
                bullet.ValueRW.DamageAmount = shootAttack.ValueRO.Damage;
                
                RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
                bulletTarget.ValueRW.TargetEntity = target.ValueRO.TargetEntity;
            }
        }
    }
}
