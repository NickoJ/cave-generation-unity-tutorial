using System;
using System.Collections.Generic;
using UnityEngine;

using SysRandom = System.Random;

public class MapGenerator : MonoBehaviour 
{

	[Range(0, 100)] public int mapFillPercent = 50;
	public int width = 60;
	public int height = 80;
	public int smoothTimes = 5;
	public int borderSize = 5;
	public int wallThresholdShize = 50;
	public int roomThresholdSize = 50;

	public string seed = string.Empty;
	public bool useRandomSeed;

	private int[,] map;

	private void Start()
	{
		GenerateMap();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0)) GenerateMap();
	}

	private void GenerateMap()
	{
		map = new int[width, height];
		FillMap();

		for (int i = 0; i < smoothTimes; ++i) SmoothMap();

		ProcessMap();

		int[,] borderedMap = new int [width + borderSize * 2, height + borderSize * 2];
		for (int x = 0; x < borderedMap.GetLength(0); x++)
		{
			for (int y = 0; y < borderedMap.GetLength(1); y++)
			{
				if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
					borderedMap[x, y] = map[x - borderSize, y - borderSize];
				}
				else
				{
					borderedMap[x, y] = 1;
				}
			}
		}
		map = borderedMap;

		MeshGenerator gen = GetComponent<MeshGenerator>();
		gen.GenerateMesh(map, 1);
	}

	private void ProcessMap()
	{
		List<List<Coord>> wallRegions = GetRegions(1);
		foreach(var wallRegion in wallRegions)
		{
			if (wallRegion.Count >= wallThresholdShize) continue;
			foreach(var tile in wallRegion) map[tile.x, tile.y] = 0;
		}

		List<List<Coord>> roomRegions = GetRegions(0);
		foreach(var roomRegion in roomRegions)
		{
			if (roomRegion.Count >= roomThresholdSize) continue;
			foreach(var tile in roomRegion) map[tile.x, tile.y] = 1;
		}		
	}

	private List<List<Coord>> GetRegions(int tileType)
	{
		List<List<Coord>> regions = new List<List<Coord>>();
		var mapFlags = new int[width, height];

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (mapFlags[x,y] == 0 && map[x,y] == tileType)
				{
					var newRegion = GetRegionTiles(x, y);
					regions.Add(newRegion);

					foreach(var tile in newRegion) mapFlags[tile.x, tile.y] = 1;
				}
			}
		}

		return regions;
	}

	private List<Coord> GetRegionTiles(int startX, int startY)
	{
		var tiles = new List<Coord>();
		var mapFlags = new int[width, height];
		int tileType = map[startX, startY];

		var queue = new Queue<Coord>();
		queue.Enqueue(new Coord(startX, startY));
		mapFlags[startX, startY] = 1;

		while(queue.Count > 0)
		{
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			for (int x = tile.x - 1; x <= tile.x + 1; x++)
			{
				for (int y = tile.y - 1; x <= tile.y + 1; y++)
				{
					if (!IsInMapRange(x, y) || (y != tile.y  && x != tile.x)) continue;
					if (mapFlags[x,y] != 0 || map[x,y] != tileType) continue;
					mapFlags[x,y] = 1;
					queue.Enqueue(new Coord(x,y));
				}
			}
		}
		return tiles;
	}

	private bool IsInMapRange(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

	private void FillMap()
	{
		if (useRandomSeed) seed = DateTime.UtcNow.ToString();

		SysRandom rand = new SysRandom(seed.GetHashCode()); 

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1) map[x,y] = 1;
				else map[x,y] = (rand.Next(0, 100) < mapFillPercent) ? 1 : 0;
			}
		}
	}

	private void SmoothMap()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				int neighbourWalls = GetSurroundingWallCount(x, y);

				if (neighbourWalls > 4) map[x, y] = 1;
				else if (neighbourWalls < 4) map[x, y] = 0;
			}
		}
	}

	private int GetSurroundingWallCount(int x, int y)
	{
		int count = 0;
		for (int nX = x - 1; nX <= x + 1; ++nX)
		{
			for (int nY = y - 1; nY <= y + 1; ++nY)
			{
				if (nX == x && nY == y) continue;

				if (IsInMapRange(nX, nY)) count += map[nX, nY];
				else count += 1;
			}
		}
		return count;
	}

	private struct Coord
	{
		public int x;
		public int y;

		public Coord(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

	}

}