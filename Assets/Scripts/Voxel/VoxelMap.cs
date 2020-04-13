using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for controlling chunking
/// </summary>
public class VoxelMap : MonoBehaviour
{

    public float size = 2f;

    public int voxelResolution = 8; // Size of the individual maps maybe?
    public int chunkResolution = 2;

    public VoxelGrid voxelGridPrefab;

    private VoxelGrid[] chunks;

    float chunkSize, voxelSize, halfSize;

    private void Awake()
    {
        halfSize = size * .5f;
        chunkSize = size / chunkResolution;
        voxelSize = chunkSize / voxelResolution;

        var box = gameObject.AddComponent<BoxCollider>(); // add 3d box collider so we can detect input (3d so its portable)    
        box.size = new Vector3(size, size);


        chunks = new VoxelGrid[chunkResolution * chunkResolution];
        int i = 0;
        for (int y = 0; y < chunkResolution; y++)
        {
            for (int x = 0; x < chunkResolution; x++)
            {
                CreateChunk(i++, x, y);
            }
        }
    }

    private void CreateChunk(int i, int x, int y)
    {
        VoxelGrid chunk = Instantiate(voxelGridPrefab) as VoxelGrid;
        chunk.Initialize(voxelResolution, chunkSize);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
        chunks[i] = chunk;
    }


    public void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo))
            {
                if (hitInfo.collider.gameObject == gameObject)
                {
                    EditVoxels(transform.InverseTransformPoint(hitInfo.point));
                }

            }

        }
    }

    private void EditVoxels(Vector3 point)
    {
        int voxelCenterX = (int)((point.x + halfSize) / voxelSize); // correct it so bottom left is origin.
        int voxelCenterY = (int)((point.y + halfSize) / voxelSize);

        int xStart = (voxelCenterX - radiusIndex) / voxelResolution;
        if (xStart < 0)
        {
            xStart = 0;
        }
        int xEnd = (voxelCenterX + radiusIndex) / voxelResolution;
        if (xEnd >= chunkResolution)
        {
            xEnd = chunkResolution - 1;
        }
        int yStart = (voxelCenterY - radiusIndex) / voxelResolution;
        if (yStart < 0)
        {
            yStart = 0;
        }
        int yEnd = (voxelCenterY + radiusIndex) / voxelResolution;
        if (yEnd >= chunkResolution)
        {
            yEnd = chunkResolution - 1;
        }

        VoxelStencil activeStencil = stencils[stencilIndex];
        activeStencil.Initialize(fillTypeIndex == 0, radiusIndex);

        int voxelYOffset = yStart * voxelResolution;
        for (int y = yStart; y <= yEnd; y++)
        {
            int i = y * chunkResolution + xStart;
            int voxelXOffset = xStart * voxelResolution;
            for (int x = xStart; x <= xEnd; x++, i++)
            {
                activeStencil.SetCenter(voxelCenterX - voxelXOffset, voxelCenterY - voxelYOffset);
                chunks[i].Apply(activeStencil);
                voxelXOffset += voxelResolution;
            }
            voxelYOffset += voxelResolution;
        }
    }

    //OnGui shit... Should propably refactor...
    private static string[] radiusNames = { "0", "1", "2", "3", "4", "5" };

    private static string[] fillTypeNames = { "Filled", "Empty" };

    private static string[] stencilNames = { "Square", "Circle" };

    private int stencilIndex;


    private int fillTypeIndex, radiusIndex;

    private VoxelStencil[] stencils = {
        new VoxelStencil(),
        new VoxelStencilCircle()
    };

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
        GUILayout.Label("Fill Type");
        fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 2);

        GUILayout.Label("Radius");
        radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);

        GUILayout.Label("Stencil");
        stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
        GUILayout.EndArea();
    }
}
