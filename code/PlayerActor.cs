using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using static FishingGame.Utilities;

namespace FishingGame;

partial class PlayerActor : Singleton<PlayerActor>
{
    enum AccelerationMode : byte
    {
        Static,
        Accelerating,
        Decelerating,
        CounterAccelerating
    }

    [Flags]
    enum NudgeFlags : byte
    {
        NoNudge = 0,
        XNudge = 1,
        YNudge = 2
    }

    public static PlayerActor Create(Vector2 position)
    { return Register(new PlayerActor(position)); }

    // consts
    const float topSpeed = 1f;
    const float acceleration = 0.2f;
    const float deceleration = 0.3f;
    const float counterAcceleration = 0.4f;
    const float rolloverSpeed = 0.2f;
    const float collisionVelocityCost = 0.2f;
    const float edgeBevelDepth = 0.4f;
    const float maxNudgePortion = 0.1f; // maximum distance of nudge (per frame) caused by edge bevel measured in portion of bevel length

    readonly NaturalRectangle collider = new(
        new Point(6, 13), // offset of top left corner from playerActor positon
        new NaturalSize(4, 2)
        );

    // fixed
    Vector2 position;
    Vector2 velocity = Vector2.Zero;
    Vector2 wishVelocity;
    Vector2 displacement;
    CardinalDirection facingDirection;

    (BobberProjectile? Projectile, BobberState State) bobber = (null, BobberState.Withdrawn); // null if fishing line not cast

    NudgeFlags nudgeFlags = NudgeFlags.NoNudge;

    // shared
    public Lock SharedDataLock { get; } = new();
    public Vector2 SharedPosition { get; private set; }
    public Vector2 SharedOldPosition { get; private set; }
    public CardinalDirection SharedFacingDirection { get; private set; }
    public (BobberProjectile? Projectile, BobberState State) SharedBobber { get; private set; }

    private PlayerActor(Vector2 position)
    {
        this.position = position;
        SharedPosition = position;
        SharedOldPosition = position;

        facingDirection = CardinalDirection.Down;
        SharedFacingDirection = facingDirection;
    }

    void Rollover()
    {
        static void PredictStoppingPositionAndTime(float position, float orthogonalWishVelocity, Vector2 velocity, int axis, out int ticksTillStatic, out float finalPosition)
        {
            Vector2 wishVelocity = Vector2.Zero;
            wishVelocity[axis == 0 ? 1 : 0] = orthogonalWishVelocity;

            finalPosition = position;

            ticksTillStatic = 0;
            while (velocity[axis] != 0) // wishVelocity[axis] must be equal to zero so 
            {
                ticksTillStatic++;
                finalPosition += velocity[axis];

                // todo: unify this code with the identical code in FixedUpdate()
                // < snippet from FixedUpdate() >
                AccelerationMode accelerationMode = GetAccelerationMode(wishVelocity, velocity);

                if (accelerationMode == AccelerationMode.Accelerating)
                {
                    velocity = velocity.MoveTowards(wishVelocity, acceleration);
                }
                else if (accelerationMode == AccelerationMode.Decelerating)
                {
                    velocity = velocity.MoveTowards(wishVelocity, deceleration);
                }
                else if (accelerationMode == AccelerationMode.CounterAccelerating)
                {
                    velocity = velocity.MoveTowards(wishVelocity, counterAcceleration);
                }
                // </snippet from FixedUpdate() >
            }
        }

        // todo: bug: rollover seems to overshoot and bounce-back frequently when simply moving on one axis and stopping.

        for (int axis = 0; axis < 2; axis++)
        {
            // return if not appropriate for rollover
            if (wishVelocity[axis] != 0 || (axis == 0 ? nudgeFlags.HasFlag(NudgeFlags.XNudge) : nudgeFlags.HasFlag(NudgeFlags.YNudge)))
            { continue; }

            if (velocity[axis] == 0) // static
            {
                float positionTarget = MathF.Round(position[axis]);

                // already on pixel grid
                if (positionTarget == position[axis]) { continue; }

                displacement[axis] += MovementTowards(position[axis], positionTarget, rolloverSpeed);
            }
            else // coast
            {
                // get predicted tick count till static and the final position
                PredictStoppingPositionAndTime(position[axis], wishVelocity[axis == 0 ? 1 : 0], velocity, axis,
                    out int ticksTillStatic, out float finalPosition);

                // manipulate data to get final position frame delta
                float finalPositionTarget = MathF.Round(finalPosition);
                float finalPositionDelta = finalPositionTarget - finalPosition;
                float finalPositionFrameDelta = finalPositionDelta / ticksTillStatic;

                displacement[axis] +=
                    MathF.Abs(finalPositionFrameDelta) > rolloverSpeed ?
                    rolloverSpeed * MathF.Sign(finalPositionFrameDelta) :
                    finalPositionFrameDelta;
            }
        }
    }

    static float? ApplySubtickDisplacementNudge(Vector2 unnudgedSubtickDisplacement, Vector2 wishVelocity, AABBHit hit, bool horizontalCollision, Point closestTileHit, NaturalSize colliderSize)
    {
        if (hit.TEdge < edgeBevelDepth || hit.TEdge > 1f - edgeBevelDepth)
        {
            int nudgeSign = hit.TEdge > 0.5f ? 1 : -1;
            int normalSign = hit.CollisionNormal == CardinalDirection.Left ||
                hit.CollisionNormal == CardinalDirection.Up ? -1 : 1;

            // if player is applying input towards the wall
            if ((horizontalCollision? wishVelocity.X : wishVelocity.Y) * normalSign < 0f)
            {
                Point firstSample = closestTileHit + // cardinal neighbour tile
                    (horizontalCollision ? new(0, nudgeSign) : new(nudgeSign, 0));
                Point secondSample = closestTileHit + // diagonal neighbour tile
                    (horizontalCollision ? new(normalSign, nudgeSign) : new(nudgeSign, normalSign));

                if (Engine.PointToCollision(firstSample) == CollisionType.Walkable &&
                    Engine.PointToCollision(secondSample) == CollisionType.Walkable)
                {
                    float tCorner = 0.5f - MathF.Abs(hit.TEdge - 0.5f);
                    // lerp nudge between zero when just barely on the bevel to the maximum when right at the edge
                    float bevelStrength = 1 - (tCorner / edgeBevelDepth);

                    // nudge axis calculations
                    float colliderLength = horizontalCollision ? colliderSize.Height : colliderSize.Width;
                    float tileSize = horizontalCollision ? TileSize.Height : TileSize.Width;
                    float edgePixelLength = tileSize + colliderLength;
                    float edgeStart = (horizontalCollision ? closestTileHit.Y : closestTileHit.X) * tileSize - colliderLength;

                    float cornerPixel = edgeStart + (nudgeSign > 0 ? edgePixelLength : 0f);
                    float intersectionPixel = (horizontalCollision ? hit.IntersectionPoint.Y : hit.IntersectionPoint.X) * tileSize;
                    float cornerDelta = cornerPixel - intersectionPixel;

                    float unnudgedSubtickDelta = horizontalCollision ? unnudgedSubtickDisplacement.Y : unnudgedSubtickDisplacement.X;
                    float displacedPredictedPosition = intersectionPixel + unnudgedSubtickDelta;

                    // ensure that current displacement doesn't already round the corner
                    if (displacedPredictedPosition * nudgeSign < cornerPixel * nudgeSign)
                    {
                        float bevelMaxDelta = maxNudgePortion * (bevelStrength * (edgePixelLength * edgeBevelDepth)); // todo: consider modulating nudge strength based on how strongly wishDir is pointing into the wall
                        // ensure nudge doesn't overshoot the corner
                        return unnudgedSubtickDelta.MoveTowards(cornerDelta, bevelMaxDelta);
                    }
                }
            }
        }
        return null;
    }

    void ApplyDisplacement()
    {
        nudgeFlags = NudgeFlags.NoNudge;

        if (displacement == Vector2.Zero) { return; }

        float remainingTime = 1f;
        Vector2 subtickDisplacement = displacement;

        (AABBHit closestAABBHit, Point closestTileHit)? closestHit;

        while (remainingTime > 0f) // while there is still time in the tick
        {
            float subtickTimeLength = remainingTime;

            // measure the tiles the player is colliding with excluding the displacement
            Rectangle currentCollider = new(
                (position + collider.Position) / TileSize,
                (Vector2)collider.Size / (Vector2)TileSize
                );
            NaturalRectangle currentCollidingTiles = NaturalRectangle.ExpansiveRound(currentCollider, false); // use open intervals as the playerActor is not on tiles they are touching

            // measure the tiles the player is colliding with including the displacement
            NaturalRectangle potentialCollidingTiles = NaturalRectangle.ExpansiveRound(
                currentCollider.GrowRectangle(subtickDisplacement / TileSize)
                );

            // if the potentialCollidingTiles set is equal to the currentCollidingTiles set then just add displacement and return
            if (currentCollidingTiles == potentialCollidingTiles)
            { position += subtickDisplacement; return; }

            closestHit = null;
            // iterate on each potentialCollidingTile
            for (int tileX = potentialCollidingTiles.Position.X; tileX < potentialCollidingTiles.Size.Width + potentialCollidingTiles.Position.X; tileX++)
            {
                for (int tileY = potentialCollidingTiles.Position.Y; tileY < potentialCollidingTiles.Size.Height + potentialCollidingTiles.Position.Y; tileY++)
                {
                    // if tile is walkable then continue
                    if (Engine.PointToCollision(tileX, tileY) == CollisionType.Walkable) { continue; }

                    // get collision data
                    AABBHit? aabbHit = CollisionUtil.SweepBoxAgainstBox(currentCollider, // todo: do sweep in pixel space to simplify maths everywhere
                        subtickDisplacement / TileSize, new(tileX, tileY, 1, 1));

                    if (aabbHit != null) // if there is a collision
                    {
                        // set subtick displacement to bring the player just up to the tile
                        // shortening subtick displacment ensures that subsequent hits are only registered if even closer
                        subtickDisplacement = (aabbHit.Value.IntersectionPoint - currentCollider.Position) * TileSize;
                        subtickTimeLength *= aabbHit.Value.TimeTillCollision;
                        closestHit = new(aabbHit.Value, new(tileX, tileY)); // store intersection data
                    }
                }
            }
            if (!closestHit.HasValue) // no intersection
            { position += subtickDisplacement; return; }

            AABBHit closestAABBHit = closestHit.Value.closestAABBHit;
            Point closestTileHit = closestHit.Value.closestTileHit;

            // reduce time by subtickTimeLength
            remainingTime -= subtickTimeLength;

            // apply normal to displacement
            displacement = displacement.ApplyNormal(closestAABBHit.CollisionNormal);

            // apply normal partially to velocity
            velocity = velocity.MoveTowards(velocity.ApplyNormal(closestAABBHit.CollisionNormal), collisionVelocityCost);

            // move to collision intersection
            position = closestAABBHit.IntersectionPoint * TileSize - collider.Position;

            // use the initial displacement to produce new displacement
            subtickDisplacement = displacement * remainingTime;

            // check and apply nudge to round corner if needed (this can only happen once per edge per frame as displacement into a tile is entirely stopped on collision)
            {
                bool horizontalCollision = closestAABBHit.CollisionNormal == CardinalDirection.Left || closestAABBHit.CollisionNormal == CardinalDirection.Right;
                float? nudge = ApplySubtickDisplacementNudge(subtickDisplacement, wishVelocity, closestAABBHit, horizontalCollision, closestTileHit, collider.Size);
                if (nudge.HasValue)
                {
                    if (horizontalCollision)
                    {
                        subtickDisplacement.Y = nudge.Value;
                        nudgeFlags |= NudgeFlags.YNudge;
                    }
                    else
                    {
                        subtickDisplacement.X = nudge.Value;
                        nudgeFlags |= NudgeFlags.XNudge;
                    }
                }
            }
        }
    }

    static AccelerationMode GetAccelerationMode(Vector2 wishVelocity, Vector2 velocity)
    {
        if (velocity == Vector2.Zero)
        { return wishVelocity == Vector2.Zero? AccelerationMode.Static : AccelerationMode.Accelerating; }

        if (wishVelocity == Vector2.Zero)
        { return AccelerationMode.Decelerating; }

        if (Vector2.Dot(velocity, wishVelocity) < 0)
        { return AccelerationMode.CounterAccelerating; }

        // rotational changes within one quarter of a rotation from velocity direction can benefit from 'deceleration' acceleration speeds if wishDir magnitude <= velocity magnitude
        return wishVelocity.LengthSquared() <= velocity.LengthSquared()? AccelerationMode.Decelerating : AccelerationMode.Accelerating;
    }

    Vector2 GetWishVelocity()
    {
        if (bobber.State != BobberState.Withdrawn) { return Vector2.Zero; }
        else { return Controller.WishDir * topSpeed; }
    }

    void UpdateVelocity()
    {
        AccelerationMode accelerationMode = GetAccelerationMode(wishVelocity, velocity);

        if (accelerationMode == AccelerationMode.Accelerating)
        {
            velocity = velocity.MoveTowards(wishVelocity, acceleration);
        }
        else if (accelerationMode == AccelerationMode.Decelerating)
        {
            velocity = velocity.MoveTowards(wishVelocity, deceleration);
        }
        else if (accelerationMode == AccelerationMode.CounterAccelerating)
        {
            velocity = velocity.MoveTowards(wishVelocity, counterAcceleration);
        }
    }

    void UpdateFacingDirection()
    {
        if (wishVelocity != Vector2.Zero)
        {
            if (MathF.Abs(wishVelocity.X) >= MathF.Abs(wishVelocity.Y))
            { facingDirection = wishVelocity.X > 0 ? CardinalDirection.Right : CardinalDirection.Left; }
            else { facingDirection = wishVelocity.Y > 0 ? CardinalDirection.Down : CardinalDirection.Up; }
        }
    }

    void UpdateBobber()
    {
        if (Controller.castRod)
        {
            if (bobber.Projectile is null)
            {
                // throw bobber
                // todo: add control system to set current magic number throwDistance (24)
                bobber.Projectile = new(Point.RoundToPoint(position + collider.Position + ((Vector2)collider.Size / 2f)),
                    24, facingDirection);
                bobber.State = BobberState.InAir;
            }
            else
            {
                // return bobber
                bobber.Projectile = null;
                bobber.State = BobberState.Withdrawn;
            }
        }
        if (bobber.Projectile is not null)
        {
            BobberState nextBobberState = bobber.Projectile.Value.FixedUpdate();

            if (nextBobberState == BobberState.Withdrawn)
            {
                bobber.Projectile = null;
            }

            bobber.State = nextBobberState;
        }
    }

    void SaveToSharedData()
    {
        lock (SharedDataLock)
        {
            SharedOldPosition = SharedPosition;
            SharedPosition = position;
            SharedFacingDirection = facingDirection;
            SharedBobber = bobber;
        }
    }

    public void FixedUpdate()
    {
        wishVelocity = GetWishVelocity();

        UpdateVelocity();
        UpdateFacingDirection();

        displacement = velocity;

        // set displacement to shift player onto pixel grid when stationary
        Rollover();

        ApplyDisplacement();

        UpdateBobber();

        SaveToSharedData();
    }
}
