using System.Numerics;

namespace FishingGame;

partial class PlayerActor : Singleton<PlayerActor>
{
    public readonly struct BobberProjectile
    {
        // todo: tune constants
        const float startHeight = -3f; // must be less than zero
        const float gravity = 48f; // must be greater than zero
        // todo: consider varying horizontal velocity based on throwDistance too
        const float horizontalVelocity = 24f; // must be greater than zero

        readonly float initialVerticalVelocity;
        readonly Point origin;
        readonly CardinalDirection direction;
        readonly int creationTick;
        readonly float landingTimeDelta;
        readonly float collisionTimeDelta;
        readonly bool landingInWater;

        public BobberProjectile(Point origin, int throwDistance, CardinalDirection direction)
        {
            // maths based on rearranging the equation:
            // g is gravity, d is throwDistance, s is startHeight, i is initialVerticalVelocity, h is horizontal velocity
            // (g(d/h)^2)/2 + i(d/h) + s = 0
            // which produces this: i = -gd/2h - hs/d
            initialVerticalVelocity = -(gravity * throwDistance) / (2 * horizontalVelocity) - (horizontalVelocity * startHeight) / throwDistance;
            this.origin = origin;
            this.direction = direction;
            creationTick = Engine.CurrentTick;

            bool horizontal = direction.IsHorizontal();
            int directionSign = direction.Sign();
            Point originTile = Point.FloorToPoint(origin / (Vector2)Utilities.TileSize);

            // on moving axis
            int tileSize = horizontal ? Utilities.TileSize.width : Utilities.TileSize.height;
            int startingPos = horizontal ? origin.x : origin.y;
            int startingTile = (int)MathF.Floor(startingPos / (float)tileSize);
            int finalPos = startingPos + (directionSign * throwDistance);
            int finalTile = (int)MathF.Floor(finalPos / (float)tileSize);

            int fullTileOffset = Math.Abs(finalTile - startingTile);

            landingTimeDelta = throwDistance / horizontalVelocity;
            collisionTimeDelta = landingTimeDelta;
            landingInWater = Engine.PointToCollision(horizontal? finalTile : originTile.x, horizontal ? originTile.y : finalTile) == CollisionType.Wet;

            // todo: widen the bobber collider to be 2 pixels wide (rather than just a point)
            for (int tileOffset = 0; tileOffset <= fullTileOffset; tileOffset++)
            {
                int tile = startingTile + (directionSign * tileOffset);
                if (Engine.PointToCollision(horizontal ? tile : originTile.x, horizontal ? originTile.y : tile) == CollisionType.Hilly)
                {
                    int intersectionPoint = (tile + (direction.IsPositive() ? 0 : 1)) * tileSize;
                    collisionTimeDelta = MathF.Abs(intersectionPoint - startingPos) / horizontalVelocity;

                    // check to see if the point before the hit point was water, if so, then water is allowed
                    tile -= directionSign;
                    landingInWater = Engine.PointToCollision(horizontal ? tile : originTile.x, horizontal ? originTile.y : tile) == CollisionType.Wet;
                    break;
                }
            }
        }

        float CalculateHeight(float timePassed)
        {
            return (gravity * timePassed * timePassed * 0.5f) + (initialVerticalVelocity * timePassed) + startHeight;
        }

        // time is floating point for interpolation
        public Vector2 GetPosition(float currentTick)
        {
            float timePassed = (currentTick - creationTick) * Engine.FixedUpdateIntervalF;
            float heightTimePassed = MathF.Min(timePassed, landingTimeDelta); // freezes height interpolation at landing point
            float horizontalTimePassed = MathF.Min(timePassed, collisionTimeDelta); // freezes horizontal interpolation at collision point
            return (direction.ToVector2() * horizontalTimePassed * horizontalVelocity) +
                new Vector2(0f, CalculateHeight(heightTimePassed)) + origin;
        }

        public bool FixedUpdate()
        {
            float timePassed = (Engine.CurrentTick - creationTick) * Engine.FixedUpdateIntervalF;
            if (timePassed >= landingTimeDelta)
            {
                if (landingInWater)
                {
                    return true;
                }
                else
                {
                    // todo: withdraw bobber
                }
            }

            return false;
        }
    }
}
