using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using static FishingGame.Utilities;

namespace FishingGame;


readonly struct Prop(Point location, NaturalRectangle graphicSource, CollisionType? collision = null, bool flippedX = false, float rotation = 0)
{ 
    public readonly Point Location = location; // location in pixel space
    readonly NaturalRectangle graphicSource = graphicSource;
    public readonly CollisionType? Collision = collision;
    readonly bool flippedX = flippedX;
    readonly float rotation = rotation;

    public Prop(Point position, byte graphicIndex, CollisionType? collision = null, bool flippedX = false, float rotation = 0) :
        this(position * TileSize, new NaturalRectangle(GraphicIndexToPoint(graphicIndex), TileSize), collision, flippedX, rotation)
    { }

    public void Render()
    {
        DrawTexturePro(
            Engine.AtlasTexture,
            flippedX? new((Vector2)graphicSource.Position, new(-graphicSource.Size.Width, graphicSource.Size.Height)) : (Rectangle)graphicSource,
            new Rectangle((Vector2)Location + new Vector2(graphicSource.Size.Width / 2f, graphicSource.Size.Height / 2f), (Vector2)graphicSource.Size),
            new(graphicSource.Size.Width / 2f, graphicSource.Size.Height / 2f),
            rotation,
            Color.White
            );
    }
}
