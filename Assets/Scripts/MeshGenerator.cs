using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

	public SquareGrid grid;

	public void GenerateMesh(int[,] map, float squareSize)
	{
		grid = new SquareGrid(map, squareSize);
	}

	private void OnDrawGizmos()
	{
		if (grid == null) return;

		for (int x = 0; x < grid.squares.GetLength(0); ++x)
		{
			for (int y = 0; y < grid.squares.GetLength(1); ++y)
			{
				Gizmos.color = grid.squares[x,y].topLeft.active ? Color.black : Color.white;
				Gizmos.DrawCube(grid.squares[x,y].topLeft.position, Vector3.one * 0.4f);

				Gizmos.color = grid.squares[x,y].topRight.active ? Color.black : Color.white;
				Gizmos.DrawCube(grid.squares[x,y].topRight.position, Vector3.one * 0.4f);

				Gizmos.color = grid.squares[x,y].bottomRight.active ? Color.black : Color.white;
				Gizmos.DrawCube(grid.squares[x,y].bottomRight.position, Vector3.one * 0.4f);

				Gizmos.color = grid.squares[x,y].bottomLeft.active ? Color.black : Color.white;
				Gizmos.DrawCube(grid.squares[x,y].bottomLeft.position, Vector3.one * 0.4f);

				Gizmos.color = Color.gray;
				Gizmos.DrawCube(grid.squares[x,y].midTop.position, Vector3.one * 0.15f);
				Gizmos.DrawCube(grid.squares[x,y].midRight.position, Vector3.one * 0.15f);
				Gizmos.DrawCube(grid.squares[x,y].midBottom.position, Vector3.one * 0.15f);
				Gizmos.DrawCube(grid.squares[x,y].midLeft.position, Vector3.one * 0.15f);
			}
		}
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