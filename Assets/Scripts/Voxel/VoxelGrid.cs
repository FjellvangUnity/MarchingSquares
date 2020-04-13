using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class VoxelGrid : MonoBehaviour
{
    public GameObject voxelPrefab;

    public int resolution;

    private bool[] voxels; //TODO: Could but set as uints ? not sure if faster. should be more memory effecient...
    private float voxelSize;

    private Material[] voxelMaterials;

    public void Initialize(int resolution, float size)
    {
        this.resolution = resolution;
        voxelSize = size / this.resolution;
        voxels = new bool[this.resolution * this.resolution];

        voxelMaterials = new Material[voxels.Length];

        int i = 0;
        for (int y = 0; y < this.resolution; y++)
        {
            for (int x = 0; x < this.resolution; x++)
            {
                CreateVoxel(i++, x, y);
            }
        }
        SetVoxelColors();
    }

    private void CreateVoxel(int i, int x, int y)
    {
        GameObject o = Instantiate(voxelPrefab) as GameObject;
        o.transform.parent = transform;
        o.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize);
        o.transform.localScale = Vector3.one * voxelSize;

        //save the material for coloring..
        voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
    }
    public void Apply(VoxelStencil stencil)
    {
        int xStart = stencil.XStart;
        if (xStart < 0)
        {
            xStart = 0;
        }
        int xEnd = stencil.XEnd;
        if (xEnd >= resolution)
        {
            xEnd = resolution - 1;
        }
        int yStart = stencil.YStart;
        if (yStart < 0)
        {
            yStart = 0;
        }
        int yEnd = stencil.YEnd;
        if (yEnd >= resolution)
        {
            yEnd = resolution - 1;
        }
        for (int y = yStart; y <= yEnd; y++)
        {
            int i = y * resolution + xStart;
            for (int x = xStart; x <= xEnd; x++, i++)
            {
                voxels[i] = stencil.Apply(x, y, voxels[i]);
            }
        }
        SetVoxelColors(); // sets all the voxels.. apprently theres a reason for the madness
    }

    [Obsolete]
    public void SetVoxel(int x, int y, bool state)
    {
        voxels[x * resolution + y] = state;

        SetVoxelColors(); // sets all the voxels.. apprently theres a reason for the madness
    }

    private void SetVoxelColors()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            voxelMaterials[i].color = voxels[i] ? Color.black : Color.white;
        }
    }
}
