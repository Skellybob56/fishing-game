using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System.Diagnostics;

namespace FishingGame;

public class World
{
    readonly Acre acre;
    readonly AcreSize acreSize;
    readonly Texture2D textureAtlas;

    public World()
    {
        // <temp heightmap loading>
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
        // </temp heightmap loading>

        textureAtlas = LoadTexture("textures/atlas.png");

        acreSize = new AcreSize(25, 15);
        acre = new Acre(
            new AcrePosition(0, 0),
            acreSize, heightmap
        );
    }

    void RenderQuadrant(byte graphicIndex, int x, int y, int quadrant)
    {
        int quad_offset_x = (quadrant & 1) * 4;
        int quad_offset_y = (quadrant & 2) * 2;
        DrawTextureRec(textureAtlas,
            new Rectangle((graphicIndex & 0x0f) * 8 + quad_offset_x,
                (graphicIndex & 0xf0) / 2 + quad_offset_y, 4, 4),
            new Vector2(x*8 + quad_offset_x, y*8 + quad_offset_y),
            Color.White
            );
    }

    void RenderTile(TileGraphicIndices tileGraphicIndices, int x, int y)
    {
        if (tileGraphicIndices.topLeft == tileGraphicIndices.topRight &&
            tileGraphicIndices.topLeft == tileGraphicIndices.bottomLeft &&
            tileGraphicIndices.topLeft == tileGraphicIndices.bottomRight)
        {
            byte graphicIndex = tileGraphicIndices.topLeft;
            Rectangle source = new Rectangle((graphicIndex & 0x0f) * 8,
                    (graphicIndex & 0xf0) / 2, 8, 8);
            DrawTextureRec(textureAtlas,
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

    public void RenderAcre()
    {
        for (int x = 0; x<acreSize.width; x++){
            for (int y = 0; y < acreSize.height; y++)
            {
                RenderTile(acre.tilemap[x + y*acreSize.width], x, y);
            }
        }
    }
}
