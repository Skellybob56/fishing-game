using Raylib_cs;

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
}
