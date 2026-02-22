using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class Player : Singleton<Player>
{
    public static Player Create(Vector2 position)
    { return Register(new Player(position)); }

    const float playerSpeed = 1f;

    Vector2 position;
    readonly Lock sharedDataLock = new();


    private Player(Vector2 position)
    {
        this.position = position * playerSpeed;
    }

    public void FixedUpdate()
    {
        lock (sharedDataLock)
        {
            position += Controller.WishDir;
        }
    }

    public void Render()
    {
        lock (sharedDataLock)
        {
            DrawTextureRec(
                Engine.playerTexture,
                new(0, 16, 16, 16),
                position,
                Color.White
                );
        }
    }
}
