using Raylib_cs;
using System.Numerics;
using static FishingGame.Utilities;

namespace FishingGame;

partial class Player : Singleton<Player>
{
    void Rollover()
    {
        // todo: make rollover static and leave mutation of variables to fixed update
        // todo: make rollover start applying when the player stops actively moving in that axis.
        // cont. apply rollover gently to account for predicted player location if the player does not apply any movement on that axis

        // add or cancel rollover target for x
        if (velocity.X != 0) { rolloverTargetX = null; }
        else if (oldVelocity.X != 0)
        {
            float fract = fixedPosition.X - MathF.Truncate(fixedPosition.X);
            rolloverTargetX = (fract <= rolloverDeadzone || fract >= (1f - rolloverDeadzone))
                ? MathF.Round(fixedPosition.X)
                : (oldVelocity.X > 0 ? MathF.Ceiling(fixedPosition.X) : MathF.Floor(fixedPosition.X));
        }
        if (fixedPosition.X == rolloverTargetX) { rolloverTargetX = null; }

        // add or cancel rollover target for y
        if (velocity.Y != 0) { rolloverTargetY = null; }
        else if (oldVelocity.Y != 0)
        {
            float fract = fixedPosition.Y - MathF.Truncate(fixedPosition.Y);
            rolloverTargetY = (fract <= rolloverDeadzone || fract >= (1f - rolloverDeadzone))
                ? MathF.Round(fixedPosition.Y)
                : (oldVelocity.Y > 0 ? MathF.Ceiling(fixedPosition.Y) : MathF.Floor(fixedPosition.Y));
        }
        if (fixedPosition.Y == rolloverTargetY) { rolloverTargetY = null; }

        // apply rollover
        if (rolloverTargetX.HasValue)
        { displacement.X = MovementTowards(fixedPosition.X, rolloverTargetX.Value, rolloverSpeed); }
        if (rolloverTargetY.HasValue)
        { displacement.Y += MovementTowards(fixedPosition.Y, rolloverTargetY.Value, rolloverSpeed); }
    }

    void ApplyDisplacement()
    {
        static Vector2 ApplySubtickDisplacementNudge(Vector2 unnudgedSubtickDisplacement, AABBHit hit, Point closestTileHit, NaturalSize colliderSize)
        {
            // todo: only apply nudge if player is applying input towards the wall
            // todo: bug: sometimes nudge doesn't get the player entirely around the edge. if you let go of the action pushing you into the edge, you shift exactly two pixels back away from the corner - seems to be caused by interaction with Rollover()
            if (hit.tEdge < edgeBevelDepth || hit.tEdge > 1f - edgeBevelDepth)
            {
                bool horizontalCollision = hit.collisionNormal == CollisionNormal.Left ||
                    hit.collisionNormal == CollisionNormal.Right;

                int nudgeSign = hit.tEdge > 0.5f ? 1 : -1;
                int normalSign = hit.collisionNormal == CollisionNormal.Left ||
                    hit.collisionNormal == CollisionNormal.Up ? -1 : 1;

                Point firstSample = closestTileHit + // cardinal neighbour tile
                    (horizontalCollision ? new(0, nudgeSign) : new(nudgeSign, 0));
                Point secondSample = closestTileHit + // diagonal neighbour tile
                    (horizontalCollision ? new(normalSign, nudgeSign) : new(nudgeSign, normalSign));

                if (Engine.PointToCollision(firstSample) == CollisionType.Walkable &&
                    Engine.PointToCollision(secondSample) == CollisionType.Walkable)
                {
                    float tCorner = 0.5f - MathF.Abs(hit.tEdge - 0.5f);
                    // lerp nudge between zero when just barely on the bevel to the maximum when right at the edge
                    float bevelStrength = 1 - (tCorner / edgeBevelDepth);

                    // nudge axis calculations
                    float colliderLength = horizontalCollision ? colliderSize.height : colliderSize.width;
                    float tileSize = horizontalCollision ? TileSize.height : TileSize.width;
                    float edgePixelLength = tileSize + colliderLength;
                    float bevelPixelLength = edgePixelLength * edgeBevelDepth;
                    float edgeStart = (horizontalCollision ? closestTileHit.y : closestTileHit.x) * tileSize - colliderLength;

                    float cornerPixel = edgeStart + (nudgeSign > 0 ? edgePixelLength : 0f);
                    float intersectionPixel = (horizontalCollision ? hit.intersectionPoint.Y : hit.intersectionPoint.X) * tileSize;
                    float cornerDelta = cornerPixel - intersectionPixel;

                    float displacedPredictedPosition = intersectionPixel + (horizontalCollision ? unnudgedSubtickDisplacement.Y : unnudgedSubtickDisplacement.X);

                    // ensure that current displacement doesn't already round the corner
                    if (displacedPredictedPosition * nudgeSign < cornerPixel * nudgeSign)
                    {
                        // ensure nudge doesn't overshoot the corner
                        if (horizontalCollision)
                        {
                            return new(unnudgedSubtickDisplacement.X, unnudgedSubtickDisplacement.Y.MoveTowards(cornerDelta, maxNudgePortion * bevelStrength * bevelPixelLength));
                        }
                        return new(unnudgedSubtickDisplacement.X.MoveTowards(cornerDelta, maxNudgePortion * bevelStrength * bevelPixelLength), unnudgedSubtickDisplacement.Y);
                    }
                }
            }
            return unnudgedSubtickDisplacement;
        }

        if (displacement == Vector2.Zero) { return; }


        float remainingTime = 1f;
        Vector2 subtickDisplacement = displacement;

        (AABBHit closestAABBHit, Point closestTileHit)? closestHit;

        while (remainingTime > 0f) // while there is still time in the tick
        {
            float subtickTimeLength = remainingTime;

            // measure the tiles the player is colliding with excluding the displacement
            Rectangle currentCollider = new(
                (fixedPosition + collider.position) / TileSize,
                (Vector2)collider.size / (Vector2)TileSize
                );
            NaturalRectangle currentCollidingTiles = NaturalRectangle.ExpansiveRound(currentCollider, false); // use open intervals as the player is not on tiles they are touching

            // measure the tiles the player is colliding with including the displacement
            NaturalRectangle potentialCollidingTiles = NaturalRectangle.ExpansiveRound(
                currentCollider.GrowRectangle(subtickDisplacement / TileSize)
                );

            // if the potentialCollidingTiles set is equal to the currentCollidingTiles set then just add displacement and return
            if (currentCollidingTiles == potentialCollidingTiles)
            { fixedPosition += subtickDisplacement; return; }

            closestHit = null;
            // iterate on each potentialCollidingTile
            for (int tileX = potentialCollidingTiles.position.x; tileX < potentialCollidingTiles.size.width + potentialCollidingTiles.position.x; tileX++)
            {
                for (int tileY = potentialCollidingTiles.position.y; tileY < potentialCollidingTiles.size.height + potentialCollidingTiles.position.y; tileY++)
                {
                    // if tile is walkable then continue
                    if (Engine.PointToCollision(tileX, tileY) == CollisionType.Walkable) { continue; }

                    // get collision data
                    AABBHit? aabbHit = CollisionUtil.SweepBoxAgainstBox(currentCollider, // todo: do sweep in pixel space to simplify maths everywhere
                        subtickDisplacement / TileSize, new(tileX, tileY, 1, 1));

                    if (aabbHit != null) // if there is a collision
                    {
                        // set subtick displacement to bring the player just up to the tile
                        // shortening subtick displacment ensures that subsequent hits are only registered if even closer
                        subtickDisplacement = (aabbHit.Value.intersectionPoint - currentCollider.Position) * TileSize;
                        subtickTimeLength *= aabbHit.Value.timeTillCollision;
                        closestHit = new(aabbHit.Value, new(tileX, tileY)); // store intersection data
                    }
                }
            }
            if (!closestHit.HasValue) // no intersection
            { fixedPosition += subtickDisplacement; return; }

            AABBHit closestAABBHit = closestHit.Value.closestAABBHit;
            Point closestTileHit = closestHit.Value.closestTileHit;

            // reduce time by subtickTimeLength
            remainingTime -= subtickTimeLength;

            // apply normal to displacement
            displacement = displacement.ApplyNormal(closestAABBHit.collisionNormal);

            // apply normal partially to velocity
            velocity = velocity.MoveTowards(velocity.ApplyNormal(closestAABBHit.collisionNormal), collisionVelocityCost);

            // move to collision intersection
            fixedPosition = closestAABBHit.intersectionPoint * TileSize - collider.position;

            // use the initial displacement to produce new displacement
            subtickDisplacement = displacement * remainingTime;

            // check and apply nudge to round corner if needed (this can only happen once per edge per frame as displacement into a tile is entirely stopped on collision)
            subtickDisplacement = ApplySubtickDisplacementNudge(subtickDisplacement, closestAABBHit, closestTileHit, collider.size);
        }
    }
}
