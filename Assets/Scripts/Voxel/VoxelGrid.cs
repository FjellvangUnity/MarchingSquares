using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class VoxelGrid : MonoBehaviour
{
    public GameObject voxelPrefab;

    public int resolution;

    private Voxel[] voxels; 
    private float voxelSize, gridSize;

    private Voxel dummyX, dummyY, dummyT;
    //Left right and corner neighbor
    public VoxelGrid xNeighbor, yNeighbor, xyNeighbor; 

    private Material[] voxelMaterials;


    private Mesh mesh;

    private List<Vector3> vertices;
    private List<int> triangles;

    public void Initialize(int resolution, float size)
    {
        this.resolution = resolution;
        voxelSize = size / this.resolution;
        voxels = new Voxel[this.resolution * this.resolution];

        gridSize = size;
        voxelMaterials = new Material[voxels.Length];

        int i = 0;
        for (int y = 0; y < this.resolution; y++)
        {
            for (int x = 0; x < this.resolution; x++)
            {
                CreateVoxel(i++, x, y);
            }
        }
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "VoxelGrid Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();

        dummyX = new Voxel();
        dummyY = new Voxel();
        dummyT = new Voxel();

        Refresh();
    }



    private void CreateVoxel(int i, int x, int y)
    {
        GameObject o = Instantiate(voxelPrefab) as GameObject;
        o.transform.parent = transform;
        o.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize, -0.01f);
        o.transform.localScale = Vector3.one * voxelSize * .1f;

        voxels[i] = new Voxel(x, y, voxelSize);
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
                voxels[i].state = stencil.Apply(x, y, voxels[i].state);
            }
        }
        Refresh();
    }


    private void Refresh()
    {
        SetVoxelColors();
        Triangulate();
    }

    private void Triangulate()
    {
        vertices.Clear(); //TODO: could be a place of optimization. using fixed size arrays instead of lists?
        triangles.Clear();
        mesh.Clear();

        if (xNeighbor != null)
        {
            dummyX.BecomeXDummyOf(xNeighbor.voxels[0], gridSize);
        }
        TriangulateCellRows();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray(); 
    }

    private void TriangulateCellRows()
    {
        int cells = resolution - 1;
        for (int i = 0, y = 0; y < cells; y++, i++)
        {
            for (int x = 0; x < cells; x++, i++)
            {
                TriangulateCell(
                    voxels[i], 
                    voxels[i+1],
                    voxels[i+resolution],
                    voxels[i+resolution+1]
                    );
            }
            if (xNeighbor != null)
            {
                TriangulateGapCell(i);
            }

        }
    }

    private void TriangulateGapCell(int i)
    {
        Voxel dummySwap = dummyT;
        dummySwap.BecomeXDummyOf(xNeighbor.voxels[i + 1], gridSize);
        dummyT = dummyX;
        dummyX = dummySwap;
        TriangulateCell(voxels[i], dummyT, voxels[i + resolution], dummyX);
    }

    private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        int celltype = 0;
        if (a.state)
        {
            celltype |= 1;
        }
        if (b.state)
        {
            celltype |= 2;
        }
        if (c.state)
        {
            celltype |= 4;
        }
        if (d.state)
        {
            celltype |= 8;
        }

        switch (celltype)
        {
            case 0:
                return;
            case 1:
                AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                break;
            case 2:
                AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                break;
            case 4:
                AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                break;
            case 8:
                AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                break;
            case 3:
                AddQuad(a.position, a.yEdgePosition, b.yEdgePosition, b.position);
                break;
            case 5:
                AddQuad(a.position, c.position, c.xEdgePosition, a.xEdgePosition);
                break;
            case 10:
                AddQuad(a.xEdgePosition, c.xEdgePosition, d.position, b.position);
                break;
            case 12:
                AddQuad(a.yEdgePosition, c.position, d.position, b.yEdgePosition);
                break;
            case 15:
                AddQuad(a.position, c.position, d.position, b.position);
                break;
            case 7:
                AddPentagon(a.position, c.position, c.xEdgePosition, b.yEdgePosition, b.position);
                break;
            case 11:
                AddPentagon(b.position, a.position, a.yEdgePosition, c.xEdgePosition, d.position);
                break;
            case 13:
                AddPentagon(c.position, d.position, b.yEdgePosition, a.xEdgePosition, a.position);
                break;
            case 14:
                AddPentagon(d.position, b.position, a.xEdgePosition, a.yEdgePosition, c.position);
                break;
            case 6:
                AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                break;
            case 9:
                AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                break;
            default:
                break;
        }
    }

    private void AddTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        var vertexIndex = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }
    private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }
    private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        vertices.Add(e);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 3);
        triangles.Add(vertexIndex + 4);
    }

    private void SetVoxelColors()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            voxelMaterials[i].color = voxels[i].state ? Color.black : Color.white;
        }
    }
}
