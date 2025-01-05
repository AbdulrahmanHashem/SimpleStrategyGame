using Unity.Entities;
using UnityEngine;

class SelectedVisualAuthoring : MonoBehaviour
{
    public GameObject visualEntity;
    public float showScale;
}

class Baker : Baker<SelectedVisualAuthoring>
{
    public override void Bake(SelectedVisualAuthoring visualAuthoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Selected
        {
            VisualEntity = GetEntity(visualAuthoring.visualEntity, TransformUsageFlags.Dynamic),
            ShowScale = visualAuthoring.showScale,
        });
        SetComponentEnabled<Selected>(entity, false);
    }
}

public struct Selected : IComponentData, IEnableableComponent
{
    public Entity VisualEntity;
    public float ShowScale;
    
    public bool OnSelected;
    public bool OnDeselected;
}