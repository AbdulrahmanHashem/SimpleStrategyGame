using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
partial struct DetectorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        
        NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
        foreach (
            (
                RefRO<LocalTransform> localTransform,
                RefRW<Detector> findTarget,
                RefRW<Unit> unit,
                RefRW<Target> target
            )
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<Detector>,
                RefRW<Unit>,
                RefRW<Target>>()
            )
        {
            findTarget.ValueRW.Time -= SystemAPI.Time.DeltaTime;
            
            if (findTarget.ValueRW.Time > 0)
            {
                continue;
            }
            findTarget.ValueRW.Time = findTarget.ValueRW.MaxTime;
            
            distanceHits.Clear();
            if (collisionWorld.OverlapSphere
                (
                    localTransform.ValueRO.Position,
                    findTarget.ValueRO.Range,
                    ref distanceHits,
                    new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << Assets.UNITS_LAYER,
                        GroupIndex = 0
                    }
                ))
            {
                foreach (DistanceHit distanceHit in distanceHits)
                {
                    if (SystemAPI.Exists(distanceHit.Entity) ||
                        SystemAPI.HasComponent<LocalTransform>(distanceHit.Entity))
                    {
                        Unit detectedUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);

                        if (detectedUnit.Team != unit.ValueRO.Team && detectedUnit.Team != Assets.Neutral)
                        {
                            // Valid Target
                            target.ValueRW.TargetEntity = distanceHit.Entity;
                        }
                    }
                }
            }
        }
        
    }
}
