using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class Player
{
    Vector2 position;

    public Player()
    {

    }

    public void Render()
    {
        DrawTextureRec(
            Engine.playerTexture,
            new(0, 16, 16, 16),
            new(32, 32),
            Color.White
            );
    }
}
