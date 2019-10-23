using DotsConversion;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UR = UnityEngine.Random;

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
        NativeArray<DotsConversion.Point> points = findPoints.ToComponentDataArray<DotsConversion.Point>(Allocator.TempJob);
        DebrisGenerator settings = GetSingleton<DebrisGenerator>();

        int index = 0;

        if (points.Length < 1)
        {
            points.Dispose();
            return;
        }

        for (int i = 0; i < settings.BuildingCount; i += 3)
        {
            int buildingHeight = UR.Range(settings.BuildingMinHeight, settings.BuildingMaxHeight);
            float spacing = 2f;
            float3 pos = new float3(UR.Range(-45f, 45f), 0f, UR.Range(-45f, 45f));

            for (int n = 0; n < buildingHeight; n++)
            {
                // Buildings are composed of sets of 3 points forming a triangle. The first floor is marked as the anchor
                points[index++] = CreatePoint(new float3(pos.x + spacing, n * spacing, pos.z - spacing), n == 0);
                points[index++] = CreatePoint(new float3(pos.x - spacing, n * spacing, pos.z - spacing), n == 0);
                points[index++] = CreatePoint(new float3(pos.x + 0f, n * spacing, pos.z + spacing), n == 0);
            }
        }

        for (int i = index, c = points.Length; i < c; i += 2)
        {
            float3 pos = new float3(UR.Range(-55f, 55f), 0f, UR.Range(-55f, 55f));
            CreatePoint(pos + new float3(UR.Range(-.2f, -.1f), UR.Range(0f, 3f), UR.Range(.1f, .2f)), false);
            CreatePoint(float3(UR.Range(.2f, .1f), UR.Range(0f, .2f), UR.Range(-.1f, -.2f)), UR.value < .1f);
        }

        EntityManager.RemoveComponent<PointInitialize>(findPoints);

        points.Dispose();
    }
}
