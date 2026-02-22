using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace FishingGame;

static partial class Engine
{
    // assets
    public static readonly Texture2D atlasTexture;
    public static readonly Texture2D playerTexture;

    // singletons
    static readonly Controller controller;
    static readonly Player player;
    static readonly World world;
    static readonly RenderTexture2D lowRenderTexture;
    static readonly RenderTexture2D highRenderTexture;

    // fixed update
    public static bool Running { get; private set; } = true;
    static readonly Thread fixedUpdateThread;

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

        lowRenderTexture = LoadRenderTexture(internalWidth, internalHeight);
        SetTextureFilter(lowRenderTexture.Texture, TextureFilter.Point);
        highRenderTexture = LoadRenderTexture(internalWidth, internalHeight);
        SetTextureFilter(highRenderTexture.Texture, TextureFilter.Point);

        fixedUpdateThread = new Thread(FixedUpdateLoop);
        fixedUpdateThread.Start();
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

        controller.Update();
    }

    static void RenderToTextures()
    {
        BeginTextureMode(lowRenderTexture);
        ClearBackground(Color.Magenta);

        world.RenderTilemap();
        world.RenderLowProps();

        player.Render();

        EndTextureMode();

        BeginTextureMode(highRenderTexture);
        ClearBackground(new(0, 0, 0, 0)); // transparent background

        world.RenderHighProps();

        EndTextureMode();
    }

    static void RenderToScreen()
    {
        BeginDrawing();
        ClearBackground(Color.Black);

        Rectangle source = new(0, 0, internalWidth, -internalHeight);
        Rectangle dest = new(
                screenHeightLimited ? (screenWidth - scale * internalWidth) / 2f : 0f,
                screenHeightLimited ? 0f : (screenHeight - scale * internalHeight) / 2f,
                internalWidth * scale, internalHeight * scale
                );

        DrawTexturePro(lowRenderTexture.Texture, source, dest, Vector2.Zero, 0f, Color.White);
        DrawTexturePro(highRenderTexture.Texture, source, dest, Vector2.Zero, 0f, Color.White);

        EndDrawing();
    }

    static void Render()
    {
        RenderToTextures();
        RenderToScreen();
    }

    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [System.STAThread]
    public static void Main()
    {
        while (Running)
        {
            Update();
            Render();

            Running = !WindowShouldClose();
        }

        CloseWindow();
    }
}