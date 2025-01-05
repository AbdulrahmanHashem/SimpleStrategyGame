using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    public int team;
    public int owner;
}

public class UnitAuthoringBaker : Baker<UnitAuthoring>
{
    public override void Bake(UnitAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        
        AddComponent(entity, new Unit
        {
            Team = authoring.team,
            Owner = authoring.owner,
        });
    }
}

public struct Unit : IComponentData
{
    public int Team;
    public int Owner;
}