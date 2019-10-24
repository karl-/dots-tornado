using DotsConversion;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

public class InitializePointsSystem : ComponentSystem
{
    static DotsConversion.Point CreatePoint(float3 position, bool anchor)
    {
        return new DotsConversion.Point()
        {
            position = position,
            previous = position,
            anchor = anchor,
            neighborCount = 0
        };
    }

    protected override void OnUpdate()
    {
        EntityQuery findPoints = GetEntityQuery(typeof(DotsConversion.Point), typeof(PointInitialize));
        NativeArray<Entity> pointEntities = findPoints.ToEntityArray(Allocator.TempJob);

        int index = 0;

        if (pointEntities.Length < 1)
        {
            pointEntities.Dispose();
            return;
        }

        System.Collections.Generic.List<DebrisGenerator> settingsList = new System.Collections.Generic.List<DebrisGenerator>();
        EntityManager.GetAllUniqueSharedComponentData(settingsList);
        // why the actual data is stored in the second index is beyond me, but it works... so :)
        DebrisGenerator settings = settingsList[1];
        Random random = new Random(42);

        // Initialize points
        for (int i = 0; i < settings.BuildingCount; i++)
        {
            int buildingHeight = random.NextInt(settings.BuildingMinHeight, settings.BuildingMaxHeight);
            float spacing = 2f;
            float3 pos = new float3(random.NextFloat(-45f, 45f), 0f, random.NextFloat(-45f, 45f));

            for (int n = 0; n < buildingHeight; n++)
            {
                // Buildings are composed of sets of 3 points forming a triangle. The first floor is marked as the anchor
                DotsConversion.Point a = CreatePoint(new float3(pos.x + spacing, n * spacing, pos.z - spacing), n == 0);
                DotsConversion.Point b = CreatePoint(new float3(pos.x - spacing, n * spacing, pos.z - spacing), n == 0);
                DotsConversion.Point c = CreatePoint(new float3(pos.x + 0f, n * spacing, pos.z + spacing), n == 0);

                EntityManager.SetComponentData(pointEntities[index++], a);
                EntityManager.SetComponentData(pointEntities[index++], b);
                EntityManager.SetComponentData(pointEntities[index++], c);
            }
        }

        for (int i = 0; i < settings.AdditionalPointCount / 2; i++)
        {
            float3 pos = new float3(random.NextFloat(-55f, 55f), 0f, random.NextFloat(-55f, 55f));

            var a = CreatePoint(pos + new float3(random.NextFloat(-.2f, -.1f), random.NextFloat(0f, 3f), random.NextFloat(.1f, .2f)), false);
            var b = CreatePoint(float3(random.NextFloat(.2f, .1f), random.NextFloat(0f, .2f), random.NextFloat(-.1f, -.2f)), random.NextFloat() < .1f);

            EntityManager.SetComponentData(pointEntities[index++], a);
            EntityManager.SetComponentData(pointEntities[index++], b);
        }

        pointEntities.Dispose();
    }
}
