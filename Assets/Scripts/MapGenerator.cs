using System;
using UnityEngine;

using SysRandom = System.Random;

public class MapGenerator : MonoBehaviour 
{

	[Range(0, 100)] public int mapFillPercent = 50;
	public int width = 60;
	public int height = 80;
	public int smoothTimes = 5;

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
	}

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

				if (nX >= 0 && nX < width && nY >= 0 && nY < height) count += map[nX, nY];
				else count += 1;
			}
		}
		return count;
	}

	private void OnDrawGizmos()
	{
		if (map == null) return;

		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				Gizmos.color = (map[x,y] == 1) ? Color.black : Color.white;
				var pos = new Vector3(-width/2 + x + 0.5f, 0, -height/2 + y + 0.5f);
				Gizmos.DrawCube(pos, Vector3.one);
			}
		}
	}

}