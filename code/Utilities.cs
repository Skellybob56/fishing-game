using Raylib_cs;
using System.Numerics;

namespace FishingGame;

public static class Utilities
{
    // TileSize must be a multiple of 2
    public static readonly NaturalSize TileSize = new(8, 8);

    public const byte DeepWaterBaseTextureIndex = 0;
    public const byte WaterBaseTextureIndex = 0x10;
    public const byte SandBaseTextureIndex = 0x20;
    public const byte GrassBaseTextureIndex = 0x28;
    public const byte HillBaseTextureIndex = 0x30;
    public const byte TallHillBaseTextureIndex = 0x38;

    public const byte PropBaseTextureIndex = 0x80;
    public const byte HillOverlayBaseTextureIndex = PropBaseTextureIndex + 0x00;
    public const byte TallHillOverlayBaseTextureIndex = PropBaseTextureIndex + 0x04;
    public const byte BridgeBaseTextureIndex = PropBaseTextureIndex + 0x07;
    public const byte RockBaseTextureIndex = PropBaseTextureIndex + 0x10;
    public const byte FlowerBaseTextureIndex = PropBaseTextureIndex + 0x18;

    public static Point GraphicIndexToPoint(byte graphicIndex)
    {
        return new((graphicIndex & 0x0f) * TileSize.width, ((graphicIndex & 0xf0) >> 4) * TileSize.height);
    }
    public static Point GraphicIndexQuadrantToPoint(byte graphicIndex, int quadrant)
    {
        return new((graphicIndex & 0x0f) * TileSize.width + (quadrant % 2 * TileSize.width / 2),
            ((graphicIndex & 0xf0) >> 4) * TileSize.height + (quadrant / 2 * TileSize.height / 2));
    }

    public static float MovementTowards(float current, float target, float maxDelta)
    {
        return current.MoveTowards(target, maxDelta) - current;
    }

    public static Vector2 MovementTowards(Vector2 current, Vector2 target, float maxDelta)
    {
        return new(MovementTowards(current.X, target.X, maxDelta),
            MovementTowards(current.X, target.Y, maxDelta));
    }

    public static float MoveTowards(this float current, float target, float maxDelta)
    {
        float delta = target - current;
        if (MathF.Abs(delta) <= maxDelta) { return target; }
        return current + MathF.Sign(delta) * maxDelta;
    }

    public static Vector2 MoveTowards(this Vector2 current, Vector2 target, float maxDelta)
    {
        return new(current.X.MoveTowards(target.X, maxDelta),
            current.Y.MoveTowards(target.Y, maxDelta));
    }

    public static Rectangle GrowRectangle(this Rectangle rect, Vector2 directionalGrowth)
    {
        Rectangle output = rect;

        if (directionalGrowth.X > 0)
        { output.Width += directionalGrowth.X; }
        else if (directionalGrowth.X < 0)
        { output.X += directionalGrowth.X; output.Width -= directionalGrowth.X; }

        if (directionalGrowth.Y > 0)
        { output.Height += directionalGrowth.Y; }
        else if (directionalGrowth.Y < 0)
        { output.Y += directionalGrowth.Y; output.Height -= directionalGrowth.Y; }

        return output;
    }

    public static Vector2 ToVector2(this CardinalDirection direction)
    {
        return direction switch
        {
            CardinalDirection.Up => -Vector2.UnitY,
            CardinalDirection.Down => Vector2.UnitY,
            CardinalDirection.Left => -Vector2.UnitX,
            CardinalDirection.Right => Vector2.UnitX,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), "CollisionNormal variables must be Up, Down, Left or Right"),
        }
    }
}
