using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class Player : Singleton<Player>
{
    public static Player Create(Vector2 position)
    { return Register(new Player(position)); }

    Vector2 position;


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
