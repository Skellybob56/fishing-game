using Raylib_cs;
using System.Numerics;

namespace FishingGame;

public enum CollisionNormal : byte
{ Up, Down, Left, Right }

public readonly struct AABBHit(float timeTillCollision, float tEdge, Vector2 intersectionPoint, CollisionNormal collisionNormal)
{
    // timeTillCollision is measured assuming that the displacement takes 1 unit of time
    public readonly float timeTillCollision = timeTillCollision;
    public readonly float tEdge = tEdge; // the t value of the intersection on the hit edge from negative towards positive
    public readonly Vector2 intersectionPoint = intersectionPoint;
    public readonly CollisionNormal collisionNormal = collisionNormal;
}

public static class CollisionUtil
{
    public static AABBHit? SweepBoxAgainstBox(Rectangle dynamicBox, Vector2 displacement, Rectangle staticBox)
    {
        return LineBoxIntersection(dynamicBox.Position, displacement,
            new(staticBox.Position - dynamicBox.Size, staticBox.Size + dynamicBox.Size));
    }

    // if the point is inside the box, collisions are not counted
    static AABBHit? LineBoxIntersection(Vector2 point, Vector2 displacement, Rectangle box)
    {
        float tMinimum = 0f;
        CollisionNormal? collisionNormal = null;

        Vector2 boxMin = box.Position;
        Vector2 boxMax = box.Position + box.Size;

        // x slab
        if (displacement.X != 0f)
        {
            // find t values for crossing left and right x lines of box
            float tXLeft = (boxMin.X - point.X) / displacement.X;
            float tXRight = (boxMax.X - point.X) / displacement.X;

            // tXLeft and tXRight are behind the minimum entry point
            if (tXLeft < tMinimum && tXRight < tMinimum) { return null; }

            float tXMinimum = MathF.Min(tXLeft, tXRight);

            if (tXMinimum >= tMinimum)
            {
                collisionNormal = displacement.X > 0 ? CollisionNormal.Left : CollisionNormal.Right;
                tMinimum = tXMinimum;
            }
        }
        // if there is no horizontal movment and the point is not in the same x range of the box already then return direction miss
        else if (point.X <= boxMin.X || point.X >= boxMax.X)
        { return null; }

        // y slab
        if (displacement.Y != 0f)
        {
            // find t values for crossing top and bottom y lines of box
            float tYTop = (boxMin.Y - point.Y) / displacement.Y;
            float tYBottom = (boxMax.Y - point.Y) / displacement.Y;

            // tYTop and tYBottom are behind the minimum entry point
            if (tYTop < tMinimum && tYBottom < tMinimum) { return null; }

            float tYMinimum = MathF.Min(tYTop, tYBottom);

            if (tYMinimum >= tMinimum)
            {
                collisionNormal = displacement.Y > 0 ? CollisionNormal.Up : CollisionNormal.Down;
                tMinimum = tYMinimum;
            }
        }
        // if there is no vertical movment and the point is not in the same y range of the box already then return direction miss
        else if (point.Y <= boxMin.Y || point.Y >= boxMax.Y)
        { return null; }

        // collision occurs beyond the bounds of the displacement vector
        if (tMinimum >= 1) { return null; }

        if (collisionNormal.HasValue)
        {
            Vector2 intersectionPoint = point + displacement * tMinimum;
            float tEdge;
            if (collisionNormal.Value == CollisionNormal.Up || collisionNormal.Value == CollisionNormal.Down)
            {
                intersectionPoint.Y = collisionNormal.Value == CollisionNormal.Up ? boxMin.Y : boxMax.Y;
                tEdge = (intersectionPoint.X - boxMin.X) / box.Width;
            }
            else if (collisionNormal.Value == CollisionNormal.Left || collisionNormal.Value == CollisionNormal.Right)
            {
                intersectionPoint.X = collisionNormal.Value == CollisionNormal.Left ? boxMin.X : boxMax.X;
                tEdge = (intersectionPoint.Y - boxMin.Y) / box.Height;
            }
            else { throw new ArgumentOutOfRangeException(nameof(collisionNormal), "CollisionNormal variables must be Up, Down, Left or Right"); }

            return new(tMinimum, tEdge, intersectionPoint, collisionNormal.Value);
        }

        return null;
    }

    public static Vector2 ApplyNormal(this Vector2 direction, CollisionNormal collisionNormal)
    {
        if (collisionNormal == CollisionNormal.Up)
        { return direction.Y <= 0f ? direction : new(direction.X, 0); }
        else if (collisionNormal == CollisionNormal.Down)
        { return direction.Y >= 0f ? direction : new(direction.X, 0); }
        else if (collisionNormal == CollisionNormal.Left)
        { return direction.X <= 0f ? direction : new(0, direction.Y); }
        else if (collisionNormal == CollisionNormal.Right)
        { return direction.X >= 0f ? direction : new(0, direction.Y); }
        else { throw new ArgumentOutOfRangeException(nameof(collisionNormal), "CollisionNormal variables must be Up, Down, Left or Right"); }
    }
}
