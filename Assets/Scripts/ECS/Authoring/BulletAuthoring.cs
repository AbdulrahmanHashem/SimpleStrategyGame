using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

class BulletAuthoring : MonoBehaviour
{
    public float speed;
    public int damageAmount;
    public int maxRange;
    public float explosionRadius;

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
            MaxRange = authoring.maxRange,
            ExplosionRadius = authoring.explosionRadius
        });
    }
}

public struct Bullet : IComponentData
{
    public float Speed;
    public int DamageAmount;
    public int MaxRange;
    public float ExplosionRadius;
    public LocalTransform ShootPosition;
    public float3 Direction;

    public Entity Shooter;
}
