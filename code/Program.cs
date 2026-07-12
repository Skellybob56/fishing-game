using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace FishingGame;

static partial class Engine
{
	// assets
	public static readonly Texture2D AtlasTexture;
	public static readonly Texture2D PlayerTexture;
	public static readonly Texture2D SpritesTexture;

	// singletons
	static readonly Controller controller;
	static readonly (PlayerActor actor, PlayerSprite sprite) player;
	static readonly World world;
	static readonly RenderTexture2D lowRenderTexture;

	// fixed update
	public static bool Running { get; private set; } = true;
	static readonly Thread fixedUpdateThread;

	// interpolation
	public static float InterpT { get; private set; }
	public static int CurrentInterpTick { get; private set; } // todo: rename to CurrentRenderTick
	static long lastTickTimeRenderMsec;

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
	static float graphicalScale;
	static Camera2D camera;

	static Engine()
	{
		SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
		InitWindow(startScreenWidth, startScreenHeight, "Fishing Game");
		WindowResized();

		AtlasTexture = LoadTexture("textures/atlas.png");
		PlayerTexture = LoadTexture("textures/player.png");
		SpritesTexture = LoadTexture("textures/sprites.png");

		controller = Controller.Create();
		{
			PlayerActor playerActor = PlayerActor.Create(new(36, 32));
			player = new(playerActor, PlayerSprite.Create(playerActor));
		}
		world = World.Create();

		lowRenderTexture = LoadRenderTexture(internalWidth, internalHeight);
		SetTextureFilter(lowRenderTexture.Texture, TextureFilter.Point);

		fixedUpdateThread = new Thread(FixedUpdateLoop);
		fixedUpdateThread.Start();
	}

	public static CollisionType PointToCollision(int x, int y)
	{ return world.PointToCollision(x, y); }
	public static CollisionType PointToCollision(Point p)
	{ return PointToCollision(p.X, p.Y); }

	static void LoadSharedData()
	{
		player.sprite.LoadSharedData();
	}

	static void WindowResized()
	{
		screenWidth = GetScreenWidth();
		screenHeight = GetScreenHeight();
		screenRatio = (float)screenWidth / (float)screenHeight;
		screenHeightLimited = screenRatio > internalRatio;

		if (screenHeightLimited)
		{
			graphicalScale = screenHeight / (float)internalHeight;
			camera = new(Vector2.Zero, new((internalWidth - screenWidth / graphicalScale) / 2f, 0f), 0f, graphicalScale);
		}
		else
		{
			graphicalScale = screenWidth / (float)internalWidth;
			camera = new(Vector2.Zero, new((internalHeight - screenHeight / graphicalScale) / 2f, 0f), 0f, graphicalScale);
		}
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
	}

	static void RenderToTexture()
	{
		BeginTextureMode(lowRenderTexture);
		ClearBackground(Color.Magenta);

		world.RenderTilemap();
		world.RenderLowProps();

		EndTextureMode();
	}

	static void RenderToScreen()
	{
		BeginDrawing();
		BeginMode2D(camera);
		ClearBackground(Color.Black);

		DrawTexturePro(lowRenderTexture.Texture, new(0, 0, internalWidth, -internalHeight), new(0, 0, internalWidth, internalHeight), Vector2.Zero, 0f, Color.White);

		// todo: move all prop/sprite rendering to use a sorting system to make the render in the correct order
		// cont. the y location can be different from the tile location of the sprite (for example, overhangs should be understood as being located at the y value below them)
		player.sprite.Render();

		world.RenderHighProps();

		EndMode2D();
		EndDrawing();
	}

	static void Render()
	{
		RenderToTexture();
		RenderToScreen();
	}

	// STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
	[System.STAThread]
	public static void Main()
	{
		while (Running)
		{
			lock (SharedDataLock)
			{
				if (CurrentInterpTick != CurrentTick)
				{
					lastTickTimeRenderMsec = lastTickTimeSharedMsec;
					LoadSharedData();
					CurrentInterpTick = CurrentTick;
				}
			}
			InterpT = (float)(stopwatchFixedUpdate.ElapsedMilliseconds - lastTickTimeRenderMsec) * (1f / FixedUpdateIntervalMSec);

			Update();

			Render();

			Running = !WindowShouldClose(); // todo: make the program exit gracefully (including fixing race conditions with the Running variable)
		}

		CloseWindow();
	}
}