using System;
using DotsConversion;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditorInternal;
using UnityEngine;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

public class BarTornadoSway : ComponentSystem
{
    float tornadoFader = 0f;
    [Range(0f, 1f)] public float damping= 0.012f;

    public float friction = 0.4f;

    public EntityQuery m_Group;

    public float3 tornadoPosition;

    public float tornadoForce = 0.022f;
    public float tornadoMaxForceDist = 30.0f;
    public float tornadoHeight = 50.0f;
    public float tornadoUpForce = 1.4f;
    public float tornadoInwardForce= 9.0f;

    protected override void OnCreate()
    {
        var query = new EntityQueryDesc
        {
            None = new ComponentType[] {typeof(DotsConversion.BarAnchor)},
            All = new ComponentType[] {typeof(DotsConversion.Bar)}
        };
        m_Group = GetEntityQuery(query);
    }

    void SwayABar(ref DotsConversion.Bar b)
    {
        SwayAPoint(ref b.a);
        SwayAPoint(ref b.b);
 
            DotsConversion.Point point1 = b.a;
            DotsConversion.Point point2 = b.b;

            float dx = point2.position.x - point1.position.x;
            float dy = point2.position.y - point1.position.y;
            float dz = point2.position.z - point1.position.z;

            // Calculate how much distance has been added due to the tornado force affecting points
            float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
            float extraDist = dist - b.a.barLength;

            // When a point is affected by the suction, move it in the direction of the suck by half of the distance traveled
            float pushX = (dx / dist * extraDist) * .5f;
            float pushY = (dy / dist * extraDist) * .5f;
            float pushZ = (dz / dist * extraDist) * .5f;

           {
                point1.position.x += pushX;
                point1.position.y += pushY;
                point1.position.z += pushZ;
                point2.position.x -= pushX;
                point2.position.y -= pushY;
                point2.position.z -= pushZ;
            }
//            else if (point1.anchor)
//            {
//                point2.position.x -= pushX * 2f;
//                point2.position.y -= pushY * 2f;
//                point2.position.z -= pushZ * 2f;
//            }
//            else if (point2.anchor)
//            {
//                point1.position.x += pushX * 2f;
//                point1.position.y += pushY * 2f;
//                point1.position.z += pushZ * 2f;
//            }


    }

    void SwayAPoint(ref DotsConversion.Point point)
    {
       // Debug.Log( "Time.deltaTime :" + Time.deltaTime);
        tornadoFader = UnityEngine.Mathf.Clamp01(tornadoFader + Time.deltaTime / 10f);

        float invDamping = 1f - damping;

        float startX = point.position.x;
        float startY = point.position.y;
        float startZ = point.position.z;

        point.previous.y += .01f;

        // Calculate the tornado force on this point
        float tdx = tornadoPosition.x + PointManager.TornadoSway(point.position.y) - point.position.x;
        float tdz = tornadoPosition.y - point.position.z;
        float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
        tdx /= tornadoDist;
        tdz /= tornadoDist;

        // If the point is within max force distance of the tornado, apply force to this point
        if (tornadoDist < tornadoMaxForceDist)
        {
            float force = (1f - tornadoDist / tornadoMaxForceDist);
            float yFader = Mathf.Clamp01(1f - point.position.y / tornadoHeight);
            // apply greater force at the base of the tornado, tapering as we ascend the Y axis
            force *= tornadoFader * tornadoForce * Random.Range(-.3f, 1.3f);
            float forceY = tornadoUpForce;
            point.previous.y -= forceY * force;
            float forceX = -tdz + tdx * tornadoInwardForce * yFader;
            float forceZ = tdx + tdz * tornadoInwardForce * yFader;
            point.previous.x -= forceX * force;
            point.previous.z -= forceZ * force;
        }

        // dampen the effect of the tornado force (if applied)
        point.position.x += (point.position.x - point.previous.x) * invDamping;
        point.position.y += (point.position.y - point.previous.y) * invDamping;
        point.position.z += (point.position.z - point.previous.z) * invDamping;

        point.previous.x = startX;
        point.previous.y = startY;
        point.previous.z = startZ;

        if (point.position.y < 0f)
        {
            point.position.y = 0f;
            point.previous.y = -point.previous.y;
            point.previous.x += (point.position.x - point.previous.x) * friction;
            point.previous.z += (point.position.z - point.previous.z) * friction;
        }
        
        
        
    }

    protected override void OnUpdate()
    {
        Entities.WithAll<Tornado, Translation>().ForEach((Entity entity, ref Translation t)
            =>
        {
            tornadoPosition = t.Value;
        });

        Entities.With(m_Group).ForEach<DotsConversion.Bar>(SwayABar);
    }
}
