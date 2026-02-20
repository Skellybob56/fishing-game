using System;
using System.Collections.Generic;
using System.Text;

namespace FishingGame;

enum TileHeight : byte
{
    DeepWater,
    Water,
    Sand,
    Grass,
    Hill,
    TallHill
}
enum Collision : byte
{
    Wet,
    Walkable,
    Hilly
}

readonly struct AcrePosition
{
    public readonly int x;
    public readonly int y;

    public AcrePosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
readonly struct AcreSize
{
    public readonly int width;
    public readonly int height;

    public AcreSize(int width, int height)
    {
        if (width <= 0)
        { throw new ArgumentOutOfRangeException(nameof(width), "Acre size must be greater than zero"); }
        this.width = width;

        if (height <= 0)
        { throw new ArgumentOutOfRangeException(nameof(height), "Acre size must be greater than zero"); }
        this.height = height;
    }
}

readonly struct TileGraphicIndices
{
    public readonly byte topLeft;
    public readonly byte topRight;
    public readonly byte bottomLeft;
    public readonly byte bottomRight;

    public TileGraphicIndices(byte topLeft, byte topRight, byte bottomLeft, byte bottomRight)
    {
        this.topLeft = topLeft;
        this.topRight = topRight;
        this.bottomLeft = bottomLeft;
        this.bottomRight = bottomRight;
    }

    public TileGraphicIndices(Span<byte> indices)
    {
        if (indices.Length != 4)
        {
            throw new ArgumentException("TileGraphicIndices byte array must be of length four.", nameof(indices));
        }

        topLeft = indices[0];
        topRight = indices[1];
        bottomLeft = indices[2];
        bottomRight = indices[3];
    }

    public TileGraphicIndices(byte[] indices) : this(indices.AsSpan()) { }
}
