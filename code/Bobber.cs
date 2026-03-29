using System.Numerics;

namespace FishingGame;

partial class PlayerActor : Singleton<PlayerActor>
{
    public struct Bobber
    {
        // todo: tune values
        const float startHeight = -0.5f; // must be less than zero
        const float gravity = 1f; // must be greater than zero
        // todo: consider varying horizontal velocity based on throwDistance too
        const float horizontalVelocity = 0.5f; // must be greater than zero

        readonly float initialVerticalVelocity;
        readonly Vector2 origin;
        readonly CardinalDirection direction;
        readonly int creationTick;
        readonly float collisionTimeDelta;
        readonly bool landingInWater;

        public Bobber(Vector2 origin, float throwDistance, CardinalDirection direction)
        {
            initialVerticalVelocity = -(gravity * throwDistance) / (2 * horizontalVelocity) - (horizontalVelocity * startHeight) / throwDistance;
            this.origin = origin;
            this.direction = direction;
            creationTick = Engine.CurrentTick;

            // todo: predict if will collide with hilly collision
            // cont. also predict if will land in water to be used in update
            // cont. if there is a collision, shorten collisionTimeDelta to the point of collision and mark landingInWater as false
            collisionTimeDelta = throwDistance / horizontalVelocity;
            landingInWater = true;
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

        public void Update()
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
