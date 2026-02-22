using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using static FishingGame.Utilities;

namespace FishingGame;


readonly struct Prop(Point location, NaturalRectangle graphicSource, Collision? collision = null, bool flippedX = false, float rotation = 0)
{ 
    public readonly Point location = location; // location in pixel space
    readonly NaturalRectangle graphicSource = graphicSource;
    public readonly Collision? collision = collision;
    readonly bool flippedX = flippedX;
    readonly float rotation = rotation;

    public Prop(Point position, byte graphicIndex, Collision? collision = null, bool flippedX = false, float rotation = 0) :
        this(position * TileSize, new NaturalRectangle(GraphicIndexToPoint(graphicIndex), TileSize), collision, flippedX, rotation)
    { }

    public void Render()
    {
        DrawTexturePro(
            Engine.atlasTexture,
            flippedX? new((Vector2)graphicSource.position, new(-graphicSource.size.width, graphicSource.size.height)) : (Rectangle)graphicSource,
            new Rectangle((Vector2)location + new Vector2(graphicSource.size.width / 2f, graphicSource.size.height / 2f), (Vector2)graphicSource.size),
            new(graphicSource.size.width / 2f, graphicSource.size.height / 2f),
            rotation,
            Color.White
            );
    }
}
