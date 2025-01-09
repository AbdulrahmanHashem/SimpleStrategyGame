using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

class ShootAttackAuthoring : MonoBehaviour
{
    public float timerMax;
    
    public float attackDistance;
    // public int damage;
    
    public Transform bulletStartPositionTransform;
}

class ShootAttackAuthoringBaker : Baker<ShootAttackAuthoring>
{
    public override void Bake(ShootAttackAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new ShootAttack
        {
            TimerMax = authoring.timerMax,
            // Damage = authoring.damage,
            AttackDistance = authoring.attackDistance,
            BulletStartPosition = authoring.bulletStartPositionTransform.localPosition,
        });        
    }
}

public struct ShootAttack : IComponentData
{
    public float Time;
    public float TimerMax;
    
    public float AttackDistance;
    // public int Damage;
    
    public float3 BulletStartPosition;
}