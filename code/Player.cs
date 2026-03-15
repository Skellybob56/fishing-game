using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using static FishingGame.Utilities;

namespace FishingGame;

class Player : Singleton<Player>
{
    public static Player Create(Vector2 position)
    { return Register(new Player(position)); }

    readonly NaturalSize spriteSize = new(16, 16);
    
    // consts
    const float topSpeed = 1f;
    const float acceleration = 0.1f;
    const float deceleration = 0.2f;
    const float rolloverDeadzone = 0.25f; // how close you must be to a pixel grid boundry to shift in the opposite direction of prior 
    const float rolloverSpeed = 0.1f;
    const float collisionVelocityCost = 0.2f;
    const float edgeBevelDepth = 0.25f;
    const float maxNudgeDistance = 0.5f; // maximum distance of nudge (per frame) caused by edge bevel measured in portion of bevel length

    readonly NaturalRectangle collider = new(
        new Point(6, 13), // offset of top left corner from player positon
        new NaturalSize(4, 2)
        );

    // fixed
    Vector2 fixedPosition;
    Vector2 velocity = Vector2.Zero;
    Vector2 wishVelocity;
    Vector2 displacement;
    Vector2 oldVelocity = Vector2.Zero;
    float? rolloverTargetX;
    float? rolloverTargetY;

    // shared
    readonly Lock sharedDataLock = new();
    int oldCurrentInterpTick = -1;
    Vector2 sharedPosition;
    Vector2 sharedOldPosition;

    // render
    Vector2 renderPosition;
    Vector2 renderOldPosition;
    Vector2 renderInterpolatedPosition;

    private Player(Vector2 position)
    {
        fixedPosition = position;
        sharedPosition = position;
        renderPosition = position;
        sharedOldPosition = position;
        renderOldPosition = position;
    }

    void Rollover()
    {
        // todo: make rollover start applying when the player stops actively moving in that axis.
        // cont.: apply rollover gently to account for predicted player location if the player does not apply any movement on that axis

        // add or cancel rollover target for x
        if (velocity.X != 0) { rolloverTargetX = null; }
        else if (oldVelocity.X != 0)
        {
            float fract = fixedPosition.X - MathF.Truncate(fixedPosition.X);
            rolloverTargetX = (fract <= rolloverDeadzone || fract >= (1f - rolloverDeadzone))
                ? MathF.Round(fixedPosition.X)
                : (oldVelocity.X > 0 ? MathF.Ceiling(fixedPosition.X) : MathF.Floor(fixedPosition.X));
        }
        if (fixedPosition.X == rolloverTargetX) { rolloverTargetX = null; }

        // add or cancel rollover target for y
        if (velocity.Y != 0) { rolloverTargetY = null; }
        else if (oldVelocity.Y != 0)
        {
            float fract = fixedPosition.Y - MathF.Truncate(fixedPosition.Y);
            rolloverTargetY = (fract <= rolloverDeadzone || fract >= (1f - rolloverDeadzone))
                ? MathF.Round(fixedPosition.Y)
                : (oldVelocity.Y > 0 ? MathF.Ceiling(fixedPosition.Y) : MathF.Floor(fixedPosition.Y));
        }
        if (fixedPosition.Y == rolloverTargetY) { rolloverTargetY = null; }

        // apply rollover
        if (rolloverTargetX.HasValue)
        { displacement.X += MovementTowards(fixedPosition.X, rolloverTargetX.Value, rolloverSpeed); }
        if (rolloverTargetY.HasValue)
        { displacement.Y += MovementTowards(fixedPosition.Y, rolloverTargetY.Value, rolloverSpeed); }
    }

    void ApplyDisplacement()
    {
        Vector2 GetSubtickDisplacementNudge(AABBHit closestAABBHit, Point closestTileHit)
        {
            // todo: only apply nudge if player is applying input towards the wall
            if (closestAABBHit.tEdge <= edgeBevelDepth || closestAABBHit.tEdge >= 1f - edgeBevelDepth)
            {
                bool horizontalCollision = closestAABBHit.collisionNormal == CollisionNormal.Left ||
                    closestAABBHit.collisionNormal == CollisionNormal.Right;

                int nudgeSign = closestAABBHit.tEdge > 0.5f ? 1 : -1;
                int normalSign = closestAABBHit.collisionNormal == CollisionNormal.Right ||
                    closestAABBHit.collisionNormal == CollisionNormal.Down ? -1 : 1;

                Point firstSample = horizontalCollision ? new(closestTileHit.x, closestTileHit.y + nudgeSign) :
                    new(closestTileHit.x + nudgeSign, closestTileHit.y);
                Point secondSample = horizontalCollision ? new(closestTileHit.x + normalSign, closestTileHit.y + nudgeSign) :
                    new(closestTileHit.x + nudgeSign, closestTileHit.y + normalSign);

                if (Engine.PointToCollision(firstSample) == CollisionType.Walkable &&
                    Engine.PointToCollision(secondSample) == CollisionType.Walkable)
                {
                    float tCorner = 0.5f - MathF.Abs(closestAABBHit.tEdge - 0.5f);
                    float bevelStrength = 1 - (tCorner / edgeBevelDepth);
                    float edgePixelLength = (horizontalCollision ? collider.size.height + TileSize.height : collider.size.width + TileSize.width);
                    float bevelPixelLength = edgePixelLength * edgeBevelDepth;
                    float edgeStart = horizontalCollision ? closestTileHit.y * TileSize.height - collider.size.height :
                        closestTileHit.x * TileSize.width - collider.size.width; // only on nudge axis
                    float cornerPixel = edgeStart + (nudgeSign > 0 ? edgePixelLength : 0f); // only on nudge axis
                    Vector2 cornerDelta = horizontalCollision ? new(0f, cornerPixel - closestAABBHit.intersectionPoint.Y) :
                        new(cornerPixel - closestAABBHit.intersectionPoint.X, 0f);

                    // lerp nudge between zero when just barely on the bevel to the maximum when right at the edge
                    // ensure nudge doesn't overshoot the corner
                    return MovementTowards(Vector2.Zero, cornerDelta, maxNudgeDistance * bevelStrength * bevelPixelLength);
                }
            }
            return Vector2.Zero;
        }
        
        if (displacement == Vector2.Zero) { return; }


        float remainingTime = 1f;
        Vector2 subtickDisplacement = displacement;

        float subtickTimeLength;
        (AABBHit closestAABBHit, Point closestTileHit)? closestHit;
        Rectangle currentCollider;
        NaturalRectangle currentCollidingTiles;
        NaturalRectangle potentialCollidingTiles;

        while (remainingTime > 0f) // while there is still time in the tick
        {
            subtickTimeLength = remainingTime;

            // measure the tiles the player is colliding with excluding the displacement
            currentCollider = new(
                (fixedPosition + (Vector2)collider.position) / (Vector2)TileSize,
                ((Vector2)collider.size) / (Vector2)TileSize
                );
            currentCollidingTiles = NaturalRectangle.ExpansiveRound(currentCollider, false); // use open intervals as the player is not on tiles they are touching

            // measure the tiles the player is colliding with including the displacement
            potentialCollidingTiles = NaturalRectangle.ExpansiveRound(
                currentCollider.GrowRectangle(subtickDisplacement / TileSize)
                );

            // if the potentialCollidingTiles set is equal to the currentCollidingTiles set then just add displacement and return
            if (currentCollidingTiles == potentialCollidingTiles)
            { fixedPosition += subtickDisplacement; return; }

            closestHit = null;
            // iterate on each potentialCollidingTile
            for (int tileX = potentialCollidingTiles.position.x; tileX < potentialCollidingTiles.size.width + potentialCollidingTiles.position.x; tileX++)
            {
                for (int tileY = potentialCollidingTiles.position.y; tileY < potentialCollidingTiles.size.height + potentialCollidingTiles.position.y; tileY++)
                {
                    // if tile is walkable then continue
                    if (Engine.PointToCollision(tileX, tileY) == CollisionType.Walkable) { continue; }

                    // get collision data
                    AABBHit? aabbHit = CollisionUtil.SweepBoxAgainstBox(currentCollider,
                        subtickDisplacement / TileSize, new(tileX, tileY, 1, 1));

                    if (aabbHit != null) // if there is a collision
                    {
                        // set subtick displacement to bring the player just up to the tile
                        // shortening subtick displacment ensures that subsequent hits are only registered if even closer
                        subtickDisplacement = (aabbHit.Value.intersectionPoint - currentCollider.Position) * TileSize;
                        subtickTimeLength *= aabbHit.Value.timeTillCollision;
                        closestHit = new(aabbHit.Value, new(tileX, tileY)); // store intersection data
                    }
                }
            }
            if (!closestHit.HasValue) // no intersection
            { fixedPosition += subtickDisplacement; return; }

            subtickDisplacement = Vector2.Zero; // this is safe as subtick displacement will be applied using the stored intersection point

            AABBHit closestAABBHit = closestHit.Value.closestAABBHit;
            Point closestTileHit = closestHit.Value.closestTileHit;

            // reduce time by subtickTimeLength
            remainingTime -= subtickTimeLength;

            // apply normal to displacement
            displacement = displacement.ApplyNormal(closestAABBHit.collisionNormal);

            // apply normal partially to velocity
            velocity = velocity.MoveTowards(velocity.ApplyNormal(closestAABBHit.collisionNormal), collisionVelocityCost);

            // check and apply nudge to round corner if needed
            subtickDisplacement += GetSubtickDisplacementNudge(closestAABBHit, closestTileHit);

            // move to collision intersection
            fixedPosition = closestAABBHit.intersectionPoint * TileSize - collider.position;

            // use the initial displacement to produce new displacement (note: this subtickDisplacement can also be modified by the nudge code above)
            subtickDisplacement += displacement * remainingTime;
        }
    }

    public void FixedUpdate()
    {
        wishVelocity = Controller.WishDir * topSpeed;
        // todo: make deceleration faster when the player actively is opposing the current velocity than when they are just slowing down
        if (Vector2.Dot(velocity, wishVelocity) < 0 || wishVelocity.LengthSquared() < velocity.LengthSquared())
        {
            // reducing in speed
            velocity = velocity.MoveTowards(wishVelocity, deceleration);
        }
        else
        {
            velocity = velocity.MoveTowards(wishVelocity, acceleration);
        }

        displacement = velocity;

        // set displacement to shift player onto pixel grid when stationary
        Rollover();

        ApplyDisplacement();

        // old vars
        oldVelocity = velocity;

        // share data
        lock (sharedDataLock)
        {
            sharedOldPosition = sharedPosition;
            sharedPosition = fixedPosition;
        }
    }

    public void Render(Vector2 screenPosition, float graphicalScale)
    {
        // load data
        if (oldCurrentInterpTick != Engine.CurrentInterpTick)
        {
            lock (sharedDataLock)
            {
                renderPosition = sharedPosition;
                renderOldPosition = sharedOldPosition;
            }
            oldCurrentInterpTick = Engine.CurrentInterpTick;
        }

        renderInterpolatedPosition = Vector2.Lerp(renderOldPosition, renderPosition, Engine.InterpT);

        DrawTexturePro(
            Engine.playerTexture,
            new(0, spriteSize.height, (Vector2)spriteSize),
            new(renderInterpolatedPosition * graphicalScale + screenPosition, (Vector2)spriteSize * graphicalScale),
            Vector2.Zero, 0f, Color.White
            );
    }
}
