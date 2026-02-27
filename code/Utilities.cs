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

    static public Point GraphicIndexToPoint(byte graphicIndex)
    {
        return new((graphicIndex & 0x0f) * TileSize.width, ((graphicIndex & 0xf0) >> 4) * TileSize.height);
    }
    static public Point GraphicIndexQuadrantToPoint(byte graphicIndex, int quadrant)
    {
        return new((graphicIndex & 0x0f) * TileSize.width + (quadrant % 2 * TileSize.width / 2),
            ((graphicIndex & 0xf0) >> 4) * TileSize.height + (quadrant / 2 * TileSize.height / 2));
    }

    static public float MovementTowards(float current, float target, float maxDelta)
    {
        float delta = target - current;
        if (MathF.Abs(delta) <= maxDelta) { return delta; }
        return MathF.Sign(delta) * maxDelta;
    }

    static public Rectangle GrowRectangle(this Rectangle rect, Vector2 directionalGrowth)
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
}
