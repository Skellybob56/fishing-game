using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

static class Engine
{
    // assets
    public static readonly Texture2D atlasTexture;
    public static readonly Texture2D playerTexture;

    // singletons
    static readonly Controller controller;
    static readonly Player player;
    static readonly World world;
    static readonly RenderTexture2D renderTexture;

    // screen resolution vars
    const int startScreenWidth = 800;
    const int startScreenHeight = 480;

    const int internalWidth = 200;
    const int internalHeight = 120;
    const float internalRatio = (float)internalWidth / (float)internalHeight;

    static int screenWidth;
    static int screenHeight;
    static float screenRatio;

    static bool screenHeightLimited;
    static float scale;

    static void WindowResized()
    {
        screenWidth = GetScreenWidth();
        screenHeight = GetScreenHeight();
        screenRatio = (float)screenWidth / (float)screenHeight;
        screenHeightLimited = screenRatio > internalRatio;
        scale = screenHeightLimited ?
            screenHeight / (float)internalHeight :
            screenWidth / (float)internalWidth;
    }

    static Engine()
    {
        SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
        InitWindow(startScreenWidth, startScreenHeight, "Fishing Game");
        WindowResized();

        atlasTexture = LoadTexture("textures/atlas.png");
        playerTexture = LoadTexture("textures/player.png");

        controller = Controller.Create();
        player = Player.Create(new(32, 32));
        world = World.Create();

        renderTexture = LoadRenderTexture(internalWidth, internalHeight);
        SetTextureFilter(renderTexture.Texture, TextureFilter.Point);
    }

    static void Update()
    {
        if (IsKeyPressed(KeyboardKey.F11) || (IsKeyPressed(KeyboardKey.Enter) && IsKeyDown(KeyboardKey.LeftAlt)))
        {
            ToggleBorderlessWindowed();
        }
        if (IsWindowResized())
        {
            WindowResized();
        }

        player.Update();
    }

    static void RenderRenderTextureToScreen()
    {
        BeginDrawing();
        ClearBackground(Color.Black);
        DrawTexturePro(
            renderTexture.Texture,
            new Rectangle(0, 0, internalWidth, -internalHeight),
            new Rectangle(
                screenHeightLimited ? (screenWidth - scale * internalWidth) / 2f : 0f,
                screenHeightLimited ? 0f : (screenHeight - scale * internalHeight) / 2f,
                internalWidth * scale, internalHeight * scale),
            Vector2.Zero, 0f, Color.White
            );
        EndDrawing();
    }

    static void Render()
    {
        // render to texture
        BeginTextureMode(renderTexture);
        ClearBackground(Color.Magenta);

        world.RenderTilemap();
        world.RenderLowProps();

        player.Render();

        world.RenderHighProps();

        EndTextureMode();

        // render to screen
        RenderRenderTextureToScreen();
    }

    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [System.STAThread]
    public static void Main()
    {
        while (!WindowShouldClose())
        {
            Update();
            Render();
        }

        CloseWindow();
    }
}