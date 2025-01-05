using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(ResetEventSystem))]
public partial struct SelectedVisualSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<Selected> selectable in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
        {
            if (selectable.ValueRW.OnDeselected)
            {
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selectable.ValueRO.VisualEntity);
                visualLocalTransform.ValueRW.Scale = 0f;
            }
            
            if (selectable.ValueRW.OnSelected)
            {
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selectable.ValueRO.VisualEntity);
                visualLocalTransform.ValueRW.Scale = selectable.ValueRO.ShowScale;
            }
        }
    }
}
