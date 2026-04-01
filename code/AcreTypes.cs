namespace FishingGame;

public enum TileHeight : byte
{
    DeepWater,
    Water,
    Sand,
    Grass,
    Hill,
    TallHill
}
public enum CollisionType : byte
{
    Wet,
    Walkable,
    Hilly
}

readonly struct TileGraphicIndices
{
    public readonly byte TopLeft;
    public readonly byte TopRight;
    public readonly byte BottomLeft;
    public readonly byte BottomRight;

    public TileGraphicIndices(byte topLeft, byte topRight, byte bottomLeft, byte bottomRight)
    {
        this.TopLeft = topLeft;
        this.TopRight = topRight;
        this.BottomLeft = bottomLeft;
        this.BottomRight = bottomRight;
    }

    public TileGraphicIndices(Span<byte> indices)
    {
        if (indices.Length != 4)
        {
            throw new ArgumentException("TileGraphicIndices byte array must be of length four.", nameof(indices));
        }

        TopLeft = indices[0];
        TopRight = indices[1];
        BottomLeft = indices[2];
        BottomRight = indices[3];
    }

    public TileGraphicIndices(byte[] indices) : this(indices.AsSpan()) { }
}
