using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class Controller : Singleton<Controller>
{
    public static Controller Create()
    { return Register(new Controller()); }

    public static Vector2 WishDir { get; private set; }

    const float leftStickDeadzoneX = 0.2f;
    const float leftStickDeadzoneY = 0.2f;

    static int gamepad = 0;
    static Vector2 leftStick;

    static void UpdateStickInput()
    {
        leftStick = new(
            GetGamepadAxisMovement(gamepad, GamepadAxis.LeftX),
            GetGamepadAxisMovement(gamepad, GamepadAxis.LeftY)
            );
        
        // cross-shaped deadzone used to make walking in cardinal directions easier
        if (leftStick.X > -leftStickDeadzoneX && leftStick.X < leftStickDeadzoneX)
        { leftStick.X = 0; }
        if (leftStick.Y > -leftStickDeadzoneY && leftStick.Y < leftStickDeadzoneY)
        { leftStick.Y = 0; }
    }

    static void UpdateWishDir()
    {
        WishDir = Vector2.Zero;

        if (leftStick != Vector2.Zero)
        {
            WishDir = leftStick;
        }
        else
        {
            if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up) || IsGamepadButtonDown(gamepad, GamepadButton.LeftFaceUp)) 
            { WishDir = new(WishDir.X, WishDir.Y - 1f); }
            if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left) || IsGamepadButtonDown(gamepad, GamepadButton.LeftFaceLeft)) 
            { WishDir = new(WishDir.X - 1f, WishDir.Y); }
            if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down) || IsGamepadButtonDown(gamepad, GamepadButton.LeftFaceDown)) 
            { WishDir = new(WishDir.X, WishDir.Y + 1f); }
            if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right) || IsGamepadButtonDown(gamepad, GamepadButton.LeftFaceRight)) 
            { WishDir = new(WishDir.X + 1f, WishDir.Y); }
        }
        if (WishDir.LengthSquared() > 1f) { WishDir = Vector2.Normalize(WishDir); }
    }

    public void Update()
    {
        UpdateStickInput();
        UpdateWishDir();
    }
}
