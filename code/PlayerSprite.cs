using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class PlayerSprite : Singleton<PlayerSprite>
{
    public static PlayerSprite Create(PlayerActor playerActor)
    { return Register(new PlayerSprite(playerActor)); }

    readonly NaturalSize spriteSize = new(16, 16);

    PlayerActor playerActor;
    Vector2 renderPosition;
    Vector2 renderOldPosition;
    int oldCurrentInterpTick = -1;

    private PlayerSprite(PlayerActor playerActor)
    {
        this.playerActor = playerActor;
        UpdateSharedData();
    }

    void UpdateSharedData()
    {
        lock (playerActor.sharedDataLock)
        {
            renderPosition = playerActor.sharedPosition;
            renderOldPosition = playerActor.sharedOldPosition;
        }
        oldCurrentInterpTick = Engine.CurrentInterpTick;
    }

    public void Render(Vector2 screenPosition, float graphicalScale)
    {
        // load data
        if (oldCurrentInterpTick != Engine.CurrentInterpTick)
        {
            UpdateSharedData();
        }

        Vector2 renderInterpolatedPosition = Vector2.Lerp(renderOldPosition, renderPosition, Engine.InterpT);

        DrawTexturePro(
            Engine.playerTexture,
            new(0, spriteSize.height, spriteSize), // todo: replace 0, spriteSize.height with animation code
            new(renderInterpolatedPosition * graphicalScale + screenPosition, (Vector2)spriteSize * graphicalScale),
            Vector2.Zero, 0f, Color.White
            );
    }
}
