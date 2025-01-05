using Unity.Entities;
using UnityEngine;

class DetectorAuthoring : MonoBehaviour
{
    public float range;
    public float maxTime;
}

class AggressionAuthoringBaker : Baker<DetectorAuthoring>
{
    public override void Bake(DetectorAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Detector
        {
            Range = authoring.range,
            MaxTime = authoring.maxTime,
        });
    }
}

public struct Detector : IComponentData, IEnableableComponent
{
    public float Range;
    public float Time;
    public float MaxTime;
}