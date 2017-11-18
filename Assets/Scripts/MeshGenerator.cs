using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{

	public SquareGrid grid;
	public float wallHeight = 5;
	public MeshFilter wallsFilter;

	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();

	private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
	private List<List<int>> outlines = new List<List<int>>();
	private HashSet<int> checkedVertices = new HashSet<int>();

	public void GenerateMesh(int[,] map, float squareSize)
	{
		outlines.Clear();
		checkedVertices.Clear();
		vertices.Clear();
		triangles.Clear();
		triangleDictionary.Clear();

		grid = new SquareGrid(map, squareSize);

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

		CreateWallMesh();
	}

	private void CreateWallMesh()
	{
		CalculateMeshOutlines();

		var wallVertices = new List<Vector3>();
		var wallTriangles = new List<int>();
		var wallMesh = new Mesh();

		foreach(List<int> outline in outlines)
		{
			for (int i = 0; i < outline.Count - 1; i++)
			{
				int startIndex = wallVertices.Count;
				wallVertices.Add(vertices[outline[i]]); //left
				wallVertices.Add(vertices[outline[i + 1]]); //right
				wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); //bottom left
				wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); //bottom right

				wallTriangles.Add(startIndex + 0);
				wallTriangles.Add(startIndex + 2);
				wallTriangles.Add(startIndex + 3);

				wallTriangles.Add(startIndex + 3);
				wallTriangles.Add(startIndex + 1);
				wallTriangles.Add(startIndex + 0);
			}
		}
		wallMesh.vertices = wallVertices.ToArray();
		wallMesh.triangles = wallTriangles.ToArray();
		wallsFilter.mesh = wallMesh;
	}

	private void TriangulateSquares(Square square)
	{
        switch (square.configuration) {
        case 0: break;

        // 1 points:
        case 1: MeshFromPoints(square.midLeft, square.midBottom, square.bottomLeft); break;
        case 2: MeshFromPoints(square.bottomRight, square.midBottom, square.midRight); break;
        case 4: MeshFromPoints(square.topRight, square.midRight, square.midTop); break;
        case 8: MeshFromPoints(square.topLeft, square.midTop, square.midLeft); break;

        // 2 points:
        case 3: MeshFromPoints(square.midRight, square.bottomRight, square.bottomLeft, square.midLeft); break;
        case 6: MeshFromPoints(square.midTop, square.topRight, square.bottomRight, square.midBottom); break;
        case 9: MeshFromPoints(square.topLeft, square.midTop, square.midBottom, square.bottomLeft); break;
        case 12: MeshFromPoints(square.topLeft, square.topRight, square.midRight, square.midLeft); break;
        case 5: MeshFromPoints(square.midTop, square.topRight, square.midRight, square.midBottom, square.bottomLeft, square.midLeft); break;
        case 10: MeshFromPoints(square.topLeft, square.midTop, square.midRight, square.bottomRight, square.midBottom, square.midLeft); break;

        // 3 point:
        case 7: MeshFromPoints(square.midTop, square.topRight, square.bottomRight, square.bottomLeft, square.midLeft); break;
        case 11: MeshFromPoints(square.topLeft, square.midTop, square.midRight, square.bottomRight, square.bottomLeft); break;
        case 13: MeshFromPoints(square.topLeft, square.topRight, square.midRight, square.midBottom, square.bottomLeft); break;
        case 14: MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.midBottom, square.midLeft); break;

        // 4 point:
        case 15:
            MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
            checkedVertices.Add(square.topLeft.vertexIndex);
            checkedVertices.Add(square.topRight.vertexIndex);
            checkedVertices.Add(square.bottomRight.vertexIndex);
            checkedVertices.Add(square.bottomLeft.vertexIndex);
            break;
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

		var triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
		AddTriangleToDictionary(triangle.a, triangle);
		AddTriangleToDictionary(triangle.b, triangle);
		AddTriangleToDictionary(triangle.c, triangle);
	}

	private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
	{
		if (triangleDictionary.ContainsKey(vertexIndexKey))
		{
			triangleDictionary[vertexIndexKey].Add(triangle);
		}
		else
		{
			List<Triangle> triangles = new List<Triangle>();
			triangles.Add(triangle);
			triangleDictionary[vertexIndexKey] = triangles;
		}
	}

	private void CalculateMeshOutlines()
	{
		for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
		{
			if (checkedVertices.Contains(vertexIndex)) continue;

			int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
			if (newOutlineVertex >= 0)
			{
				checkedVertices.Add(vertexIndex);
				var newOutline = new List<int>();
				newOutline.Add(vertexIndex);
				outlines.Add(newOutline);
				FollowOutline(newOutlineVertex, outlines.Count - 1);
				outlines[outlines.Count - 1].Add(vertexIndex);
			}
		}
	}

	private void FollowOutline(int vertexIndex, int outlineIndex)
	{
		outlines[outlineIndex].Add(vertexIndex);
		checkedVertices.Add(vertexIndex);

		int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

		if (nextVertexIndex >= 0) FollowOutline(nextVertexIndex, outlineIndex);
	}

	private int GetConnectedOutlineVertex(int vertexA)
	{
		List<Triangle> trianglesWithVertexA = triangleDictionary[vertexA];

		for (int i = 0; i < trianglesWithVertexA.Count; i++)
		{
			var triangle = trianglesWithVertexA[i];

			for (int j = 0; j < 3; j++)
			{
				int vertexB = triangle[j];
				if (vertexA == vertexB || checkedVertices.Contains(vertexB)) continue;

				if (IsOutlineEdge(vertexA, vertexB)) return vertexB;
			}
		}

		return -1;
	}

	private bool IsOutlineEdge(int vertexA, int vertexB)
	{
		var trianglesWithA = triangleDictionary[vertexA];
		int sharedTriangleCount = 0;

		for (int i = 0; i < trianglesWithA.Count; i++)
		{
			if (!trianglesWithA[i].Contains(vertexB)) continue;

			sharedTriangleCount += 1;
			if (sharedTriangleCount > 1) break;
		}
		return sharedTriangleCount == 1;
	}

	struct Triangle
	{
		public readonly int a;
		public readonly int b;
		public readonly int c;

		private readonly int[] vertices;

		public int this[int index] => vertices[index];

		public Triangle(int a, int b, int c)
		{
			this.a = a;
			this.b = b;
			this.c = c;

			vertices = new int[] {a, b, c,};
		}

		public bool Contains(int vertex) => vertex == a || vertex == b || vertex == c;

	}

	public class SquareGrid
	{

		public Square[,] squares;

		public SquareGrid(int[,] map, float squareSize)
		{
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
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

		public Square(ControlNode topLeft, ControlNode topRight, 
            ControlNode bottomRight, ControlNode bottomLeft)
		{
			this.topLeft = topLeft;
			this.topRight = topRight;
			this.bottomRight = bottomRight;
			this.bottomLeft = bottomLeft;

			midTop = topLeft.right;
			midRight = bottomRight.above;
			midBottom = bottomLeft.right;
			midLeft = bottomLeft.above;

			if (topLeft.active) configuration += 8;
			if (topRight.active) configuration += 4;
			if (bottomRight.active) configuration += 2;
			if (bottomLeft.active) configuration += 1;
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