using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System.Diagnostics;

namespace FishingGame;

class Player : Singleton<Player>
{
    public static Player Create(Vector2 position)
    { return Register(new Player(position)); }

    readonly NaturalSize spriteSize = new(16, 16);

    const float movementSpeed = 1f;
    const float rolloverDeadzone = 0.25f; // how close you must be to a pixel grid boundry to shift in the opposite direction of prior 
    const float rolloverSpeed = 0.1f;

    // fixed
    Vector2 fixedPosition;
    Vector2 displacement = Vector2.Zero;
    Vector2 oldDisplacement = Vector2.Zero;
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
        // add or cancel rollover target for x
        if (displacement.X != 0) { rolloverTargetX = null; }
        else if (oldDisplacement.X != 0)
        {
            float fract = fixedPosition.X - MathF.Truncate(fixedPosition.X);
            rolloverTargetX = (fract <= rolloverDeadzone || fract >= (1f - rolloverDeadzone))
                ? MathF.Round(fixedPosition.X)
                : (oldDisplacement.X > 0 ? MathF.Ceiling(fixedPosition.X) : MathF.Floor(fixedPosition.X));
        }
        if (fixedPosition.X == rolloverTargetX) { rolloverTargetX = null; }

        // add or cancel rollover target for y
        if (displacement.Y != 0) { rolloverTargetY = null; }
        else if (oldDisplacement.Y != 0)
        {
            float fract = fixedPosition.Y - MathF.Truncate(fixedPosition.Y);
            rolloverTargetY = (fract <= rolloverDeadzone || fract >= (1f - rolloverDeadzone))
                ? MathF.Round(fixedPosition.Y)
                : (oldDisplacement.Y > 0 ? MathF.Ceiling(fixedPosition.Y) : MathF.Floor(fixedPosition.Y));
        }
        if (fixedPosition.Y == rolloverTargetY) { rolloverTargetY = null; }

        // apply rollover
        // todo: important!!! this must use displacement for it's movement to allow it to work with collision but the current code above does not work with this!
        if (rolloverTargetX.HasValue)
        { fixedPosition.X = Utilities.MoveTowards(fixedPosition.X, rolloverTargetX.Value, rolloverSpeed); }
        if (rolloverTargetY.HasValue)
        { fixedPosition.Y = Utilities.MoveTowards(fixedPosition.Y, rolloverTargetY.Value, rolloverSpeed); }
    }

    public void FixedUpdate()
    {
        displacement = Controller.WishDir * movementSpeed;

        // shift player onto pixel grid when stationary
        Rollover();

        fixedPosition += displacement;

        // old vars
        oldDisplacement = displacement;

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
