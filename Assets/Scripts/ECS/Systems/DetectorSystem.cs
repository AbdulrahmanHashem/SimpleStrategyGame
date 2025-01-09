using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
                RefRW<Detector> detector,
                RefRW<Unit> unit,
                RefRW<Target> target,
                RefRO<ShootAttack> shootAttack
            )
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<Detector>,
                RefRW<Unit>,
                RefRW<Target>,
                RefRO<ShootAttack>>()
            )
        {
            detector.ValueRW.Time -= SystemAPI.Time.DeltaTime;

            if (detector.ValueRW.Time > 0)
                continue;

            detector.ValueRW.Time = detector.ValueRW.MaxTime;

            distanceHits.Clear();

            if (collisionWorld.OverlapSphere
                (
                    localTransform.ValueRO.Position,
                    detector.ValueRO.Range,
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
                    if (SystemAPI.Exists(distanceHit.Entity) &&
                        SystemAPI.HasComponent<LocalTransform>(distanceHit.Entity) &&
                        SystemAPI.HasComponent<Unit>(distanceHit.Entity))
                    {
                        Unit detectedUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);

                        if (detectedUnit.Team != unit.ValueRO.Team && detectedUnit.Team != Assets.Neutral)
                        {
                            RefRO<LocalTransform> distanceHitLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(distanceHit.Entity);

                            if (SystemAPI.Exists(target.ValueRW.TargetEntity) && SystemAPI.HasComponent<Unit>(target.ValueRW.TargetEntity))
                            {
                                RefRO<LocalTransform> targetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRW.TargetEntity);

                                if (math.distance(distanceHitLocalTransform.ValueRO.Position, localTransform.ValueRO.Position)
                                >= math.distance(targetLocalTransform.ValueRO.Position, localTransform.ValueRO.Position))
                                {
                                    continue;
                                }
                            }

                            target.ValueRW.TargetEntity = distanceHit.Entity;
                        }
                    }
                }
            }
        }
    }
}
