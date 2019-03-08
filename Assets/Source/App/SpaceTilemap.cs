using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class SpaceTilemap : MonoBehaviour
{
    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private Tile[] tiles;

    private Grid grid;

    private Tilemap tilemap;

    private TilemapRenderer tilemapRenderer;

    private float pixelsPerUnit;

    private int rowsCount;

    private Vector3Int cameraTilePosition;

    public Camera Camera { get => camera; set => camera = value; }

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
        grid = tilemap.layoutGrid;
        cameraTilePosition = tilemap.WorldToCell(camera.transform.position);
        FillMapInView();
    }

    private Tile GetRandomTile(List<int> tileIndexPool)
    {
        if (tileIndexPool.Count < 1)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tileIndexPool.Add(i);
            }
        }
        int randIndex = UnityEngine.Random.Range(0, tileIndexPool.Count);
        int tileIndex = tileIndexPool[randIndex];
        tileIndexPool.RemoveAt(randIndex);
        return tiles[tileIndex];
    }

    private void GetHalfRowsAndColsCount(out int halfYCount, out int halfXCount)
    {
        halfYCount = (int)(camera.orthographicSize / grid.cellSize.y);
        halfXCount = (halfYCount * Screen.width / Screen.height) + 2;
        halfYCount += 1;
    }

    private void FillMapInView()
    {
        List<int> tileIndexPool = new List<int>();
        GetHalfRowsAndColsCount(out int halfYCount, out int halfXCount);
        Vector3Int camCurrTilePos = tilemap.WorldToCell(camera.transform.position);
        tilemap.ClearAllTiles();
        for (int y = -halfYCount; y <= halfYCount; y++)
        {
            for (int x = -halfXCount; x <= halfXCount; x++)
            {
                Tile tile = Instantiate(GetRandomTile(tileIndexPool));
                tile.transform = Matrix4x4.Rotate(Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 4) * 90));
                tilemap.SetTile(new Vector3Int(camCurrTilePos.x + x, camCurrTilePos.y + y, 0), tile);
            }
        }
    }

    private void LateUpdate()
    {
        Vector3Int camPrevTilePos = cameraTilePosition;
        Vector3Int camCurrTilePos = tilemap.WorldToCell(camera.transform.position);
        camCurrTilePos.z = 0;
        if (camPrevTilePos != camCurrTilePos)
        {
            List<int> tileIndexPool = new List<int>();
            GetHalfRowsAndColsCount(out int halfYCount, out int halfXCount);
            int deltaY = camCurrTilePos.y - camPrevTilePos.y;
            int deltaX = camCurrTilePos.x - camPrevTilePos.x;
            Tile[,] viewTiles = new Tile[2 * halfXCount + 1, 2 * halfYCount + 1];
            for (int y = -halfYCount; y <= halfYCount; y++)
            {
                int tileY = y + halfYCount;
                for (int x = -halfXCount; x <= halfXCount; x++)
                {
                    int tileX = x + halfXCount;
                    viewTiles[tileX, tileY] = tilemap.GetTile<Tile>(new Vector3Int(camCurrTilePos.x + x, camCurrTilePos.y + y, 0));
                }
            }
            if (deltaY != 0)
            {
                int begY = deltaY > 0 ? Math.Max(-halfYCount, halfYCount - deltaY + 1) : -halfYCount;
                int endY = Math.Min(halfYCount, begY + Math.Abs(deltaY) - 1);
                for (int y = begY; y <= endY; y++)
                {
                    int tileY = y + halfYCount;
                    for (int x = -halfXCount; x <= halfXCount; x++)
                    {
                        int tileX = x + halfXCount;
                        Tile tile = viewTiles[tileX, tileY] = Instantiate(GetRandomTile(tileIndexPool));
                        tile.transform = Matrix4x4.Rotate(Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 4) * 90));
                    }
                }
            }
            if (deltaX != 0)
            {
                int begX = deltaX > 0 ? Math.Max(-halfXCount, halfXCount - deltaX + 1) : -halfXCount;
                int endX = Math.Min(halfXCount, begX + Math.Abs(deltaX) - 1);
                for (int y = -halfYCount; y <= halfYCount; y++)
                {
                    int tileY = y + halfYCount;
                    for (int x = begX; x <= endX; x++)
                    {
                        int tileX = x + halfXCount;
                        Tile tile = viewTiles[tileX, tileY] = Instantiate(GetRandomTile(tileIndexPool));
                        tile.transform = Matrix4x4.Rotate(Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 4) * 90));
                    }
                }
            }
            tilemap.ClearAllTiles();
            for (int y = -halfYCount; y <= halfYCount; y++)
            {
                int tileY = y + halfYCount;
                for (int x = -halfXCount; x <= halfXCount; x++)
                {
                    int tileX = x + halfXCount;
                    tilemap.SetTile(new Vector3Int(camCurrTilePos.x + x, camCurrTilePos.y + y, 0), viewTiles[tileX, tileY]);
                }
            }
        }
        cameraTilePosition = camCurrTilePos;
        //Debug.Log(GetType() + ".cameraTilePosition="+cameraTilePosition);
        //Debug.Log(GetType() + "."+tilemap.GetTile(cameraTilePosition));
        //Debug.LogWarning(GetType() + "." + tilemap.WorldToCell(camera.transform.position) + " " + grid.cellSize + " " + tilemap.cellSize + " " + Screen.height);

    }
}
