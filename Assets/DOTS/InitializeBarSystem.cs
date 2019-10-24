using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsConversion
{
    [UpdateAfter(typeof(InitializePointsSystem))]
    public class InitializeBarSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            EntityQuery findPoints = GetEntityQuery(typeof(DotsConversion.Point), typeof(PointInitialize));
            NativeArray<Entity> pointEntities = findPoints.ToEntityArray(Allocator.TempJob);

            System.Collections.Generic.List<DebrisGenerator> settingsList = new System.Collections.Generic.List<DebrisGenerator>();
            EntityManager.GetAllUniqueSharedComponentData(settingsList);
            // why the actual data is stored in the second index is beyond me, but it works... so :)
            DebrisGenerator settings = settingsList[1];

            EntityArchetype barArchetype = EntityManager.CreateArchetype(
                typeof(Bar),
                typeof(BarThickness),
                typeof(LocalToWorld),
                settings.UseRenderMesh ? typeof(RenderMesh) : typeof(DotsConversion.MeshRenderer));

            EntityArchetype pointArchetype = EntityManager.CreateArchetype(
                typeof(Point),
                typeof(PointInitialize));

            // Now go through the point list and connect adjacent points, forming "bars"
            for (int i = 0, c = pointEntities.Length; i < c; i++)
            {
                var a = EntityManager.GetComponentData<Point>(pointEntities[i]);

                for (int j = i + 1; j < c; j++)
                {
                    var b = EntityManager.GetComponentData<Point>(pointEntities[j]);

                    float length = math.distance(a.position, b.position);

                    if (length < 5f && length > .2f)
                    {
                        // for each point, create a connection to any point between .2f and 5f radius of self
                        Bar bar = new Bar()
                        {
                            barLength = length,
                            a = pointEntities[i],
                            b = pointEntities[j],
                            backupA = EntityManager.CreateEntity(pointArchetype),
                            backupB = EntityManager.CreateEntity(pointArchetype)
                        };

                        bar.a = pointEntities[i];
                        bar.b = pointEntities[j];

                        a.neighborCount++;
                        b.neighborCount++;

                        EntityManager.SetComponentData(pointEntities[i], a);
                        EntityManager.SetComponentData(pointEntities[j], b);

                        var barEntity = EntityManager.CreateEntity(barArchetype);

                        EntityManager.SetComponentData(barEntity, bar);
                        EntityManager.SetComponentData(barEntity, new BarThickness() { thickness = .4f });
                        EntityManager.SetComponentData(barEntity, new LocalToWorld());

                        if (settings.UseRenderMesh)
                        {
                            EntityManager.SetSharedComponentData(barEntity,
                                new RenderMesh() { mesh = settings.barMesh, material = settings.barMaterial });
                        }
                        else
                        {
                            EntityManager.SetSharedComponentData(barEntity,
                                new MeshRenderer() { mesh = settings.barMesh, material = settings.barMaterial });
                        }
                    }
                }
            }

            for (int i = 0; i < pointEntities.Length; i++)
            {
                Point p = EntityManager.GetComponentData<Point>(pointEntities[i]);

                if(p.neighborCount < 1)
                    EntityManager.DestroyEntity(pointEntities[i]);
            }

            EntityManager.RemoveComponent<PointInitialize>(findPoints);
            pointEntities.Dispose();
        }
    }
}
