using Unity.Entities;
using Unity.Mathematics;

namespace DotsConversion
{
    public sealed class BarBreakSystem : ComponentSystem
    {
        float m_BreakDistance;
        ComponentDataFromEntity<Point> m_Points;
        EntityArchetype m_PointArchetype;
        EntityQueryBuilder.F_D<Bar> m_CalculateBreak;

        protected override void OnCreate()
        {
            m_PointArchetype = EntityManager.CreateArchetype(typeof(Point));
            m_CalculateBreak = CalculateBreak;
        }

        protected override void OnUpdate()
        {
            m_BreakDistance = GetSingleton<BarSettings>().BreakDistance;
            m_Points = GetComponentDataFromEntity<Point>();
            Entities.ForEach(m_CalculateBreak);
        }

        void CalculateBreak(ref Bar bar)
        {
            if (bar.a == Entity.Null)
                return;

            if (math.abs(bar.extraDist) > m_BreakDistance)
            {
                Point pointB = m_Points[bar.b];
                if (pointB.neighborCount > 1)
                {
                    pointB.neighborCount--;
                    m_Points[bar.b] = pointB;
                    Entity newPoint = EntityManager.CreateEntity(m_PointArchetype);
                    Point copyPointB = pointB;
                    copyPointB.neighborCount = 1;
                    EntityManager.SetComponentData(newPoint, copyPointB);
                    bar.b = newPoint;
                    return;
                }

                Point pointA = m_Points[bar.a];
                if (pointA.neighborCount > 1)
                {
                    pointA.neighborCount--;
                    m_Points[bar.a] = pointA;
                    Entity newPoint = EntityManager.CreateEntity(m_PointArchetype);
                    Point copyPointA = pointA;
                    copyPointA.neighborCount = 1;
                    EntityManager.SetComponentData(newPoint, copyPointA);
                    bar.a = newPoint;
                }
            }
        }
    }
}
