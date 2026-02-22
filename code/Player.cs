using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class Player : Singleton<Player>
{
    public static Player Create(Vector2 position)
    { return Register(new Player(position)); }

    readonly NaturalSize spriteSize = new(16, 16);

    const float playerSpeed = 0.5f;

    Vector2 position;
    readonly Lock sharedDataLock = new();


    private Player(Vector2 position)
    {
        this.position = position;
    }

    public void FixedUpdate()
    {
        lock (sharedDataLock)
        {
            position += Controller.WishDir * playerSpeed;
        }
    }

    public void Render(Vector2 screenPosition, float graphicalScale)
    {
        lock (sharedDataLock)
        {
            DrawTexturePro(
                Engine.playerTexture,
                new(0, spriteSize.height, (Vector2)spriteSize),
                new(position * graphicalScale + screenPosition, (Vector2)spriteSize * graphicalScale),
                Vector2.Zero, 0f, Color.White
                );
        }
    }
}
