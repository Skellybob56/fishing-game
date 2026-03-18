using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

partial class Player : Singleton<Player>
{
    public static Player Create(Vector2 position)
    { return Register(new Player(position)); }

    readonly NaturalSize spriteSize = new(16, 16);

    // consts
    const float topSpeed = 1f;
    const float acceleration = 0.1f;
    const float deceleration = 0.2f;
    const float rolloverDeadzone = 0.25f; // how close you must be to a pixel grid boundry to shift in the opposite direction of prior 
    const float rolloverSpeed = 0.1f;
    const float collisionVelocityCost = 0.2f;
    const float edgeBevelDepth = 0.4f;
    const float maxNudgePortion = 0.1f; // maximum distance of nudge (per frame) caused by edge bevel measured in portion of bevel length

    readonly NaturalRectangle collider = new(
        new Point(6, 13), // offset of top left corner from player positon
        new NaturalSize(4, 2)
        );

    // fixed
    Vector2 fixedPosition;
    Vector2 velocity = Vector2.Zero;
    Vector2 wishVelocity;
    Vector2 displacement;
    Vector2 oldVelocity = Vector2.Zero;
    float? rolloverTargetX;
    float? rolloverTargetY;

    // shared
    readonly Lock sharedDataLock = new();
    Vector2 sharedPosition;
    Vector2 sharedOldPosition;

    // render
    Vector2 renderPosition;
    Vector2 renderOldPosition;
    int oldCurrentInterpTick = -1;

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
        wishVelocity = Controller.WishDir * topSpeed;
        // todo: make deceleration faster when the player actively is opposing the current velocity than when they are just slowing down
        // cont. use an enum to store the state of [acceleration/deceleration/turning around], it is useful to know elsewhere (e.g. ApplySubtickDisplacementNudge)
        // cont. acceleration is when wishVelocity is equal to or greater than velocity in length and is facing in the same direction
        // cont. deceleration is when wishVelocity is less than velocity in length and is facing in the same direction
        // cont. turning around is when wishVelocity is facing away from velocity
        if (Vector2.Dot(velocity, wishVelocity) < 0 || wishVelocity.LengthSquared() < velocity.LengthSquared())
        {
            // reducing in speed
            velocity = velocity.MoveTowards(wishVelocity, deceleration);
        }
        else
        {
            velocity = velocity.MoveTowards(wishVelocity, acceleration);
        }

        displacement = velocity;

        // set displacement to shift player onto pixel grid when stationary
        Rollover();

        ApplyDisplacement();

        // old vars
        oldVelocity = velocity;

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

        Vector2 renderInterpolatedPosition = Vector2.Lerp(renderOldPosition, renderPosition, Engine.InterpT);

        DrawTexturePro(
            Engine.playerTexture,
            new(0, spriteSize.height, spriteSize), // todo: replace 0, spriteSize.height with animation code
            new(renderInterpolatedPosition * graphicalScale + screenPosition, (Vector2)spriteSize * graphicalScale),
            Vector2.Zero, 0f, Color.White
            );
    }
}
