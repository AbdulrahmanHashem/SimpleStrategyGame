using Unity.Entities;
using UnityEngine;

class NewBakerScript : MonoBehaviour
{
    public GameObject bulletPrefabGameObject;
}

class NewBakerScriptBaker : Baker<NewBakerScript>
{
    public override void Bake(NewBakerScript authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new EntitiesReferences
        {
            BulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic)
        });
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity BulletPrefabEntity;
}
