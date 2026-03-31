using System.Numerics;

namespace FishingGame;

partial class PlayerActor : Singleton<PlayerActor>
{
    public struct Bobber
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
        readonly float collisionTimeDelta;
        readonly bool landingInWater;

        public Bobber(Vector2 origin, int throwDistance, CardinalDirection direction)
        {
            // maths based on rearranging the equation:
            // g is gravity, d is throwDistance, s is startHeight, i is initialVerticalVelocity, h is horizontal velocity
            // (g(d/h)^2)/2 + i(d/h) + s = 0
            // which produces this: i = -gd/2h - hs/d
            initialVerticalVelocity = -(gravity * throwDistance) / (2 * horizontalVelocity) - (horizontalVelocity * startHeight) / throwDistance;
            this.origin = Point.RoundToPoint(origin);
            this.direction = direction;
            creationTick = Engine.CurrentTick;

            // todo: predict if will collide with hilly collision
            // cont. also predict if will land in water to be used in update
            // cont. if there is a collision, shorten collisionTimeDelta to the point of collision and mark landingInWater as false

            bool horizontal = direction.IsHorizontal();
            Point originTile = Point.FloorToPoint(origin / (Vector2)Utilities.TileSize);
            int finalTile = (int)MathF.Floor((horizontal ? origin.X : origin.Y) + (direction.Sign() * throwDistance)); // final tile position on moving axis

            collisionTimeDelta = throwDistance / horizontalVelocity;
            landingInWater = Engine.PointToCollision(horizontal? new(finalTile, originTile.y) : new(originTile.x, finalTile)) == CollisionType.Wet;

            for (int tile = horizontal ? originTile.x : originTile.y; tile <= finalTile; tile++)
            {
                if (Engine.PointToCollision(horizontal ? new(tile, originTile.y) : new(originTile.x, tile)) == CollisionType.Hilly)
                {
                    // todo: set collisionTimeDelta
                    landingInWater = false;
                    break;
                }
            }
        }

        public bool InWater { get; private set; } = false;

        readonly float CalculateHeight(float timePassed)
        {
            return (gravity * timePassed * timePassed * 0.5f) + (initialVerticalVelocity * timePassed) + startHeight;
        }

        // time is floating point for interpolation
        public readonly Vector2 GetPosition(float currentTick)
        {
            float timePassed = (currentTick - creationTick) * Engine.FixedUpdateIntervalF;
            timePassed = MathF.Min(timePassed, collisionTimeDelta); // freezes interpolation at collision point
            return (direction.ToVector2() * timePassed * horizontalVelocity) +
                new Vector2(0f, CalculateHeight(timePassed)) + origin;
        }

        public void FixedUpdate()
        {
            if (InWater) { return; }
            float timePassed = (Engine.CurrentTick - creationTick) * Engine.FixedUpdateIntervalF;
            if (timePassed >= collisionTimeDelta) // note: collision can be with the ground or with a wall
            {
                if (landingInWater)
                {
                    InWater = true;
                }
                else
                {
                    // todo: withdraw bobber
                }
            }
        }
    }
}
