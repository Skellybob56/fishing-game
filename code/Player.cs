using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class Player
{
    private static Player? Instance;
    Vector2 position;

    public static Player Create(Vector2 position)
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Player already exists.");
        }
        Instance = new Player(position);

        return Instance;
    }

    public static void Destroy()
    {
        Instance = null;
    }

    private Player(Vector2 position)
    {
        this.position = position;
    }

    public void Update()
    {
        
    }

    public void Render()
    {
        DrawTextureRec(
            Engine.playerTexture,
            new(0, 16, 16, 16),
            position,
            Color.White
            );
    }
}
