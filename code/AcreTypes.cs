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
