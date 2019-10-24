using DotsConversion;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(ApplyTornadoSwayToPointsSystem))]
public class ApplyBarTornadoSwayToBarSystem : ComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    //[BurstCompile]
    public ComponentDataFromEntity<DotsConversion.Point> _points;
    public ComponentDataFromEntity<DotsConversion.Bar> _bar;
    public EntityQuery m_Group;

    void ApplyBar(ref DotsConversion.Bar bar)
    {
        if (bar.a == Entity.Null)
            return;
        DotsConversion.Point pA = _points[bar.a];
        DotsConversion.Point pB = _points[bar.b];
        float3 pointA = pA.position;
        float3 pointB = pB.position;
        float dx = pointB.x - pointA.x;
        float dy = pointB.y - pointA.y;
        float dz = pointB.z - pointA.z;
        // Calculate how much distance has been added due to the tornado force affecting points
        float dist = math.sqrt(dx * dx + dy * dy + dz * dz);
        float extraDist = dist - bar.barLength;
        // When a point is affected by the suction, move it in the direction of the suck by half of the distance traveled
        float pushX = (dx / dist * extraDist) * .5f;
        float pushY = (dy / dist * extraDist) * .5f;
        float pushZ = (dz / dist * extraDist) * .5f;
       // Debug.Log( "Push " + pushX + " " + pushY + " " + pushZ );
        if (_points[bar.a].anchor == false && _points[bar.b].anchor == false)
        {
            pointA.x += pushX;
            pointA.y += pushY;
            pointA.z += pushZ;
            pointB.x -= pushX;
            pointB.y -= pushY;
            pointB.z -= pushZ;
        }
        else if (_points[bar.a].anchor)
        {
            pointB.x -= pushX * 2f;
            pointB.y -= pushY * 2f;
            pointB.z -= pushZ * 2f;
        }
        else if (_points[bar.b].anchor)
        {
            pointA.x += pushX * 2f;
            pointA.y += pushY * 2f;
            pointA.z += pushZ * 2f;
        }

        pA.position = pointA;
        pB.position = pointB;
        _points[bar.a] = pA;
        _points[bar.b] = pB;
        bar.extraDist = extraDist;
    }

    protected override void OnCreate()
    {
        var query = new EntityQueryDesc
        {
            All = new ComponentType[] {typeof(DotsConversion.Bar)}
        };
        m_Group = GetEntityQuery(query);
    }
    
    //[UpdateAfter(ApplyTornadoSwayToPointsSystem)]
    protected override void OnUpdate()
    {
       // World.GetExistingSystem(typeof(ApplyTornadoSwayToPointsSystem)).RequireForUpdate();
        _points = GetComponentDataFromEntity<DotsConversion.Point>(false);
        Entities.With(m_Group).ForEach<DotsConversion.Bar>(ApplyBar);

    }
}
