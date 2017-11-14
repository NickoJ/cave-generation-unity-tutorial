using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{

	public SquareGrid grid;

	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();

	public void GenerateMesh(int[,] map, float squareSize)
	{
		grid = new SquareGrid(map, squareSize);

		vertices.Clear();
		triangles.Clear();

		for (int x = 0; x < grid.squares.GetLength(0); x++)
		{
			for (int y = 0; y < grid.squares.GetLength(1); y++)
			{
				TriangulateSquares(grid.squares[x,y]);
			}
		}

		var mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
	}

	private void TriangulateSquares(Square square)
	{
		switch(square.configuration)
		{
			case 0: break;
			// 1 points
			case 1: MeshFromPoints(square.midBottom, square.bottomLeft, square.midLeft); break;
			case 2: MeshFromPoints(square.midRight, square.bottomRight, square.midBottom); break;
			case 4: MeshFromPoints(square.midTop, square.topRight, square.midRight); break;
			case 8: MeshFromPoints(square.topLeft, square.midTop, square.midLeft); break;

			// 2 points
			case 3: MeshFromPoints(square.midRight, square.bottomRight, square.bottomLeft, square.midLeft); break;
			case 6: MeshFromPoints(square.midTop, square.topRight, square.bottomRight, square.midBottom); break;
			case 9: MeshFromPoints(square.topLeft, square.midTop, square.midBottom, square.bottomLeft); break;
			case 12: MeshFromPoints(square.topLeft, square.topRight, square.midRight, square.midLeft); break;
			case 5: MeshFromPoints(square.midTop, square.topRight, square.midRight, square.midBottom, square.bottomLeft, square.midLeft); break;
			case 10: MeshFromPoints(square.topLeft, square.midTop, square.midRight, square.bottomRight, square.midBottom, square.midLeft); break;

			// 3 points
			case 7: MeshFromPoints(square.midTop, square.topRight, square.bottomRight, square.bottomLeft, square.midLeft); break;
			case 11: MeshFromPoints(square.topLeft, square.midTop, square.midRight, square.bottomRight, square.bottomLeft); break;
			case 13: MeshFromPoints(square.topLeft, square.topRight, square.midRight, square.midBottom, square.bottomLeft); break;
			case 14: MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.midBottom, square.midLeft); break;

			// 4 points
			case 15: MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft); break;
		}
	}

	private void MeshFromPoints(params Node[] points)
	{
		AssignVertices(points);

		if (points.Length >= 3) CreateTriangle(points[0], points[1], points[2]);
		if (points.Length >= 4) CreateTriangle(points[0], points[2], points[3]);
		if (points.Length >= 5) CreateTriangle(points[0], points[3], points[4]);
		if (points.Length >= 6) CreateTriangle(points[0], points[4], points[5]);
	}

	private void AssignVertices(Node[] points)
	{
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i].vertexIndex >= 0) continue;

			points[i].vertexIndex = vertices.Count;
			vertices.Add(points[i].position);
		}
	}

	private void CreateTriangle(Node a, Node b, Node c)
	{
		triangles.Add(a.vertexIndex);
		triangles.Add(b.vertexIndex);
		triangles.Add(c.vertexIndex);
	}

	private void OnDrawGizmos()
	{
		// if (grid == null) return;

		// for (int x = 0; x < grid.squares.GetLength(0); ++x)
		// {
		// 	for (int y = 0; y < grid.squares.GetLength(1); ++y)
		// 	{
		// 		Gizmos.color = grid.squares[x,y].topLeft.active ? Color.black : Color.white;
		// 		Gizmos.DrawCube(grid.squares[x,y].topLeft.position, Vector3.one * 0.4f);

		// 		Gizmos.color = grid.squares[x,y].topRight.active ? Color.black : Color.white;
		// 		Gizmos.DrawCube(grid.squares[x,y].topRight.position, Vector3.one * 0.4f);

		// 		Gizmos.color = grid.squares[x,y].bottomRight.active ? Color.black : Color.white;
		// 		Gizmos.DrawCube(grid.squares[x,y].bottomRight.position, Vector3.one * 0.4f);

		// 		Gizmos.color = grid.squares[x,y].bottomLeft.active ? Color.black : Color.white;
		// 		Gizmos.DrawCube(grid.squares[x,y].bottomLeft.position, Vector3.one * 0.4f);

		// 		Gizmos.color = Color.gray;
		// 		Gizmos.DrawCube(grid.squares[x,y].midTop.position, Vector3.one * 0.15f);
		// 		Gizmos.DrawCube(grid.squares[x,y].midRight.position, Vector3.one * 0.15f);
		// 		Gizmos.DrawCube(grid.squares[x,y].midBottom.position, Vector3.one * 0.15f);
		// 		Gizmos.DrawCube(grid.squares[x,y].midLeft.position, Vector3.one * 0.15f);
		// 	}
		// }
	}

	public class SquareGrid
	{

		public Square[,] squares;

		public SquareGrid(int[,] map, float squareSize)
		{
			int ncX = map.GetLength(0);
			int ncY = map.GetLength(1);

			float mapWidth = ncX * squareSize;
			float mapHeight = ncY * squareSize;

			var controlNodes = new ControlNode[ncX, ncY];

			for (int x = 0; x < ncX; ++x)
			{
				for (int y = 0; y < ncY; ++y)
				{
					var pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
					controlNodes[x, y] = new ControlNode(pos, map[x,y] == 1, squareSize);
				}
			}

			squares = new Square[ncX - 1, ncY - 1];
			for (int x = 0; x < ncX - 1; ++x)
			{
				for (int y = 0; y < ncY - 1; ++y)
				{
					squares[x,y] = new Square(controlNodes[x,y+1], controlNodes[x+1,y+1], controlNodes[x+1,y], controlNodes[x,y]);
				}
			}
		}

	}

	public class Square
	{
		
		public ControlNode topLeft;
		public ControlNode topRight;
		public ControlNode bottomRight;
		public ControlNode bottomLeft;

		public Node midTop;
		public Node midRight;
		public Node midBottom;
		public Node midLeft;

		public int configuration;

		public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
		{
			this.topLeft = topLeft;
			this.topRight = topRight;
			this.bottomRight = bottomRight;
			this.bottomLeft = bottomLeft;

			midTop = topLeft.right;
			midRight = bottomRight.above;
			midBottom = bottomLeft.right;
			midLeft = bottomLeft.above;

			if (topLeft.active) configuration = configuration + 8;
			if (topRight.active) configuration = configuration + 4;
			if (bottomRight.active) configuration = configuration + 2;
			if (bottomLeft.active) configuration = configuration + 1;
		}

	}

	public class Node
	{

		public Vector3 position;
		public int vertexIndex = -1;

		public Node(Vector3 pos)
		{
			position = pos;
		}

	}

	public class ControlNode : Node
	{

		public bool active;
		public Node above;
		public Node right;

		public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos)
		{
			this.active = active;
			above = new Node(position + Vector3.forward * squareSize / 2);
			right = new Node(position + Vector3.right * squareSize / 2);
		}

	}

}