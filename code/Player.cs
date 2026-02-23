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

    const float playerSpeed = 1f;
    const float playerRolloverDeadzone = 0.2f; // how close you must be to a pixel grid boundry to shift in the opposite direction of prior movement

    // fixed
    Vector2 fixedPosition;
    Vector2 displacement = Vector2.Zero;
    Vector2 oldDisplacement = Vector2.Zero;

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

    public void FixedUpdate()
    {
        displacement = Controller.WishDir * playerSpeed;

        if (oldDisplacement.X != 0 && displacement.X == 0)
        {
            float fract = fixedPosition.X - MathF.Truncate(fixedPosition.X);
            if (fract <= playerRolloverDeadzone || fract >= (1f-playerRolloverDeadzone))
            { fixedPosition = new(MathF.Round(fixedPosition.X), fixedPosition.Y); }
            else { fixedPosition = new(oldDisplacement.X > 0 ? MathF.Ceiling(fixedPosition.X) : MathF.Floor(fixedPosition.X), fixedPosition.Y); }

        }
        if (oldDisplacement.Y != 0 && displacement.Y == 0)
        {
            float fract = fixedPosition.Y - MathF.Truncate(fixedPosition.Y);
            if (fract <= playerRolloverDeadzone || fract >= (1f - playerRolloverDeadzone))
            { fixedPosition = new(fixedPosition.X, MathF.Round(fixedPosition.Y)); }
            else { fixedPosition = new(fixedPosition.X, oldDisplacement.Y > 0 ? MathF.Ceiling(fixedPosition.Y) : MathF.Floor(fixedPosition.Y)); }

        }

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
