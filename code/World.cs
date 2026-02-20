using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using System.Diagnostics;

namespace FishingGame;

public class World
{
    readonly Page page;
    readonly Texture2D textureAtlas;

    public World()
    {
        // <temp heightmap loading>
        byte[] byte_heightmap = [
                5,5,5,5,5,5,5,4,4,4,4,3,3,3,3,3,1,1,3,3,4,4,4,4,5,5,
                5,5,5,5,5,5,4,4,4,4,4,3,3,3,3,2,1,1,3,3,4,4,4,5,5,5,
                5,5,5,5,1,4,4,2,2,3,3,3,3,2,2,2,1,1,3,3,3,1,4,4,5,5,
                5,4,4,1,1,3,3,3,2,3,1,1,1,1,1,1,1,1,3,3,3,3,1,4,5,4,
                4,4,1,1,3,3,3,3,1,1,1,1,1,1,1,1,0,1,1,3,3,3,3,3,3,3,
                3,3,3,3,3,3,3,1,1,1,1,2,2,2,1,1,0,1,1,3,3,3,3,3,3,3,
                3,3,3,3,3,3,1,1,1,1,2,2,3,3,3,1,0,0,1,3,3,3,3,3,3,3,
                3,3,3,3,3,1,1,1,1,1,2,3,3,3,3,1,0,0,1,3,3,3,3,3,3,3,
                3,3,3,3,3,1,1,1,1,1,1,3,3,3,3,1,1,0,1,1,3,3,3,3,3,3,
                3,3,3,3,3,1,1,1,1,1,1,3,3,3,1,1,0,0,1,1,3,3,3,3,3,3,
                3,3,3,3,3,2,1,1,1,1,1,1,1,1,1,1,0,1,1,1,3,3,3,3,3,3,
                3,3,3,3,3,3,2,2,2,1,1,1,1,1,1,0,1,1,2,3,3,3,3,3,3,3,
                3,3,3,3,3,3,3,3,2,2,1,1,1,1,1,1,1,1,3,3,3,3,3,3,3,3,
                3,3,3,1,1,1,3,3,3,2,2,2,2,2,1,1,1,3,3,3,3,4,4,4,4,4,
                3,3,3,3,1,1,3,3,3,3,3,2,2,2,1,1,1,3,4,4,4,4,4,5,5,5,
                3,3,3,3,3,3,3,3,3,3,3,3,3,2,1,1,1,4,4,4,4,5,5,5,5,5,
                3,3,3,3,3,3,3,3,3,3,3,3,3,2,1,1,1,4,4,5,5,5,5,5,5,5
            ];
        TileHeight[] heightmap = Array.ConvertAll(byte_heightmap, b => (TileHeight)b);
        // </temp heightmap loading>

        textureAtlas = LoadTexture("textures/atlas.png");

        page = new Page(
            new PagePosition(0, 0),
            new PageSize(24, 15),
            heightmap
        );
    }



    public void RenderPage()
    {
        // not yet implemented
        DrawTexture(textureAtlas, 0, 0, Color.White);
    }
}
