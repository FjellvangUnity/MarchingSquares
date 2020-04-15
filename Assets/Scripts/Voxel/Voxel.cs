using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Voxel 
{
    public bool state;

    public Vector2 position, xEdgePosition, yEdgePosition;

    public Voxel(int x, int y, float size)
    {
        position.x = (x + 0.5f) * size;
        position.y = (y + 0.5f) * size;

        var halfLength = size * .5f;
        xEdgePosition = position;
        xEdgePosition.x += halfLength;

        yEdgePosition = position;
        yEdgePosition.y += halfLength;
            
        state = false;
    }

    public void BecomeXDummyOf(Voxel voxel, float offset)
    {
        state = voxel.state;
        position = voxel.position;
        xEdgePosition = voxel.xEdgePosition;
        yEdgePosition = voxel.yEdgePosition;
        position.x += offset;
        xEdgePosition.x += offset;
        yEdgePosition.x += offset;
    }
}
