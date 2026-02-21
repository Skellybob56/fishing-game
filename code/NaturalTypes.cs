using Raylib_cs;
using System.Numerics;

namespace FishingGame;

public readonly struct Point(int x, int y)
{
    public readonly int x = x;
    public readonly int y = y;

    public static Point operator +(Point a, Point b)
    {
        return new(a.x + b.x, a.y + b.y);
    }
    public static Point operator -(Point a, Point b)
    {
        return new(a.x - b.x, a.y - b.y);
    }
    public static Point operator *(Point a, Point b)
    {
        return new(a.x * b.x, a.y * b.y);
    }
    public static Point operator /(Point a, Point b)
    {
        return new(a.x / b.x, a.y / b.y);
    }

    public static Point operator *(Point a, int b)
    {
        return new(a.x * b, a.y * b);
    }
    public static Point operator /(Point a, int b)
    {
        return new(a.x / b, a.y / b);
    }
    public static Point operator *(Point a, NaturalSize b)
    {
        return new(a.x * b.width, a.y * b.height);
    }

    public static explicit operator Vector2(Point a)
    {
        return new(a.x, a.y);
    }

}
public readonly struct NaturalSize
{
    public readonly int width;
    public readonly int height;
    public NaturalSize(int width, int height)
    {
        if (width <= 0)
        { throw new ArgumentOutOfRangeException(nameof(width), "Natural size must be greater than zero"); }
        this.width = width;

        if (height <= 0)
        { throw new ArgumentOutOfRangeException(nameof(height), "Natural size must be greater than zero"); }
        this.height = height;
    }

    public static NaturalSize operator *(NaturalSize a, int b)
    {
        return new(a.width * b, a.height * b);
    }
    public static NaturalSize operator /(NaturalSize a, int b)
    {
        return new(a.width / b, a.height / b);
    }

    public static explicit operator Vector2(NaturalSize a)
    {
        return new(a.width, a.height);
    }
    public static explicit operator Point(NaturalSize a)
    {
        return new(a.width, a.height);
    }
}

readonly struct NaturalRectangle(Point position, NaturalSize size)
{
    public readonly Point position = position;
    public readonly NaturalSize size = size;

    public NaturalRectangle(int x, int y, NaturalSize size) : this(new(x, y), size) { }

    public static explicit operator Rectangle(NaturalRectangle a)
    {
        return new((Vector2)a.position, (Vector2)a.size);
    }
}
