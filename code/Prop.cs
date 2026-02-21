using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using static FishingGame.AtlasUtilities;

namespace FishingGame;


readonly struct Prop(Point destination, NaturalRectangle graphicSource, Collision? collision = null, bool flippedX = false, float rotation = 0)
{ 
    readonly Point destination = destination;
    readonly NaturalRectangle graphicSource = graphicSource;
    readonly Collision? collision = collision;
    readonly bool flippedX = flippedX;
    readonly float rotation = rotation;
    readonly Vector2 origin = new(graphicSource.size.width/2f, graphicSource.size.height/2f);

    public Prop(Point position, byte graphicIndex, Collision? collision = null, bool flippedX = false, float rotation = 0) :
        this(position * 8, new NaturalRectangle(GraphicIndexToPoint(graphicIndex), new(8, 8)), collision, flippedX, rotation)
    { }

    public void Render()
    {
        DrawTexturePro(
            textureAtlas,
            flippedX? new((Vector2)graphicSource.position, new(-graphicSource.size.width, graphicSource.size.height)) : (Rectangle)graphicSource,
            new Rectangle((Vector2)destination + origin, (Vector2)graphicSource.size),
            origin,
            rotation,
            Color.White
            );
    }
}
