using Unity.Burst;
using Unity.Entities;
using UnityEngine;

// [UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(BulletSystem))]
partial struct HealthSystem : ISystem
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

        foreach (
            (
                RefRW<Health> health, 
                Entity entity
            )
            in SystemAPI.Query<RefRW<Health>>().WithEntityAccess())
        {
            if (health.ValueRW.HealthAmount <= 0)
            {
                foreach (RefRW<Target> deadTarget in SystemAPI.Query<RefRW<Target>>())
                {
                    if (deadTarget.ValueRO.TargetEntity == entity)
                    {
                        deadTarget.ValueRW.TargetEntity = Entity.Null;
                    }
                }
                
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }
}
