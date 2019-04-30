using System;
using System.Collections.Generic;
using System.Linq;
using Bell.Common.Exceptions;
using Bell.Common.Extensions;
using Wolverine.Models.GraphTheory;
using Wolverine.Models.Mazes;
using Wolverine.Models.Resources;
using Wolverine.Services.GraphTheory;

namespace Wolverine.Services.Mazes
{

    /// <summary>
    ///  Maze generation service
    /// </summary>
    public interface IMazeService
    {
        /// <summary>
        /// Generates a random maze of given height and width
        /// </summary>
        /// <remarks>Based on the ruby implementation by Jamis Buck
        /// (https://weblog.jamisbuck.org/2010/12/27/maze-generation-recursive-backtracking)</remarks>
        /// <param name="height">The maze's height (number of cells)</param>
        /// <param name="width">The maze's width (number of cells)</param>
        /// <returns>The randomly generated maze</returns>
        Maze GenerateMaze(int height, int width);

        /// <summary>
        /// Converts the maze into graph representation
        /// </summary>
        /// <param name="maze">The maze to convert</param>
        /// <returns>A list of nodes corresponding to the graph representation of the maze</returns>
        List<Node> ConvertToGraph(Maze maze);

        /// <summary>
        /// Finds the solution between any two given points in the maze
        /// </summary>
        /// <param name="request">The solve maze request</param>
        /// <returns>A list of ids corresponding to the solution path</returns>
        List<string> FindSolution(SolveMazeRequest request);
    }

    public class MazeService: IMazeService
    {
        #region Private Fields

        private readonly IMazeValidator _mazeValidator;
        private readonly IDijkstraService _dijkstraService;

        #endregion

        #region Constructors

        public MazeService(IMazeValidator mazeValidator, IDijkstraService dijkstraService)
        {
            _mazeValidator = mazeValidator;
            _dijkstraService = dijkstraService;
        }

        #endregion

        #region Public Methods

        public Maze GenerateMaze(int height, int width)
        {
            height.ThrowUserErrorIfInvalidRange(nameof(height), 1, 100);
            width.ThrowUserErrorIfInvalidRange(nameof(width), 1, 100);

            var grid = InitializeGrid(height, width);

            CarvePassage(0, 0, grid);

            return new Maze
            {
                Height = height,
                Width = width,
                Cells = grid.Cast<MazeCell>().ToList()
            };
        }

        public List<Node> ConvertToGraph(Maze maze)
        {
            _mazeValidator.ValidateAndThrowErrors(maze);
            ValidateMaze(maze);

            var nodesById = InitializeNodes(maze.Cells);

            for (var h = 0; h < maze.Height; h++)
            {
                for (var w = 0; w < maze.Width; w++)
                {
                    var cellIndex = h * maze.Width + w;
                    var cell = maze.Cells[cellIndex];

                    var northCell = h > 0 ? maze.Cells[(h - 1) * maze.Width + w] : null;
                    var southCell = h < maze.Height - 1 ? maze.Cells[(h + 1) * maze.Width + w] : null;
                    var eastCell = w < maze.Width - 1 ? maze.Cells[cellIndex + 1] : null;
                    var westCell = w > 0 ? maze.Cells[cellIndex - 1] : null;

                    foreach (var direction in cell.Passages)
                    { 
                        var neighbor = PickNeighborCell(northCell, southCell, eastCell, westCell, direction);

                        if (neighbor != null)
                        {
                            nodesById[cell.Id].Neighbors.Add(new Neighbor {NodeId = neighbor.Id, Cost = 1});
                        }
                    }
                }
            }

            return nodesById.Values.ToList();
        }

        public List<string> FindSolution(SolveMazeRequest request)
        {
            request.ThrowUserErrorIfNull(nameof(request));

            var nodes = ConvertToGraph(request.Maze);
            return _dijkstraService.FindShortestPath(nodes, request.StartPointId, request.EndPointId);
        }

        #endregion

        #region Private Methods

        private MazeCell[,] InitializeGrid(int height, int width)
        {
            var grid = new MazeCell[height, width];

            for (var h = 0; h < height; h++)
            {
                for (var w = 0; w < width; w++)
                {
                    var id = h * height + w;
                    grid[h,w] = new MazeCell { Id = id.ToString() };
                }
            }

            return grid;
        }

        private void CarvePassage(int currentX, int currentY, MazeCell[,] grid)  
        {
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);  
            MazeCell currentCell = grid[currentY, currentX];

            var directions = new List<Direction> { Direction.North, Direction.South, Direction.East, Direction.West };
            directions.Shuffle();

            foreach(var direction in directions)
            {
                int nextX = CalculateNextX(currentX, direction);
                int nextY = CalculateNextY(currentY, direction);

                if (nextY >= 0 && nextY < height && nextX >= 0 && nextX < width)
                {
                    var nextCell = grid[nextY, nextX];

                    if (nextCell.Passages.Count == 0)
                    {
                        currentCell.Passages.Add(direction);
                        nextCell.Passages.Add(FindOppositeDirection(direction));
                        CarvePassage(nextX, nextY, grid);
                    }
                }
            }
        }

        private int CalculateNextX(int currentX, Direction direction)
        {
            int nextX = currentX;

            switch(direction)
            {
                case Direction.East:
                    nextX++;
                    break;

                case Direction.West:
                    nextX--;
                    break;
            }

            return nextX;
        }

        private int CalculateNextY(int currentY, Direction direction)
        {
            int nextY = currentY;

            switch (direction)
            {
                case Direction.North:
                    nextY--;
                    break;

                case Direction.South:
                    nextY++;
                    break;
            }

            return nextY;
        }

        private Direction FindOppositeDirection(Direction direction)
        {
            Direction opposite;

            switch (direction)
            {
                case Direction.North:
                    opposite = Direction.South;
                    break;

                case Direction.South:
                    opposite = Direction.North;
                    break;

                case Direction.East:
                    opposite = Direction.West;
                    break;

                case Direction.West:
                    opposite = Direction.East;
                    break;

                default:
                    throw new Exception($"Invalid direction: {direction}");
            }

            return opposite;
        }

        private void ValidateMaze(Maze maze)
        {
            var mazeCellCount = maze.Cells.Count;
            var expectedCellCount = maze.Height * maze.Width;

            if (mazeCellCount != expectedCellCount)
            {
                throw new UserReportableException(ErrorMessageKeys.ERROR_INVALID_DIMENSIONS);
            }

            var cellIds = new HashSet<string>();

            for (var i = 0; i < mazeCellCount; i++)
            {
                var cell = maze.Cells[i];
                cell.Id.ThrowUserErrorIfNullOrWhitespace($"Cells[{i}].Id");

                if (cellIds.Contains(cell.Id))
                {
                    throw new UserReportableException(ErrorMessageKeys.ERROR_DUPLICATE_MAZE_CELL_ID, cell.Id);
                }

                cellIds.Add(cell.Id); 
            }
        }

        private Dictionary<string, Node> InitializeNodes(List<MazeCell> cells)
        {
            var nodes = new Dictionary<string, Node>();

            foreach (var cell in cells)
            {
                nodes.Add(cell.Id, new Node { Id = cell.Id });
            }

            return nodes;
        }

        private MazeCell PickNeighborCell(MazeCell northCell, MazeCell southCell, MazeCell eastCell, MazeCell westCell, Direction direction)
        {
            MazeCell cell;

            switch (direction)
            {
                case Direction.North:
                    cell = northCell;
                    break;

                case Direction.South:
                    cell = southCell;
                    break;

                case Direction.East:
                    cell = eastCell;
                    break;

                case Direction.West:
                    cell = westCell;
                    break;

                default:
                    throw new Exception($"Invalid direction: {direction}");
            }

            return cell;
        }

        #endregion
    }
}
