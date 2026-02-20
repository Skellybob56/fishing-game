using System;
using System.Diagnostics;

namespace FishingGame;

readonly struct Acre
{
    const byte DeepWaterBaseTextureIndex = 0;
    const byte WaterBaseTextureIndex = 16;
    const byte SandBaseTextureIndex = 32;
    const byte GrassBaseTextureIndex = 40;
    const byte HillBaseTextureIndex = 48;
    const byte TallHillBaseTextureIndex = 56;

    const byte HillOverlayBaseTextureIndex = 128 | 0;
    const byte TallHillOverlayBaseTextureIndex = 128 | 4;

    public readonly AcrePosition position;
    public readonly Collision[] collisionMap;
    public readonly TileGraphicIndices[] tilemap;

    static Collision HeightToCollision(TileHeight height)
    {
        return (height <= TileHeight.Water ? Collision.Wet : 
            (height < TileHeight.Hill ? Collision.Walkable : Collision.Hilly));
    }

    static Collision[] HeightmapToCollisionMap(AcreSize acreSize, TileHeight[] heightmap)
    {
        Collision[] collisionMap = new Collision[acreSize.width * acreSize.height];

        for (int row = 1; row < acreSize.height - 1; row++)
        {
            for (int col = 1; col < acreSize.width - 1; col++)
            {
                int heightmapIndex = row * (acreSize.width + 2) + col;
                int collisionMapIndex = (row - 1) * acreSize.width + (col - 1);
                collisionMap[collisionMapIndex] = HeightToCollision(heightmap[heightmapIndex]);
            }
        }

        return collisionMap;
    }

    static byte DeepWaterTileIndex(TileHeight verticalAdjacent, TileHeight diagonal, TileHeight horizontalAdjacent)
    {
        return (byte)(verticalAdjacent == diagonal && diagonal == horizontalAdjacent && horizontalAdjacent == TileHeight.Water ? 1 : 0);
    }
    static byte WaterTileIndex(TileHeight vertical, TileHeight diagonal, TileHeight horizontal, bool top)
    {
        if (vertical >= TileHeight.Grass)
        {
            if (top)
            {
                if (vertical >= TileHeight.Hill && horizontal >= TileHeight.Hill)
                { return 9; }

                if (horizontal >= TileHeight.Grass)
                {
                    if (diagonal <= TileHeight.Water)
                    { return 12; }
                }
                else if (horizontal == TileHeight.Sand)
                {
                    if (diagonal >= TileHeight.Grass)
                    { return 11; }
                    return 10;
                }
                else if (horizontal <= TileHeight.Water)
                {
                    if (diagonal <= TileHeight.Water)
                    { return 13; }
                    return 14;
                }
            }
            else // bottom
            {
                if (horizontal <= TileHeight.Water)
                { return 0; }
                if (horizontal == TileHeight.Sand || 
                    (horizontal >= TileHeight.Grass && diagonal <= TileHeight.Water))
                { return 2; }
            }

            return 8;
        }
        else if (vertical == TileHeight.Sand)
        {
            if (!top && diagonal >= TileHeight.Grass && horizontal <= TileHeight.Water)
            { return 6; }
            if (horizontal <= TileHeight.Water)
            {
                if (diagonal>=TileHeight.Sand)
                { return 7; }
                return 6;
            }
            else
            {
                if (horizontal >= TileHeight.Grass && diagonal >= TileHeight.Grass)
                { return 5; }
                return 4;
            }
        }
        else // wet
        {
            if (vertical == horizontal && horizontal == TileHeight.DeepWater)
            { return 1; }
            if (horizontal > TileHeight.Water)
            {
                if (top && diagonal<=TileHeight.Water && horizontal>=TileHeight.Grass)
                { return 3; }
                return 2;
            }
            return 0;
        }
    }
    static byte SandTileIndex(TileHeight verticalAdjacent, TileHeight diagonal, TileHeight horizontalAdjacent, bool top)
    {
        if (horizontalAdjacent <= TileHeight.Sand && verticalAdjacent <= TileHeight.Sand)
        { return 0; }
        if (horizontalAdjacent >= TileHeight.Grass && verticalAdjacent >= TileHeight.Grass)
        {
            if (top && horizontalAdjacent >= TileHeight.Hill && verticalAdjacent >= TileHeight.Hill)
            { return 2; }
            return 1;
        }
        if (horizontalAdjacent >= TileHeight.Grass)
        {
            if (diagonal <= TileHeight.Water || (horizontalAdjacent <= TileHeight.Sand && diagonal <= TileHeight.Sand))
            { return 4; }
            return 3;
        }
        if (diagonal <= TileHeight.Water || (horizontalAdjacent <= TileHeight.Sand && diagonal <= TileHeight.Sand))
        { return 6; }
        return 5;
    }
    static byte GrassTileIndex(TileHeight verticalAdjacent, TileHeight diagonal, TileHeight horizontalAdjacent, bool top)
    {
        if (horizontalAdjacent == verticalAdjacent && verticalAdjacent == TileHeight.Water)
        {
            if (top && diagonal == TileHeight.Sand) 
            { return 2; }
            return 1;
        }
        if (top && horizontalAdjacent >= TileHeight.Hill && verticalAdjacent >= TileHeight.Hill)
        { return 3; }
        return 0;
    }
    static byte HillTileIndex(TileHeight verticalAdjacent, TileHeight diagonal, TileHeight horizontalAdjacent, bool top)
    {
        if (top || verticalAdjacent == TileHeight.Hill || verticalAdjacent == TileHeight.TallHill)
        { return 0; }
        if (diagonal == TileHeight.Hill || diagonal == TileHeight.TallHill)
        { return 1; }
        if (horizontalAdjacent == TileHeight.Hill || horizontalAdjacent == TileHeight.TallHill)
        { return 2; }
        if (verticalAdjacent <= TileHeight.Water && horizontalAdjacent < TileHeight.Grass)
        { return 3; }
        return 4;
    }
    static byte TallHillTileIndex(TileHeight verticalAdjacent, TileHeight diagonal, TileHeight horizontalAdjacent,
        TileHeight south, TileHeight southDiagonal, bool top)
    {
        if (south == TileHeight.TallHill)
        { return 0; }
        if (southDiagonal == TileHeight.TallHill)
        { return 1; }
        if (horizontalAdjacent == TileHeight.TallHill)
        {
            if (top)
            { return 2; }
            if (diagonal == TileHeight.Hill)
            { return 3; }
            return 2;
        }
        if (top || southDiagonal == TileHeight.Hill)
        { return 4; }
        if (horizontalAdjacent == TileHeight.Hill)
        { return 5; }
        if (horizontalAdjacent <= TileHeight.Water && verticalAdjacent <= TileHeight.Water)
        { return 6; }
        return 7;
    }

    static byte NeighbourhoodToTileGraphicIndex(TileHeight self,
        TileHeight verticalAdjacent, TileHeight diagonal, TileHeight horizontalAdjacent,
        TileHeight south, TileHeight southDiagonal, bool top)
    {
        return (byte)(self switch
        {
            TileHeight.DeepWater => DeepWaterBaseTextureIndex + DeepWaterTileIndex(verticalAdjacent, diagonal, horizontalAdjacent),
            TileHeight.Water     => WaterBaseTextureIndex + WaterTileIndex(verticalAdjacent, diagonal, horizontalAdjacent, top),
            TileHeight.Sand      => SandBaseTextureIndex + SandTileIndex(verticalAdjacent, diagonal, horizontalAdjacent, top),
            TileHeight.Grass     => GrassBaseTextureIndex + GrassTileIndex(verticalAdjacent, diagonal, horizontalAdjacent, top),
            TileHeight.Hill      => HillBaseTextureIndex + HillTileIndex(verticalAdjacent, diagonal, horizontalAdjacent, top),
            TileHeight.TallHill  => TallHillBaseTextureIndex + TallHillTileIndex(verticalAdjacent, diagonal, horizontalAdjacent, south, southDiagonal, top),
            _ => throw new ArgumentOutOfRangeException(nameof(self), "TileHeight variables must be within the TileHeight set")
        });
    }

    static TileGraphicIndices HeightmapToTileGraphicIndices(AcreSize acreSize, int row, int col, TileHeight[] heightmap)
    {
        // neighbourhood
        Span<TileHeight> neighbourhood = stackalloc TileHeight[9];
        int neighbourhoodIndex = 0;
        for (int nRow = -1; nRow <= 1; nRow++)
        {
            for (int nCol = -1; nCol <= 1; nCol++)
            {
                int heightmapIndex = (row + nRow)*(acreSize.width+2) + col + nCol;
                neighbourhood[neighbourhoodIndex] = heightmap[heightmapIndex];
                neighbourhoodIndex++;
            }
        }

        Span<byte> tileGraphicIndices = stackalloc byte[4];
        for (int quadrant = 0; quadrant < 4; quadrant++)
        {
            bool top = quadrant <= 1;
            bool left = quadrant % 2 == 0;
            tileGraphicIndices[quadrant] = NeighbourhoodToTileGraphicIndex(
                neighbourhood[4],
                top? neighbourhood[1] : neighbourhood[7],
                top? (left? neighbourhood[0] : neighbourhood[2]) : (left ? neighbourhood[6] : neighbourhood[8]),
                left? neighbourhood[3] : neighbourhood[5],
                neighbourhood[7],
                left ? neighbourhood[6] : neighbourhood[8],
                top
                );
        }

        return new TileGraphicIndices(tileGraphicIndices);
    }

    static TileGraphicIndices[] HeightmapToTilemap(AcreSize acreSize, TileHeight[] heightmap)
    {
        TileGraphicIndices[] tilemap = new TileGraphicIndices[acreSize.width * acreSize.height];

        // col and row are in heightmap space
        for (int row = 1; row < acreSize.height + 1; row++)
        {
            for (int col = 1; col < acreSize.width + 1; col++)
            {
                int tilemapIndex = (row - 1) * acreSize.width + (col - 1);
                tilemap[tilemapIndex] = HeightmapToTileGraphicIndices(acreSize, row, col, heightmap);
            }
        }

        return tilemap;
    }

    public Acre(AcrePosition position, AcreSize acreSize, TileHeight[] heightmap)
    {
        if (heightmap.Length != (acreSize.width + 2) * (acreSize.height + 2))
        { throw new ArgumentException($"The heightmap array length must be equal to the area described by {nameof(acreSize)}", nameof(heightmap)); }

        this.position = position;
        collisionMap = HeightmapToCollisionMap(acreSize, heightmap);
        tilemap = HeightmapToTilemap(acreSize, heightmap);
    }
}
