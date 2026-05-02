using Suity.Helpers;
using System;
using System.Drawing;

namespace Suity.Views.NodeGraph;

public static class BezierHitTestAdvanced
{
    /// <summary>
    /// Determines if a point is near the curve using recursive subdivision
    /// </summary>
    public static bool IsPointNearBezierRecursive(this LinkShape linkShape, PointF point,
        float tolerance = 10f, float flatness = 0.5f)
    {
        if (QuickReject(point, linkShape, tolerance))
            return false;

        return CheckSegment(point,
            linkShape.StartPos, linkShape.StartPosBezier,
            linkShape.EndPosBezier, linkShape.EndPos,
            tolerance, flatness);
    }

    /// <summary>
    /// Quick bounding box rejection test - conservative but 100% safe
    /// </summary>
    private static bool QuickReject(PointF point, LinkShape shape, float tolerance)
    {
        // 包含所有控制点的最小/最大坐标
        float minX = Math.Min(Math.Min(Math.Min(
            shape.StartPos.X, shape.EndPos.X),
            shape.StartPosBezier.X), shape.EndPosBezier.X) - tolerance;

        float maxX = Math.Max(Math.Max(Math.Max(
            shape.StartPos.X, shape.EndPos.X),
            shape.StartPosBezier.X), shape.EndPosBezier.X) + tolerance;

        float minY = Math.Min(Math.Min(Math.Min(
            shape.StartPos.Y, shape.EndPos.Y),
            shape.StartPosBezier.Y), shape.EndPosBezier.Y) - tolerance;

        float maxY = Math.Max(Math.Max(Math.Max(
            shape.StartPos.Y, shape.EndPos.Y),
            shape.StartPosBezier.Y), shape.EndPosBezier.Y) + tolerance;

        // 点在包围盒外则快速返回false
        return point.X < minX || point.X > maxX || point.Y < minY || point.Y > maxY;
    }

    private static bool CheckSegment(PointF pt, PointF p0, PointF p1, PointF p2, PointF p3,
        float tolerance, float flatness)
    {
        // If the curve is flat enough, approximate it as a line segment
        if (IsFlat(p0, p1, p2, p3, flatness))
        {
            return DistanceToSegment(pt, p0, p3) <= tolerance;
        }

        // de Casteljau algorithm subdivision
        PointF p01 = Lerp(p0, p1, 0.5f);
        PointF p12 = Lerp(p1, p2, 0.5f);
        PointF p23 = Lerp(p2, p3, 0.5f);
        PointF p012 = Lerp(p01, p12, 0.5f);
        PointF p123 = Lerp(p12, p23, 0.5f);
        PointF p0123 = Lerp(p012, p123, 0.5f);

        // Recursively check both segments
        return CheckSegment(pt, p0, p01, p012, p0123, tolerance, flatness) ||
               CheckSegment(pt, p0123, p123, p23, p3, tolerance, flatness);
    }

    private static bool IsFlat(PointF p0, PointF p1, PointF p2, PointF p3, float threshold)
    {
        // Maximum distance from control points to the line connecting endpoints
        return Math.Max(DistanceToSegment(p1, p0, p3),
                       DistanceToSegment(p2, p0, p3)) <= threshold;
    }

    private static PointF Lerp(PointF a, PointF b, float t) =>
        new PointF(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

    private static float DistanceToSegment(PointF pt, PointF a, PointF b)
    {
        float dx = b.X - a.X, dy = b.Y - a.Y;
        if (dx == 0 && dy == 0) // Degenerates to a point
            return (float)Math.Sqrt(Sqr(pt.X - a.X) + Sqr(pt.Y - a.Y));

        float t = ((pt.X - a.X) * dx + (pt.Y - a.Y) * dy) / (dx * dx + dy * dy);
        t = Mathf.Clamp(t, 0f, 1f);

        float projX = a.X + t * dx, projY = a.Y + t * dy;
        return (float)Math.Sqrt(Sqr(pt.X - projX) + Sqr(pt.Y - projY));
    }

    private static float Sqr(float v) => v * v;
}