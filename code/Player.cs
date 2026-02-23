using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

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
    Vector2 displacement;
    bool newFixedData = false;
    readonly Lock sharedDataLock = new();


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
            newFixedData = true;
        }
    }

    public void Render(Vector2 screenPosition, float graphicalScale)
    {
        if (newFixedData)
        {
            lock (sharedDataLock)
            {
                renderPosition = sharedPosition;
                renderOldPosition = sharedOldPosition;
                newFixedData = false;
            }
        }
        DrawTexturePro(
            Engine.playerTexture,
            new(0, spriteSize.height, (Vector2)spriteSize),
            new(renderPosition * graphicalScale + screenPosition, (Vector2)spriteSize * graphicalScale),
            Vector2.Zero, 0f, Color.White
            );
    }
}
