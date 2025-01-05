using Unity.Entities;
using UnityEngine;

class BulletAuthoring : MonoBehaviour
{
    public float speed;
    public int damageAmount;
}

class BulletAuthoringBaker : Baker<BulletAuthoring>
{
    public override void Bake(BulletAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Bullet
        {
            Speed = authoring.speed,
            DamageAmount = authoring.damageAmount,
        });
    }
}

public struct Bullet : IComponentData
{
    public float Speed;
    public int DamageAmount;
}
