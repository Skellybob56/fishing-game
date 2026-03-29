using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class PlayerSprite : Singleton<PlayerSprite>
{
    public static PlayerSprite Create(PlayerActor playerActor)
    { return Register(new PlayerSprite(playerActor)); }

    public static readonly NaturalSize spriteSize = new(16, 16);
    static readonly NaturalSize bobberSpriteSize = new(8, 8);

    PlayerActor playerActor;
    CardinalDirection facingDirection;
    Vector2 renderPosition;
    Vector2 renderOldPosition;
    PlayerActor.Bobber? bobber;
    int oldCurrentInterpTick = -1;

    private PlayerSprite(PlayerActor playerActor)
    {
        this.playerActor = playerActor;
        LoadSharedData();
    }

    void LoadSharedData()
    {
        lock (playerActor.SharedDataLock)
        {
            renderPosition = playerActor.SharedPosition;
            renderOldPosition = playerActor.SharedOldPosition;
            facingDirection = playerActor.SharedFacingDirection;
            bobber = playerActor.SharedBobber;
        }
        oldCurrentInterpTick = Engine.CurrentInterpTick;
    }

    Vector2 GetAnimationSprite()
    {
        // todo: add walking animation
        return facingDirection switch
        {
            CardinalDirection.Up => new(0, 0),
            CardinalDirection.Down => new(0, 1),
            CardinalDirection.Left => new(0, 2),
            CardinalDirection.Right => new(0, 3),
            _ => throw new ArgumentOutOfRangeException(nameof(facingDirection), $"{nameof(CardinalDirection)} variables must be within the four cardinal directions")
        };
    }

    void RenderBobber(Vector2 screenPosition, float graphicalScale)
    {
        if (!bobber.HasValue) { return; }
        float currentTick = Engine.CurrentInterpTick + Engine.InterpT;
        Vector2 bobberSpritePosition = (bobber.Value.GetPosition(currentTick) - ((Vector2)bobberSpriteSize / 2f)) * graphicalScale + screenPosition;
        DrawTexturePro(
            Engine.spritesTexture,
            new(Vector2.Zero, bobberSpriteSize),
            new(bobberSpritePosition, (Vector2)bobberSpriteSize * graphicalScale),
            Vector2.Zero, 0f, Color.White
            );
    }

    public void Render(Vector2 screenPosition, float graphicalScale)
    {
        // load data
        if (oldCurrentInterpTick != Engine.CurrentInterpTick)
        {
            LoadSharedData();
        }

        Vector2 renderInterpolatedPosition = Vector2.Lerp(renderOldPosition, renderPosition, Engine.InterpT);

        DrawTexturePro(
            Engine.playerTexture,
            new(GetAnimationSprite() * spriteSize, spriteSize),
            new(renderInterpolatedPosition * graphicalScale + screenPosition, (Vector2)spriteSize * graphicalScale),
            Vector2.Zero, 0f, Color.White
            );

        RenderBobber(screenPosition, graphicalScale);
    }
}
