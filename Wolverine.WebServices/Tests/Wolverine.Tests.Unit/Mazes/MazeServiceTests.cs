using Bell.Common.Exceptions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using NSubstitute;
using Wolverine.Models.GraphTheory;
using Wolverine.Models.Mazes;
using Wolverine.Models.Resources;
using Wolverine.Services.GraphTheory;
using Wolverine.Services.Mazes;
using Xunit;
using BellErrorMessageKeys = Bell.Common.Models.Resources.ErrorMessageKeys;

namespace Wolverine.Tests.Unit.Mazes
{
    public class MazeServiceTests
    {
        #region Protected Methods

        protected static IMazeService CreateService(IDijkstraService dijkstraService = null)
        {
            dijkstraService = dijkstraService ?? Substitute.For<IDijkstraService>();

            return new MazeService(new MazeValidator(), dijkstraService);
        }

        protected static List<MazeCell> GenerateMazeCells()
        {
            return new List<MazeCell>
            {
                new MazeCell { Id = "1", Passages = new List<Direction> { Direction.South } },
                new MazeCell { Id = "2", Passages = new List<Direction> { Direction.South, Direction.East } },
                new MazeCell { Id = "3", Passages = new List<Direction> { Direction.West, Direction.South } },
                new MazeCell { Id = "4", Passages = new List<Direction> { Direction.North, Direction.East } },
                new MazeCell { Id = "5", Passages = new List<Direction> { Direction.North, Direction.South, Direction.West } },
                new MazeCell { Id = "6", Passages = new List<Direction> { Direction.North, Direction.South } },
                new MazeCell { Id = "7", Passages = new List<Direction> { Direction.East } },
                new MazeCell { Id = "8", Passages = new List<Direction> { Direction.West, Direction.North } },
                new MazeCell { Id = "9", Passages = new List<Direction> { Direction.North } }
            };
        }

        protected static Node CreateNode(string id, List<string> neighborIds)
        {
            var node = new Node { Id = id };

            neighborIds?.ForEach(neighborId =>
            {
                node.Neighbors.Add(new Neighbor {NodeId = neighborId, Cost = 1});
            });

            return node;
        }

        #endregion

        #region Tests

        public class ConvertToGraphMethod
        {
            [Fact]
            public void ShouldThrowError_WhenGivenInvalidMazeParameters()
            {
                // Arrange
                var mazeService = CreateService();

                // Act
                Func<List<Node>> result1 = () =>
                    mazeService.ConvertToGraph(new Maze { Height = 0, Width = 5, Cells = new List<MazeCell>() });

                Func<List<Node>> result2 = () =>
                    mazeService.ConvertToGraph(new Maze { Height = 5, Width = -1, Cells = new List<MazeCell>() });

                Func<List<Node>> result3 = () =>
                    mazeService.ConvertToGraph(new Maze { Height = 5, Width = 5, Cells = null });

                // Assert
                result1.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
                result2.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
                result3.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenBlankCellId()
            {
                // Arrange
                var mazeService = CreateService();

                var mazeCells = GenerateMazeCells();
                mazeCells[4].Id = " ";


                // Act
                Func<List<Node>> result1 = () =>
                    mazeService.ConvertToGraph(new Maze { Height = 3, Width = 3, Cells = mazeCells });

                // Assert
                result1.Should().Throw<UserReportableException>()
                    .WithMessage(BellErrorMessageKeys.ERROR_STRING_ONLY_WHITE_SPACE);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenIncorrectlySizedDimensions()
            {
                // Arrange
                var mazeService = CreateService();

                var cells = new List<MazeCell>
                {
                    new MazeCell {Id = "1"}, new MazeCell {Id = "2"}, new MazeCell {Id = "3"},
                    new MazeCell {Id = "4"}, new MazeCell {Id = "5"}, new MazeCell {Id = "6"},
                    new MazeCell {Id = "7"}, new MazeCell {Id = "8"}, new MazeCell {Id = "9"}
                };

                // Act
                Func<List<Node>> result1 = () =>
                    mazeService.ConvertToGraph(new Maze { Height = 1, Width = 3, Cells = cells });
                Func<List<Node>> result2 = () =>
                    mazeService.ConvertToGraph(new Maze { Height = 3, Width = 8, Cells = cells });

                // Assert
                result1.Should().Throw<UserReportableException>()
                    .WithMessage(ErrorMessageKeys.ERROR_INVALID_DIMENSIONS);
                result2.Should().Throw<UserReportableException>()
                    .WithMessage(ErrorMessageKeys.ERROR_INVALID_DIMENSIONS);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenDuplicateCellIds()
            {
                // Arrange
                var mazeService = CreateService();

                var mazeCells = GenerateMazeCells();
                mazeCells[2].Id = mazeCells[6].Id;


                // Act
                Func<List<Node>> result1 = () =>
                    mazeService.ConvertToGraph(new Maze { Height = 3, Width = 3, Cells = mazeCells });

                // Assert
                result1.Should().Throw<UserReportableException>()
                    .WithMessage(ErrorMessageKeys.ERROR_DUPLICATE_MAZE_CELL_ID);
            }

            [Fact]
            public void ShouldConvertMazeToGraph_WhenGivenValidInput()
            {
                // Arrange
                var mazeService = CreateService();

                var expectedNodes = new List<Node>
                {
                    CreateNode("1", new List<string> { "4" }),
                    CreateNode("2", new List<string> { "3", "5" }),
                    CreateNode("3", new List<string> { "2", "6" }),
                    CreateNode("4", new List<string> { "1", "5" }),
                    CreateNode("5", new List<string> { "2", "4", "8" }),
                    CreateNode("6", new List<string> { "3", "9" }),
                    CreateNode("7", new List<string> { "8" }),
                    CreateNode("8", new List<string> { "7", "5" }),
                    CreateNode("9", new List<string> { "6" }),
                };

                // Act
                var result = mazeService.ConvertToGraph(new Maze { Height = 3, Width = 3, Cells = GenerateMazeCells() });

                // Assert
                result.Should().BeEquivalentTo(expectedNodes);
            }
        }

        public class GenerateMazeMethod
        {
            [Fact]
            public void ShouldThrowError_WhenGivenBadHeightOrWidth()
            {
                // Arrange
                var mazeService = CreateService();

                // Act
                Func<Maze> result1 = () => mazeService.GenerateMaze(-1, 11);
                Func<Maze> result2 = () => mazeService.GenerateMaze(11, -1);

                // Assert
                result1.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.ERROR_VALUE_MUST_BE_WITHIN_RANGE);
                result2.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.ERROR_VALUE_MUST_BE_WITHIN_RANGE);
            }

            [Fact]
            public void ShouldCreateUniqueNodes_WhenGivenValidInput()
            {
                // Arrange
                var mazeService = CreateService();

                // Act
                var result = mazeService.GenerateMaze(5, 5);

                var nodeIds = new HashSet<string>();

                // Assert
                foreach (var node in result.Cells)
                {
                    nodeIds.Contains(node.Id).Should().BeFalse();

                    if (!nodeIds.Contains(node.Id))
                    {
                        nodeIds.Add(node.Id);
                    }
                }
            }

            [Fact]
            public void ShouldCreateConnectedMaze_WhenGivenValidInput()
            {
                // Arrange
                var mazeService = CreateService();
                var dijkstraService = new DijkstraService(new FindShortestPathRequestValidator(), new FindShortestPathsRequestValidator());

                // Act
                var result = mazeService.GenerateMaze(5, 5);
                var nodes = mazeService.ConvertToGraph(result);

                // Assert
                foreach (var node in nodes)
                {
                    var table = dijkstraService.FindShortestPaths(new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = node.Id });

                    foreach (var row in table.Rows)
                    {
                        var isValidRow = row.NodeId == node.Id || row.NeighborNodeId != null;
                        isValidRow.Should().BeTrue();
                    }
                }
            }
        }

        public class FindSolutionMethod
        {
            [Fact]
            public void ShouldThrowError_WhenGivenInvalidInputParameters()
            {
                // Arrange
                var mazeService = CreateService();

                // Act
                Func<List<string>> result1 = () => mazeService.FindSolution(null);

                // Assert
                result1.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.ERROR_NULL_VALUE);
            }

            [Fact]
            public void ShouldCallDijkstraServiceForSolution_WhenGivenValidInput()
            {
                // Arrange
                var dijkstraService = Substitute.For<IDijkstraService>();
                var mazeService = CreateService(dijkstraService);

                var maze = new Maze { Cells = GenerateMazeCells(), Height = 3, Width = 3 };

                // Act
                var result = mazeService.FindSolution(new SolveMazeRequest { Maze = maze, StartPointId = "1", EndPointId = "9"});

                // Assert
                dijkstraService.Received()
                    .FindShortestPath(Arg.Any<List<Node>>(), Arg.Any<string>(), Arg.Any<string>());
            }
        }

        #endregion
    }
}
