using Bell.Common.Exceptions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Wolverine.Models.GraphTheory;
using Wolverine.Models.Resources;
using Wolverine.Services.GraphTheory;
using Xunit;
using BellErrorMessageKeys = Bell.Common.Models.Resources.ErrorMessageKeys;

namespace Wolverine.Tests.Unit.GraphTheory
{
    public class DijkstraServiceTests
    {
        #region Protected Methods

        protected static DijkstraService CreateService()
        {
            return new DijkstraService(new FindShortestPathRequestValidator(), new FindShortestPathsRequestValidator());
        }

        protected static List<Node> GenerateUndirectedGraphNodes()
        {
            return new List<Node> {
                new Node
                {
                    Id = "A",
                    Neighbors = new List<Neighbor>
                    {
                        new Neighbor { NodeId = "B", Cost = 6 },
                        new Neighbor { NodeId = "D", Cost = 1 }
                    }
                },
                new Node
                {
                    Id = "B",
                    Neighbors = new List<Neighbor>
                    {
                        new Neighbor { NodeId = "A", Cost = 6 },
                        new Neighbor { NodeId = "C", Cost = 5 },
                        new Neighbor { NodeId = "D", Cost = 2 },
                        new Neighbor { NodeId = "E", Cost = 2 }
                    }
                },
                new Node
                {
                    Id = "C",
                    Neighbors = new List<Neighbor>
                    {
                        new Neighbor { NodeId = "B", Cost = 5 },
                        new Neighbor { NodeId = "E", Cost = 5 }
                    }
                },
                new Node
                {
                    Id = "D",
                    Neighbors = new List<Neighbor>
                    {
                        new Neighbor { NodeId = "A", Cost = 1 },
                        new Neighbor { NodeId = "B", Cost = 2 },
                        new Neighbor { NodeId = "E", Cost = 1 }
                    }
                },
                new Node
                {
                    Id = "E",
                    Neighbors = new List<Neighbor>
                    {
                        new Neighbor { NodeId = "B", Cost = 2 },
                        new Neighbor { NodeId = "C", Cost = 5 },
                        new Neighbor { NodeId = "D", Cost = 1 }
                    }
                }
            };
        }

        #endregion

        #region Tests

        public class FindShortestPathsMethod
        {
            [Fact]
            public void ShouldThrowError_WhenGivenNullNodeCollection()
            {
                // Arrange
                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = null, PrimaryNodeId = "A" };

                // Act
                Func<FastestPathTable> result1 = () => dijkstraService.FindShortestPaths(request);

                // Assert
                result1.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenBlankPrimaryNodeId()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                var dijkstraService = CreateService();

                // Act
                Func<FastestPathTable> result1 = () => dijkstraService.FindShortestPaths(new FindShortestPathsRequest { Nodes =  nodes, PrimaryNodeId = "" });
                Func<FastestPathTable> result2 = () => dijkstraService.FindShortestPaths(new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = null });

                // Assert
                result1.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
                result2.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenBlankNodeIds()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                nodes[2].Id = " ";

                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = "A" };

                // Act
                Func<FastestPathTable> result = () => dijkstraService.FindShortestPaths(request);

                // Assert
                result.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.ERROR_STRING_ONLY_WHITE_SPACE);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenDuplicateNodeIds()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                nodes[3].Id = "B";

                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = "A" };

                // Act
                Func<FastestPathTable> result = () => dijkstraService.FindShortestPaths(request);

                // Assert
                result.Should().Throw<UserReportableException>().WithMessage(ErrorMessageKeys.ERROR_DUPLICATE_NODE_ID);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenDuplicateNeighborNodeIds()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                nodes[1].Neighbors[1].NodeId = nodes[1].Neighbors[3].NodeId;

                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = "A" };

                // Act
                Func<FastestPathTable> result = () => dijkstraService.FindShortestPaths(request);

                // Assert
                result.Should().Throw<UserReportableException>().WithMessage(ErrorMessageKeys.ERROR_DUPLICATE_NEIGHBOR_NODE_ID);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenUnmatchingCostsForNeighbors()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                nodes[1].Neighbors[0].Cost++;

                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = "A" };

                // Act
                Func<FastestPathTable> result = () => dijkstraService.FindShortestPaths(request);

                // Assert
                result.Should().Throw<UserReportableException>().WithMessage(ErrorMessageKeys.ERROR_UNMATCHING_EDGE_COSTS);
            }

            [Fact]
            public void ShouldThrowError_WhenPrimaryNodeIdDoesNotExistInNodeCollection()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = "SOME_OTHER_NODE_ID" };

                // Act
                Func<FastestPathTable> result = () => dijkstraService.FindShortestPaths(request);

                // Assert
                result.Should().Throw<UserReportableException>().WithMessage(ErrorMessageKeys.ERROR_NODE_ID_NOT_FOUND_IN_COLLECTION);
            }

            [Fact]
            public void ShouldGenerateValidTable_WhenGivenUndirectedGraphNodes()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                var expectedResult = new FastestPathTable()
                {
                    PrimaryNodeId = "A",
                    Rows = new List<FastestPathRow>
                    {
                        new FastestPathRow { NodeId = "A", LowestCost = 0, NeighborNodeId = null },
                        new FastestPathRow { NodeId = "B", LowestCost = 3, NeighborNodeId = "D" },
                        new FastestPathRow { NodeId = "C", LowestCost = 7, NeighborNodeId = "E" },
                        new FastestPathRow { NodeId = "D", LowestCost = 1, NeighborNodeId = "A" },
                        new FastestPathRow { NodeId = "E", LowestCost = 2, NeighborNodeId = "D" }
                    }
                };

                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = "A" };

                // Act
                var results = dijkstraService.FindShortestPaths(request);

                // Assert
                results.Should().BeEquivalentTo(expectedResult);
            }

            [Fact]
            public void ShouldGenerateValidTable_WhenGivenDirectedGraphNodes()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();

                // Remove the neighbor E from node D to make it directed (D->E)...
                var nodeDNeighbors = nodes.FirstOrDefault(n => n.Id == "D").Neighbors;
                nodeDNeighbors.RemoveAll(n => n.NodeId == "E");

                var expectedResult = new FastestPathTable()
                {
                    PrimaryNodeId = "A",
                    Rows = new List<FastestPathRow>
                    {
                        new FastestPathRow { NodeId = "A", LowestCost = 0, NeighborNodeId = null },
                        new FastestPathRow { NodeId = "B", LowestCost = 3, NeighborNodeId = "D" },
                        new FastestPathRow { NodeId = "C", LowestCost = 8, NeighborNodeId = "B" },
                        new FastestPathRow { NodeId = "D", LowestCost = 1, NeighborNodeId = "A" },
                        new FastestPathRow { NodeId = "E", LowestCost = 5, NeighborNodeId = "B" }
                    }
                };

                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = "A" };

                // Act
                var results = dijkstraService.FindShortestPaths(request);

                // Assert
                results.Should().BeEquivalentTo(expectedResult);
            }

            [Fact]
            public void ShouldGenerateValidTable_WhenGivenUnconnectedGraphNodes()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                nodes.Add(new Node { Id = "F", Neighbors = null });

                var expectedResult = new FastestPathTable()
                {
                    PrimaryNodeId = "A",
                    Rows = new List<FastestPathRow>
                    {
                        new FastestPathRow { NodeId = "A", LowestCost = 0, NeighborNodeId = null },
                        new FastestPathRow { NodeId = "B", LowestCost = 3, NeighborNodeId = "D" },
                        new FastestPathRow { NodeId = "C", LowestCost = 7, NeighborNodeId = "E" },
                        new FastestPathRow { NodeId = "D", LowestCost = 1, NeighborNodeId = "A" },
                        new FastestPathRow { NodeId = "E", LowestCost = 2, NeighborNodeId = "D" },
                        new FastestPathRow { NodeId = "F", LowestCost = int.MaxValue, NeighborNodeId = null }
                    }
                };

                var dijkstraService = CreateService();
                var request = new FindShortestPathsRequest { Nodes = nodes, PrimaryNodeId = "A" };

                // Act
                var results = dijkstraService.FindShortestPaths(request);

                // Assert
                results.Should().BeEquivalentTo(expectedResult);
            }
        }

        public class FindShortestPathMethod
        {
            [Fact]
            public void ShouldThrowError_WhenGivenNullNodeCollection()
            {
                // Arrange
                var dijkstraService = CreateService();
                var request = new FindShortestPathRequest { Nodes = null, StartNodeId = "A", EndNodeId = "B"};

                // Act
                Func<List<string>> result = () => dijkstraService.FindShortestPath(request);

                // Assert
                result.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
            }

            [Fact]
            public void ShouldThrowError_WhenGivenBlankPrimaryNodeId()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                var dijkstraService = CreateService();

                // Act
                Func<List<string>> result1 = () => dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = null, EndNodeId = "B" });
                Func<List<string>> result2 = () => dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "A", EndNodeId = " " });

                // Assert
                result1.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
                result2.Should().Throw<UserReportableException>().WithMessage(BellErrorMessageKeys.VALIDATION_ERRORS);
            }

            [Fact]
            public void ShouldThrowError_WhenStartNodeIdIsNotInNodeCollection()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                var dijkstraService = CreateService();
                var request = new FindShortestPathRequest { Nodes = nodes, StartNodeId = "NOT_THERE", EndNodeId = "B" };

                // Act
                Func<List<string>> result1 = () => dijkstraService.FindShortestPath(request);

                // Assert
                result1.Should().Throw<UserReportableException>().WithMessage(ErrorMessageKeys.ERROR_NODE_ID_NOT_FOUND_IN_COLLECTION);
            }

            [Fact]
            public void ShouldThrowError_WhenEndNodeIdIsNotInNodeCollection()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                var dijkstraService = CreateService();
                var request = new FindShortestPathRequest { Nodes = nodes, StartNodeId = "A", EndNodeId = "NOT_THERE" };

                // Act
                Func<List<string>> result1 = () => dijkstraService.FindShortestPath(request);

                // Assert
                result1.Should().Throw<UserReportableException>().WithMessage(ErrorMessageKeys.ERROR_NODE_ID_NOT_FOUND_IN_COLLECTION);
            }

            [Fact]
            public void ShouldGenerateSingleNodePath_WhenGivenSameNodeIds()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                var dijkstraService = CreateService();
                var request = new FindShortestPathRequest { Nodes = nodes, StartNodeId = "C", EndNodeId = "C" };

                // Act
                var result = dijkstraService.FindShortestPath(request);

                // Assert
                result.Should().BeEquivalentTo(new List<string> { "C" });
            }

            [Fact]
            public void ShouldGenerateValidPath_WhenGivenUndirectedGraphNodes()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                var dijkstraService = CreateService();

                // Act
                var result1 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "A", EndNodeId = "C" });
                var result2 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "C", EndNodeId = "A" });
                var result3 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "E", EndNodeId = "A" });
                var result4 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "B", EndNodeId = "A" });
                var result5 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "C", EndNodeId = "B" });

                // Assert
                result1.Should().BeEquivalentTo(new List<string> { "A", "D", "E", "C" });
                result2.Should().BeEquivalentTo(new List<string> { "C", "E", "D", "A" });
                result3.Should().BeEquivalentTo(new List<string> { "E", "D", "A" });
                result4.Should().BeEquivalentTo(new List<string> { "B", "D", "A" });
                result5.Should().BeEquivalentTo(new List<string> { "C", "B" });
            }

            [Fact]
            public void ShouldGenerateValidPath_WhenGivenDirectedGraphNodes()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();

                // Remove the neighbor E from node D to make it directed (D->E)...
                var nodeDNeighbors = nodes.FirstOrDefault(n => n.Id == "D")?.Neighbors;
                nodeDNeighbors?.RemoveAll(n => n.NodeId == "E");

                var dijkstraService = CreateService();

                // Act
                var result1 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "A", EndNodeId = "C" });
                var result2 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "C", EndNodeId = "A" });
                var result3 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "E", EndNodeId = "A" });
                var result4 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "B", EndNodeId = "A" });
                var result5 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "C", EndNodeId = "B" });

                // Assert
                result1.Should().BeEquivalentTo(new List<string> { "A", "D", "E", "C" });
                result2.Should().BeEquivalentTo(new List<string> { "C", "B", "D", "A" });
                result3.Should().BeEquivalentTo(new List<string> { "E", "B", "D", "A" });
                result4.Should().BeEquivalentTo(new List<string> { "B", "D", "A" });
                result5.Should().BeEquivalentTo(new List<string> { "C", "B" });
            }

            [Fact]
            public void ShouldReturnNull_WhenNoPathExistsBetweenTwoNodes()
            {
                // Arrange
                var nodes = GenerateUndirectedGraphNodes();
                nodes.Add(new Node { Id = "F", Neighbors = { new Neighbor { NodeId = "G", Cost = 1 } } });
                nodes.Add(new Node { Id = "G", Neighbors = { new Neighbor { NodeId = "F", Cost = 1 } } });

                var dijkstraService = CreateService();

                // Act
                var result1 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "A", EndNodeId = "F" });
                var result2 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "F", EndNodeId = "G" });
                var result3 = dijkstraService.FindShortestPath(new FindShortestPathRequest { Nodes = nodes, StartNodeId = "G", EndNodeId = "A" });

                // Assert
                result1.Should().BeNull();
                result2.Should().BeEquivalentTo(new List<string> { "F", "G" });
                result3.Should().BeNull();
            }
        }

        #endregion

    }
}
