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

    Vector2 fixedPosition;
    Vector2 sharedPosition;
    Vector2 sharedOldPosition;
    Vector2 renderPosition;
    Vector2 renderOldPosition;
    Vector2 renderInterpolatedPosition;
    Vector2 displacement;
    readonly Lock sharedDataLock = new();
    int latestTick;
    Stopwatch interpolationStopwatch = new();


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

        // todo: change round to round in the direction of the latest movement
        if (displacement.X == 0)
        { fixedPosition = new(MathF.Round(fixedPosition.X), fixedPosition.Y); }
        if (displacement.Y == 0)
        { fixedPosition = new(fixedPosition.X, MathF.Round(fixedPosition.Y)); }

        fixedPosition += displacement;

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
        if (latestTick != Engine.currentTick)
        {
            lock (sharedDataLock)
            {
                renderPosition = sharedPosition;
                renderOldPosition = sharedOldPosition;
            }
            interpolationStopwatch.Restart();
            latestTick = Engine.currentTick;
        }

        renderInterpolatedPosition = Vector2.Lerp(renderOldPosition, renderPosition, 
            (float)interpolationStopwatch.Elapsed.TotalSeconds * (1f/Engine.FixedUpdateIntervalF));

        DrawTexturePro(
            Engine.playerTexture,
            new(0, spriteSize.height, (Vector2)spriteSize),
            new(renderInterpolatedPosition * graphicalScale + screenPosition, (Vector2)spriteSize * graphicalScale),
            Vector2.Zero, 0f, Color.White
            );
    }
}
