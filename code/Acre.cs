using static FishingGame.Utilities;

namespace FishingGame;

readonly struct Acre
{

    public readonly Point Position;
    public readonly CollisionType[] CollisionMap;
    public readonly TileGraphicIndices[] Tilemap;
    public readonly Prop[] LowProps;
    public readonly Prop[] HighProps;

    // tile position to index into stored maps
    static int PointToIndex(int x, int y)
    {
        return x + (y * World.AcreSize.Width);
    }
    static int PointToIndex(Point pos)
    {
        return pos.X + (pos.Y * World.AcreSize.Width);
    }
    static int PixelPointToIndex(Point pos)
    {
        return (pos.X / TileSize.Width) + ((pos.Y / TileSize.Height) * World.AcreSize.Width);
    }

    public CollisionType PointToCollision(int x, int y)
    {
        if (x < 0 || x >= World.AcreSize.Width || y < 0 || y >= World.AcreSize.Height)
        { return CollisionType.Walkable; } // collsion out of bounds is walkable
        return CollisionMap[PointToIndex(x, y)];
    }

    static CollisionType HeightToCollision(TileHeight height)
    {
        return (height <= TileHeight.Water ? CollisionType.Wet : 
            (height < TileHeight.Hill ? CollisionType.Walkable : CollisionType.Hilly));
    }

    static void ApplyPropsToCollisionMap(ref CollisionType[] collisionMap, Prop[] props)
    {
        foreach (Prop prop in props)
        {
            if (prop.Collision == null) { continue; }
            collisionMap[PixelPointToIndex(prop.Location)] = (CollisionType)prop.Collision;
        }
    }

    static CollisionType[] HeightmapToCollisionMap(TileHeight[] heightmap, Prop[] lowProps, Prop[] highProps)
    {
        CollisionType[] collisionMap = new CollisionType[World.AcreSize.Width * World.AcreSize.Height];

        // col and row are in heightmap space
        for (int row = 1; row < World.AcreSize.Height + 1; row++)
        {
            for (int col = 1; col < World.AcreSize.Width + 1; col++)
            {
                int heightmapIndex = row * (World.AcreSize.Width + 2) + col;
                int collisionMapIndex = (row - 1) * World.AcreSize.Width + (col - 1);
                collisionMap[collisionMapIndex] = HeightToCollision(heightmap[heightmapIndex]);
            }
        }

        ApplyPropsToCollisionMap(ref collisionMap, lowProps);
        ApplyPropsToCollisionMap(ref collisionMap, highProps);

        return collisionMap;
    }

    static byte DeepWaterTileIndex(TileHeight vertical, TileHeight diagonal, TileHeight horizontal)
    {
        return (byte)(vertical == diagonal && diagonal == horizontal && horizontal == TileHeight.Water ? 1 : 0);
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
    static byte SandTileIndex(TileHeight vertical, TileHeight diagonal, TileHeight horizontal, bool top)
    {
        if (horizontal <= TileHeight.Sand && vertical <= TileHeight.Sand)
        { return 0; }
        if (horizontal >= TileHeight.Grass && vertical >= TileHeight.Grass)
        {
            if (top && horizontal >= TileHeight.Hill && vertical >= TileHeight.Hill)
            { return 2; }
            return 1;
        }
        if (horizontal >= TileHeight.Grass)
        {
            if (diagonal <= TileHeight.Water || (vertical <= TileHeight.Sand && diagonal <= TileHeight.Sand))
            { return 4; }
            return 3;
        }
        if (diagonal <= TileHeight.Water || (horizontal <= TileHeight.Sand && diagonal <= TileHeight.Sand))
        { return 6; }
        return 5;
    }
    static byte GrassTileIndex(TileHeight vertical, TileHeight diagonal, TileHeight horizontal, bool top)
    {
        if (horizontal == vertical && vertical == TileHeight.Water)
        {
            if (top && diagonal == TileHeight.Sand) 
            { return 2; }
            return 1;
        }
        if (top && horizontal >= TileHeight.Hill && vertical >= TileHeight.Hill)
        { return 3; }
        return 0;
    }
    static byte HillTileIndex(TileHeight vertical, TileHeight diagonal, TileHeight horizontal, bool top)
    {
        if (top || vertical == TileHeight.Hill || vertical == TileHeight.TallHill)
        { return 0; }
        if (diagonal == TileHeight.Hill || diagonal == TileHeight.TallHill)
        { return 1; }
        if (horizontal == TileHeight.Hill || horizontal == TileHeight.TallHill)
        { return 2; }
        if (vertical <= TileHeight.Water && horizontal < TileHeight.Grass)
        { return 3; }
        return 4;
    }
    static byte TallHillTileIndex(TileHeight vertical, TileHeight diagonal, TileHeight horizontal,
        TileHeight south, TileHeight southDiagonal, bool top)
    {
        if (south == TileHeight.TallHill)
        { return 0; }
        if (southDiagonal == TileHeight.TallHill)
        { return 1; }
        if (horizontal == TileHeight.TallHill)
        {
            if (top)
            { return 2; }
            if (diagonal == TileHeight.Hill)
            { return 3; }
            return 2;
        }
        if (top || southDiagonal == TileHeight.Hill)
        { return 4; }
        if (horizontal == TileHeight.Hill)
        { return 5; }
        if (horizontal <= TileHeight.Water && vertical <= TileHeight.Water)
        { return 6; }
        return 7;
    }

    static byte NeighbourhoodToTileGraphicIndex(TileHeight self,
        TileHeight vertical, TileHeight diagonal, TileHeight horizontal,
        TileHeight south, TileHeight southDiagonal, bool top)
    {
        return (byte)(self switch
        {
            TileHeight.DeepWater => DeepWaterBaseTextureIndex + DeepWaterTileIndex(vertical, diagonal, horizontal),
            TileHeight.Water     => WaterBaseTextureIndex + WaterTileIndex(vertical, diagonal, horizontal, top),
            TileHeight.Sand      => SandBaseTextureIndex + SandTileIndex(vertical, diagonal, horizontal, top),
            TileHeight.Grass     => GrassBaseTextureIndex + GrassTileIndex(vertical, diagonal, horizontal, top),
            TileHeight.Hill      => HillBaseTextureIndex + HillTileIndex(vertical, diagonal, horizontal, top),
            TileHeight.TallHill  => TallHillBaseTextureIndex + TallHillTileIndex(vertical, diagonal, horizontal, south, southDiagonal, top),
            _ => throw new ArgumentOutOfRangeException(nameof(self), "TileHeight variables must be within the TileHeight set")
        });
    }

    static TileGraphicIndices HeightmapToTileGraphicIndices(int row, int col, TileHeight[] heightmap)
    {
        // neighbourhood
        Span<TileHeight> neighbourhood = stackalloc TileHeight[9];
        int neighbourhoodIndex = 0;
        for (int nRow = -1; nRow <= 1; nRow++)
        {
            for (int nCol = -1; nCol <= 1; nCol++)
            {
                int heightmapIndex = (row + nRow)*(World.AcreSize.Width+2) + col + nCol;
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

    static TileGraphicIndices[] HeightmapToTilemap(TileHeight[] heightmap)
    {
        TileGraphicIndices[] tilemap = new TileGraphicIndices[World.AcreSize.Width * World.AcreSize.Height];

        // col and row are in heightmap space
        for (int row = 1; row < World.AcreSize.Height + 1; row++)
        {
            for (int col = 1; col < World.AcreSize.Width + 1; col++)
            {
                int tilemapIndex = (row - 1) * World.AcreSize.Width + (col - 1);
                tilemap[tilemapIndex] = HeightmapToTileGraphicIndices(row, col, heightmap);
            }
        }

        return tilemap;
    }

    static byte NeighbourhoodToOverhangGraphicIndex(TileHeight current, TileHeight south, TileHeight southDiagonal, TileHeight horizontal)
    {
        if(current < south)
        {
            if (horizontal >= south) { return 2; }
            if (southDiagonal >= south) { return 0; }
            return 1;
        }
        else // current = TallHill, south = Hill
        {
            if (southDiagonal > south) { return 1; }
            if (horizontal > south) { return 0; }
            return 2;
        }
    }

    static List<Prop> HeightmapToOverhangs(TileHeight[] heightmap)
    {
        List<Prop> overhangs = [];

        // col and row are in heightmap space
        for (int row = 1; row < World.AcreSize.Height + 1; row++)
        {
            for (int col = 1; col < World.AcreSize.Width + 1; col++)
            {
                int heightmapIndex = row * (World.AcreSize.Width + 2) + col;
                int southHeightmapIndex = heightmapIndex + World.AcreSize.Width + 2;
                if (heightmap[southHeightmapIndex] >= TileHeight.Hill && heightmap[heightmapIndex] != heightmap[southHeightmapIndex])
                {
                    byte baseGraphicIndex = heightmap[southHeightmapIndex] == TileHeight.TallHill? TallHillOverlayBaseTextureIndex : HillOverlayBaseTextureIndex;
                    byte leftGraphicIndex = (byte)(baseGraphicIndex + NeighbourhoodToOverhangGraphicIndex(
                        heightmap[heightmapIndex], heightmap[southHeightmapIndex], heightmap[southHeightmapIndex - 1], heightmap[heightmapIndex - 1]));
                    byte rightGraphicIndex = (byte)(baseGraphicIndex + NeighbourhoodToOverhangGraphicIndex(
                        heightmap[heightmapIndex], heightmap[southHeightmapIndex], heightmap[southHeightmapIndex + 1], heightmap[heightmapIndex + 1]));
                    if (leftGraphicIndex == rightGraphicIndex)
                    {
                        overhangs.Add(new Prop(new(col - 1, row - 1), leftGraphicIndex));
                    }
                    else
                    {
                        NaturalSize halfTileSize = new(TileSize.Width / 2, TileSize.Height);
                        Point destination = new((col - 1) * TileSize.Width, (row - 1) * TileSize.Height);
                        overhangs.Add(new Prop(destination, new NaturalRectangle(GraphicIndexToPoint(leftGraphicIndex), halfTileSize)));
                        overhangs.Add(new Prop(destination + new Point(TileSize.Width / 2, 0),
                            new NaturalRectangle(GraphicIndexToPoint(rightGraphicIndex) + new Point(TileSize.Width / 2, 0), halfTileSize)));
                    }
                }
            }
        }
        return overhangs;
    }

    public Acre(Point position, TileHeight[] heightmap, List<Prop> lowProps, List<Prop> highProps)
    {
        if (heightmap.Length != (World.AcreSize.Width + 2) * (World.AcreSize.Height + 2))
        { throw new ArgumentException($"The heightmap array length must be equal to the area described by {nameof(World.AcreSize)}", nameof(heightmap)); }

        this.LowProps = lowProps.ToArray();
        this.HighProps = highProps.Concat(HeightmapToOverhangs(heightmap)).ToArray();

        this.Position = position;
        CollisionMap = HeightmapToCollisionMap(heightmap, this.LowProps, this.HighProps);
        Tilemap = HeightmapToTilemap(heightmap);
    }
}
