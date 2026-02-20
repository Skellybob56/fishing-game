using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace FishingGame;

static class Engine
{
    static World world;

    static Engine()
    {
        InitWindow(800, 480, "Fishing Game");
        world = new World();
    }

    static void Update()
    {
        
    }

    static void Render()
    {
        BeginDrawing();
        ClearBackground(Color.White);

        world.RenderPage();

        EndDrawing();
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