using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJop jop = new UnitMoverJop
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        jop.ScheduleParallel();

        // foreach ((
        //          RefRW<LocalTransform> localTransform, 
        //          RefRO<UnitMover> unitMover,
        //          RefRW<PhysicsVelocity> physicsVelocity) 
        //          in SystemAPI.Query<
        //              RefRW<LocalTransform>, 
        //              RefRO<UnitMover>,
        //              RefRW<PhysicsVelocity>>())
        // {
        //     float3 moveDirection = unitMover.ValueRO.TargetPosition - localTransform.ValueRO.Position;
        //     moveDirection = math.normalize(moveDirection);
        //     
        //     localTransform.ValueRW.Rotation = 
        //         math.slerp(
        //             localTransform.ValueRO.Rotation, 
        //             quaternion.LookRotation(moveDirection, math.up()), 
        //             SystemAPI.Time.DeltaTime * unitMover.ValueRO.RotateSpeed);
        //     
        //     physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.MoveSpeed;
        //     physicsVelocity.ValueRW.Angular = float3.zero;
        // }
    }
}

[BurstCompile]
public partial struct UnitMoverJop : IJobEntity
{
    public float DeltaTime;
    
    public void Execute(
        ref LocalTransform localTransform, 
        in UnitMover unitMover, 
        ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.TargetPosition - localTransform.Position;

        float reachedTargetDistance = .2f;
        if (math.lengthsq(moveDirection) < reachedTargetDistance)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }
        
        
        moveDirection = math.normalize(moveDirection);
            
        localTransform.Rotation = 
            math.slerp(
                localTransform.Rotation, 
                quaternion.LookRotation(moveDirection, math.up()), 
                DeltaTime * unitMover.RotateSpeed);
            
        physicsVelocity.Linear = moveDirection * unitMover.MoveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}
