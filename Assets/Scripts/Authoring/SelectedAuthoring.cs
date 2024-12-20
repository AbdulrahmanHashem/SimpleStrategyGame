using Unity.Entities;
using UnityEngine;

class SelectedAuthoring : MonoBehaviour
{
    public GameObject visualEntity;
    public float showScale;
    
    class Baker : Baker<SelectedAuthoring>
    {
        public override void Bake(SelectedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Selected
            {
                VisualEntity = GetEntity(authoring.visualEntity, TransformUsageFlags.Dynamic),
                ShowScale = authoring.showScale,
            });
            
            SetComponentEnabled<Selected>(entity, false);
        }
    }
}

public struct Selected : IComponentData, IEnableableComponent
{
    public Entity VisualEntity;
    public float ShowScale;
    
    public bool OnSelected;
    public bool OnDeselected;
}