using Unity.Burst;
using Unity.Entities;
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
