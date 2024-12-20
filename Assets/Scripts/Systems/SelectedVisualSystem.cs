using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(RestEventSystem))]
public partial struct SelectedVisualSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<Selected> selectable in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
        {
            if (selectable.ValueRW.OnSelected)
            {
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selectable.ValueRO.VisualEntity);
                visualLocalTransform.ValueRW.Scale = selectable.ValueRO.ShowScale;
            }

            if (selectable.ValueRW.OnDeselected)
            {
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selectable.ValueRO.VisualEntity);
                visualLocalTransform.ValueRW.Scale = 0f;
            }
        }
        // // Build a query that includes all entities with Selected,
        // // ignoring their enableable state.
        // EntityQuery query = SystemAPI.QueryBuilder()
        //     .WithPresent<Selected>()
        //     .Build();
        //
        // // Get all entities that have a Selected component (both enabled and disabled).
        // using var entities = query.ToEntityArray(Allocator.Temp);
        //
        // // Loop through all entities and check if Selected is enabled.
        // foreach (var entity in entities)
        // {
        //     bool isEnabled = SystemAPI.IsComponentEnabled<Selected>(entity);
        //     
        //     if (isEnabled)
        //     {
        //         // If enabled, retrieve the Selected component data.
        //         Selected selected = SystemAPI.GetComponent<Selected>(entity);
        //
        //         // Now you can safely get and modify any related components as usual.
        //         RefRW<LocalTransform> visualLocalTransform = 
        //             SystemAPI.GetComponentRW<LocalTransform>(selected.VisualEntity);
        //
        //         visualLocalTransform.ValueRW.Scale = selected.ShowScale;
        //     }
        //     else
        //     {
        //         // If enabled, retrieve the Selected component data.
        //         Selected selected = SystemAPI.GetComponent<Selected>(entity);
        //
        //         // Now you can safely get and modify any related components as usual.
        //         RefRW<LocalTransform> visualLocalTransform = 
        //             SystemAPI.GetComponentRW<LocalTransform>(selected.VisualEntity);
        //
        //         visualLocalTransform.ValueRW.Scale = 0f;
        //     }
        // }
    }
}
