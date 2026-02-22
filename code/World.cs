using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using static FishingGame.AtlasUtilities;

namespace FishingGame;

public class World
{
    readonly Acre acre;
    static public readonly NaturalSize acreSize = new(25, 15);
    
    public World()
    {
        // <temp map loading>
        byte[] byte_heightmap = [
                5,5,5,5,5,5,5,5,4,4,4,4,3,3,3,3,3,1,1,3,3,4,4,4,4,5,5,
                5,5,5,5,5,5,5,4,4,4,4,4,3,3,3,3,2,1,1,3,3,4,4,4,5,5,5,
                5,5,5,5,5,1,4,4,2,2,3,3,3,3,2,2,2,1,1,3,3,3,1,4,4,5,5,
                5,5,4,4,1,1,3,3,3,2,3,1,1,1,1,1,1,1,1,3,3,3,3,1,4,5,4,
                4,4,4,1,1,3,3,3,3,1,1,1,1,1,1,1,1,0,1,1,3,3,3,3,3,3,3,
                3,3,3,3,3,3,3,3,1,1,1,1,2,2,2,1,1,0,1,1,3,3,3,3,3,3,3,
                3,3,3,3,3,3,3,1,1,1,1,2,2,3,3,3,1,0,0,1,3,3,3,3,3,3,3,
                3,3,3,3,3,3,1,1,1,1,1,2,3,3,3,3,1,0,0,1,3,3,3,3,3,3,3,
                3,3,3,3,3,3,1,1,1,1,1,1,3,3,3,3,1,1,0,1,1,3,3,3,3,3,3,
                3,3,3,3,3,3,1,1,1,1,1,1,3,3,3,1,1,0,0,1,1,3,3,3,3,3,3,
                3,3,3,3,3,3,2,1,1,1,1,1,1,1,1,1,1,0,1,1,1,3,3,3,3,3,3,
                3,3,3,3,3,3,3,2,2,2,1,1,1,1,1,1,0,1,1,2,3,3,3,3,3,3,3,
                3,3,3,3,3,3,3,3,3,2,2,1,1,1,1,1,1,1,1,3,3,3,3,3,3,3,3,
                3,3,3,3,1,1,1,3,3,3,2,2,2,2,2,1,1,1,3,3,3,3,4,4,4,4,4,
                3,3,3,3,3,1,1,3,3,3,3,3,2,2,2,1,1,1,3,4,4,4,4,4,5,5,5,
                3,3,3,3,3,3,3,3,3,3,3,3,3,3,2,1,1,1,4,4,4,4,5,5,5,5,5,
                3,3,3,3,3,3,3,3,3,3,3,3,3,3,2,1,1,1,4,4,5,5,5,5,5,5,5
            ];
        TileHeight[] heightmap = Array.ConvertAll(byte_heightmap, b => (TileHeight)b);
        List<Prop> lowProps = [
            new(new(12, 1), BridgeBaseTextureIndex + 0),
            new(new(12, 2), BridgeBaseTextureIndex + 2, Collision.Walkable),
            new(new(12, 3), BridgeBaseTextureIndex + 7, Collision.Walkable),
            new(new(12, 4), BridgeBaseTextureIndex + 8),

            new(new(5, 6), RockBaseTextureIndex + 0, Collision.Walkable),
            new(new(6, 6), RockBaseTextureIndex + 1, Collision.Walkable),
            new(new(7, 6), RockBaseTextureIndex + 2, Collision.Walkable),
            new(new(8, 6), RockBaseTextureIndex + 3, Collision.Walkable),
            new(new(9, 6), RockBaseTextureIndex + 4, Collision.Walkable),

            new(new(14, 13), RockBaseTextureIndex + 5, Collision.Walkable),
            new(new(15, 13), RockBaseTextureIndex + 6, Collision.Walkable, true),
            new(new(16, 13), RockBaseTextureIndex + 7, Collision.Walkable, true),

            new(new(20, 3), FlowerBaseTextureIndex + 0),
            new(new(21, 3), FlowerBaseTextureIndex + 1),
            new(new(22, 3), FlowerBaseTextureIndex + 2),
            new(new(23, 3), FlowerBaseTextureIndex + 3),
            new(new(20, 4), FlowerBaseTextureIndex + 4),
            new(new(21, 4), FlowerBaseTextureIndex + 5),
            new(new(22, 4), FlowerBaseTextureIndex + 6),
            new(new(23, 4), FlowerBaseTextureIndex + 7)
            ];

        // </temp map loading>

        acre = new Acre(
            new Point(0, 0), heightmap,
            lowProps, []
        );
    }

    static void RenderQuadrant(byte graphicIndex, int x, int y, int quadrant)
    {
        int quad_offset_x = (quadrant & 1) * 4;
        int quad_offset_y = (quadrant & 2) * 2;
        DrawTextureRec(Engine.atlasTexture,
            new Rectangle((Vector2)GraphicIndexQuadrantToPoint(graphicIndex, quadrant), 4, 4),
            new Vector2(x*8 + quad_offset_x, y*8 + quad_offset_y),
            Color.White
            );
    }

    static void RenderTile(TileGraphicIndices tileGraphicIndices, int x, int y)
    {
        if (tileGraphicIndices.topLeft == tileGraphicIndices.topRight &&
            tileGraphicIndices.topLeft == tileGraphicIndices.bottomLeft &&
            tileGraphicIndices.topLeft == tileGraphicIndices.bottomRight)
        {
            byte graphicIndex = tileGraphicIndices.topLeft;
            Rectangle source = new((Vector2)GraphicIndexToPoint(graphicIndex), 8, 8);
            DrawTextureRec(Engine.atlasTexture,
                source,
                new Vector2(x * 8, y * 8),
                Color.White
                );
        }
        else
        {
            RenderQuadrant(tileGraphicIndices.topLeft, x, y, 0);
            RenderQuadrant(tileGraphicIndices.topRight, x, y, 1);
            RenderQuadrant(tileGraphicIndices.bottomLeft, x, y, 2);
            RenderQuadrant(tileGraphicIndices.bottomRight, x, y, 3);
        }
    }

    public void RenderTilemap()
    {
        for (int x = 0; x<acreSize.width; x++){
            for (int y = 0; y < acreSize.height; y++)
            {
                RenderTile(acre.tilemap[x + y*acreSize.width], x, y);
            }
        }
    }

    public void RenderLowProps()
    {
        foreach (Prop prop in acre.lowProps)
        {
            prop.Render();
        }
    }

    public void RenderHighProps()
    {
        foreach (Prop prop in acre.highProps)
        {
            prop.Render();
        }
    }
}
