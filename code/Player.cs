using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

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

    readonly NaturalRectangle collider = new(
        new Point(6, 13), // offset of top left corner from player positon
        new NaturalSize(4, 2)
        );

    // fixed
    Vector2 fixedPosition;
    Vector2 velocity = Vector2.Zero;
    Vector2 wishVelocity;
    Vector2 displacement;
    float remainingTime;
    float subtickTimeLength;
    Vector2 subtickDisplacement;
    IntersectionData? closestIntersectionData;
    Vector2 oldWishDir = Vector2.Zero;
    float? rolloverTargetX;
    float? rolloverTargetY;

    Rectangle currentCollider;
    NaturalRectangle currentCollidingTiles;
    NaturalRectangle potentialCollidingTiles;

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
        // add or cancel rollover target for x
        if (Controller.WishDir.X != 0) { rolloverTargetX = null; }
        else if (oldWishDir.X != 0)
        {
            float fract = fixedPosition.X - MathF.Truncate(fixedPosition.X);
            rolloverTargetX = (fract <= rolloverDeadzone || fract >= (1f - rolloverDeadzone))
                ? MathF.Round(fixedPosition.X)
                : (oldWishDir.X > 0 ? MathF.Ceiling(fixedPosition.X) : MathF.Floor(fixedPosition.X));
        }
        if (fixedPosition.X == rolloverTargetX) { rolloverTargetX = null; }

        // add or cancel rollover target for y
        if (Controller.WishDir.Y != 0) { rolloverTargetY = null; }
        else if (oldWishDir.Y != 0)
        {
            float fract = fixedPosition.Y - MathF.Truncate(fixedPosition.Y);
            rolloverTargetY = (fract <= rolloverDeadzone || fract >= (1f - rolloverDeadzone))
                ? MathF.Round(fixedPosition.Y)
                : (oldWishDir.Y > 0 ? MathF.Ceiling(fixedPosition.Y) : MathF.Floor(fixedPosition.Y));
        }
        if (fixedPosition.Y == rolloverTargetY) { rolloverTargetY = null; }

        // apply rollover
        if (rolloverTargetX.HasValue)
        { displacement.X += Utilities.MovementTowards(fixedPosition.X, rolloverTargetX.Value, rolloverSpeed); }
        if (rolloverTargetY.HasValue)
        { displacement.Y += Utilities.MovementTowards(fixedPosition.Y, rolloverTargetY.Value, rolloverSpeed); }
    }

    void ApplyDisplacement()
    {
        if (displacement == Vector2.Zero) { return; }

        remainingTime = 1f;
        subtickDisplacement = displacement;

        while (remainingTime > 0f) // while there is still time in the tick
        {
            subtickTimeLength = remainingTime;

            // measure the tiles the player is colliding with excluding the displacement
            currentCollider = new(
                (fixedPosition + (Vector2)collider.position) / (Vector2)Utilities.TileSize,
                ((Vector2)collider.size) / (Vector2)Utilities.TileSize
                );
            currentCollidingTiles = NaturalRectangle.ExpansiveRound(currentCollider, false); // use open intervals as the player is not on tiles they are touching

            // measure the tiles the player is colliding with including the displacement
            potentialCollidingTiles = NaturalRectangle.ExpansiveRound(
                currentCollider.GrowRectangle(subtickDisplacement / Utilities.TileSize)
                );

            // if the potentialCollidingTiles set is equal to the currentCollidingTiles set then just add displacement and return
            if (currentCollidingTiles == potentialCollidingTiles)
            { fixedPosition += subtickDisplacement; return; }

            closestIntersectionData = null;
            // iterate on each potentialCollidingTile
            for (int tileX = potentialCollidingTiles.position.x; tileX < potentialCollidingTiles.size.width + potentialCollidingTiles.position.x; tileX++)
            {
                for (int tileY = potentialCollidingTiles.position.y; tileY < potentialCollidingTiles.size.height + potentialCollidingTiles.position.y; tileY++)
                {
                    // if tile is walkable then continue
                    if (Engine.PointToCollision(tileX, tileY) == CollisionType.Walkable) { continue; }

                    // get collision data
                    IntersectionData? intersectionData = CollisionUtil.SweepBoxAgainstBox(currentCollider, 
                        subtickDisplacement / Utilities.TileSize, new(tileX, tileY, 1, 1));
                    
                    if (intersectionData != null) // if there is a collision
                    {
                        // set subtick displacement to bring the player just up to the tile
                        subtickDisplacement = (intersectionData.Value.intersectionPoint - currentCollider.Position) * Utilities.TileSize;
                        subtickTimeLength *= intersectionData.Value.timeTillCollision;
                        closestIntersectionData = intersectionData.Value; // store intersection data
                    }
                }
            }
            if (!closestIntersectionData.HasValue) // no intersection
            { fixedPosition += subtickDisplacement; return; }
            // reduce time by subtickTimeLength
            remainingTime -= subtickTimeLength;
            // apply normals
            displacement = displacement.ApplyNormal(closestIntersectionData.Value.collisionNormal);
            velocity = velocity.ApplyNormal(closestIntersectionData.Value.collisionNormal);
            // add subtick displacement to location
            fixedPosition = closestIntersectionData.Value.intersectionPoint * Utilities.TileSize - collider.position;
            // use the initial displacement to produce new displacement
            subtickDisplacement = displacement * remainingTime;
        }
    }

    public void FixedUpdate()
    {
        wishVelocity = Controller.WishDir * topSpeed;
        // todo: make deceleration faster when the player actively is opposing the current velocity than when they are just slowing down
        if (Vector2.Dot(velocity, wishVelocity) < 0 || wishVelocity.LengthSquared() < velocity.LengthSquared())
        {
            // reducing in speed
            velocity = Utilities.MoveTowards(velocity, wishVelocity, deceleration);
        }
        else 
        {
            velocity = Utilities.MoveTowards(velocity, wishVelocity, acceleration); 
        }
        
        displacement = velocity;

        // set displacement to shift player onto pixel grid when stationary
        Rollover();

        ApplyDisplacement();

        // old vars
        oldWishDir = Controller.WishDir;

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
