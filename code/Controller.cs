using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

class Controller : Singleton<Controller>
{
    public static Controller Create()
    { return Register(new Controller()); }

    public static Vector2 WishDir { get; private set; }

    private static void UpdateWishDir()
    {
        WishDir = Vector2.Zero;
        if (IsKeyDown(KeyboardKey.W)) { WishDir = new(WishDir.X, WishDir.Y - 1f); }
        if (IsKeyDown(KeyboardKey.A)) { WishDir = new(WishDir.X - 1f, WishDir.Y); }
        if (IsKeyDown(KeyboardKey.S)) { WishDir = new(WishDir.X, WishDir.Y + 1f); }
        if (IsKeyDown(KeyboardKey.D)) { WishDir = new(WishDir.X + 1f, WishDir.Y); }
        if (WishDir.LengthSquared() > 1f) { WishDir = Vector2.Normalize(WishDir); }
    }

    public void Update()
    {
        UpdateWishDir();
    }
}
