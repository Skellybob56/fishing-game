using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

static class Engine
{
    static World world;
    static RenderTexture2D renderTexture;

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

        renderTexture = LoadRenderTexture(internalWidth, internalHeight);
        SetTextureFilter(renderTexture.Texture, TextureFilter.Point);
        world = new World();
    }

    static void Update()
    {
        if (IsWindowResized())
        {
            WindowResized();
        }
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

        world.RenderAcre();

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