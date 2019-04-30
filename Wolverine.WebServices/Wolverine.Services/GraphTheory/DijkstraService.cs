using Bell.Common.Exceptions;
using Bell.Common.Extensions;
using System.Collections.Generic;
using System.Linq;
using Wolverine.Models.GraphTheory;
using Wolverine.Models.Resources;

namespace Wolverine.Services.GraphTheory
{
    public interface IDijkstraService
    {
        /// <summary>
        /// Finds the shortest path between two nodes
        /// </summary>
        /// <param name="nodes">The list of nodes in the graph</param>
        /// <param name="startNodeId">The start node id</param>
        /// <param name="endNodeId">The end node it</param>
        /// <returns></returns>
        List<string> FindShortestPath(List<Node> nodes, string startNodeId, string endNodeId);

        /// <summary>
        /// Finds the shortest path between two nodes
        /// </summary>
        /// <param name="request">The "find shortest path" request</param>
        /// <returns>A list of node ids representing the shortest path between the nodes or null, 
        /// if no path exists</returns>
        List<string> FindShortestPath(FindShortestPathRequest request);

        /// <summary>
        /// Finds the shortest path from a primary node to every other node
        /// </summary>
        /// <param name="request">The "find shortest paths" request</param>
        /// <returns>The fastest path table, which can be used to find the shortest path between the primary node
        /// and any other node in the graph</returns>
        FastestPathTable FindShortestPaths(FindShortestPathsRequest request);
    }

    public class DijkstraService : IDijkstraService
    {
        #region Private Fields

        private IFindShortestPathRequestValidator _findShortestPathRequestValidator;
        private IFindShortestPathsRequestValidator _findShortestPathsRequestValidator;

        #endregion

        #region Constructors

        public DijkstraService(
            IFindShortestPathRequestValidator findShortestPathRequestValidator,
            IFindShortestPathsRequestValidator findShortestPathsRequestValidator)
        {
            _findShortestPathRequestValidator = findShortestPathRequestValidator;
            _findShortestPathsRequestValidator = findShortestPathsRequestValidator;
        }

        #endregion

        #region Public Methods

        public List<string> FindShortestPath(List<Node> nodes, string startNodeId, string endNodeId)
        {
            return FindShortestPath(new FindShortestPathRequest
                {Nodes = nodes, StartNodeId = startNodeId, EndNodeId = endNodeId});
        }

        public List<string> FindShortestPath(FindShortestPathRequest request)
        {
            _findShortestPathRequestValidator.ValidateAndThrowErrors(request);

            // Check to see if start node id exists in collection
            // The end node id is checked  in the FindShortestPaths() method
            if (!request.Nodes.Exists(n => n.Id == request.StartNodeId))
            {
                throw new UserReportableException(ErrorMessageKeys.ERROR_NODE_ID_NOT_FOUND_IN_COLLECTION, request.StartNodeId);
            }

            var fastestPathTable = FindShortestPaths(new FindShortestPathsRequest { Nodes = request.Nodes, PrimaryNodeId = request.EndNodeId });
            return FindPath(fastestPathTable, request.StartNodeId, request.EndNodeId);
        }

        public FastestPathTable FindShortestPaths(FindShortestPathsRequest request)
        {
            _findShortestPathsRequestValidator.ValidateAndThrowErrors(request);
            ValidateNodes(request.Nodes, request.PrimaryNodeId);

            var fastestPathTable = CreateFastestPathTable(request.Nodes, request.PrimaryNodeId);
            var visitedNodeIds = new HashSet<string>();
            var nodesById = request.Nodes.ToDictionary(n => n.Id);
            var rowsById = fastestPathTable.Rows.ToDictionary(r => r.NodeId);

            while (visitedNodeIds.Count < request.Nodes.Count - 1)
            {
                var lowestCostNodeId = FindLowestCostNodeId(fastestPathTable, visitedNodeIds);

                // Catches graphs that aren't connected...
                if (lowestCostNodeId == null)
                {
                    break;
                }

                UpdateNeighborCosts(rowsById, visitedNodeIds, nodesById[lowestCostNodeId]);
                visitedNodeIds.Add(lowestCostNodeId);
            }

            return fastestPathTable;
        }

        #endregion

        #region Private Methods

        private void ValidateNodes(IList<Node> nodes, string primaryNodeId)
        {
            var nodeIds = new HashSet<string>();
            var edgesToCheck = new Dictionary<string, IDictionary<string, int>>();
            var foundPrimaryNodeId = false;

            for(var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.Id.ThrowUserErrorIfNullOrWhitespace($"nodes[{i}].Id");

                if (nodeIds.Contains(node.Id))
                {
                    throw new UserReportableException(ErrorMessageKeys.ERROR_DUPLICATE_NODE_ID, node.Id);
                }
                else
                {
                    nodeIds.Add(node.Id);
                    CheckNeighborNodes(edgesToCheck, node);
                    foundPrimaryNodeId = foundPrimaryNodeId || primaryNodeId == node.Id;
                }
            }

            if (!foundPrimaryNodeId)
            {
                throw new UserReportableException(ErrorMessageKeys.ERROR_NODE_ID_NOT_FOUND_IN_COLLECTION, primaryNodeId);
            }
        }

        private void CheckNeighborNodes(IDictionary<string, IDictionary<string, int>> edgesToCheck, Node node)
        {
            var neighborNodeIds = new HashSet<string>();

            node.Neighbors?.ForEach(neighbor =>
            {
                // Ensure there are no duplicate neighbor node ids defined
                if (neighborNodeIds.Contains(neighbor.NodeId))
                {
                    throw new UserReportableException(ErrorMessageKeys.ERROR_DUPLICATE_NEIGHBOR_NODE_ID,
                        neighbor.NodeId);
                }

                // Check to make sure that any defined edges match
                if (edgesToCheck.TryGetValue(node.Id, out var edges))
                {
                    if (edges.TryGetValue(neighbor.NodeId, out int cost))
                    {
                        if (cost != neighbor.Cost)
                        {
                            throw new UserReportableException(ErrorMessageKeys.ERROR_UNMATCHING_EDGE_COSTS);
                        }
                    }
                }

                AddEdge(edgesToCheck, neighbor, node.Id);
                neighborNodeIds.Add(neighbor.NodeId);
            });
        }

        private void AddEdge(IDictionary<string, IDictionary<string, int>> edgesToCheck, Neighbor neighbor, string nodeId)
        {
            if (!edgesToCheck.TryGetValue(neighbor.NodeId, out IDictionary<string, int> edges))
            {
                edges = new Dictionary<string, int>();
                edgesToCheck.Add(neighbor.NodeId, edges);
            }

            edges.Add(nodeId, neighbor.Cost);
        }

        private FastestPathTable CreateFastestPathTable(List<Node> nodes, string primaryNodeId)
        {
            var fastestPathTable = new FastestPathTable();

            foreach (var node in nodes)
            {
                fastestPathTable.Rows.Add(new FastestPathRow
                {
                    NodeId = node.Id,
                    LowestCost = node.Id == primaryNodeId ? 0 : int.MaxValue,
                    NeighborNodeId = null
                });
            }

            fastestPathTable.PrimaryNodeId = primaryNodeId;

            return fastestPathTable;
        }

        // TODO: What happens if two nodes are at the same "lowest cost"?
        private string FindLowestCostNodeId(FastestPathTable fastestPathTable, HashSet<string> visitedNodeIds)
        {
            int lowestCost = int.MaxValue;
            string lowestCostNodeId = null;

            foreach (var row in fastestPathTable.Rows)
            {
                if (!visitedNodeIds.Contains(row.NodeId) && row.LowestCost < lowestCost)
                {
                    lowestCost = row.LowestCost;
                    lowestCostNodeId = row.NodeId;
                }
            }

            return lowestCostNodeId;
        }

        private void UpdateNeighborCosts(
            IDictionary<string, FastestPathRow> rowsById,
            HashSet<string> visitedNodeIds,
            Node lowestCostNode)
        {
            // Go through its neighbors that haven't been visited
            // Add current node cost to neighbor cost and if lower than table cost
            // If lower, update row with new cost and current node
            lowestCostNode.Neighbors?.ForEach(neighbor =>
            {
                if (!visitedNodeIds.Contains(neighbor.NodeId))
                {
                    var neighborRow = rowsById[neighbor.NodeId];
                    var newCost = neighbor.Cost + rowsById[lowestCostNode.Id].LowestCost;

                    if (newCost < neighborRow.LowestCost)
                    {
                        neighborRow.LowestCost = newCost;
                        neighborRow.NeighborNodeId = lowestCostNode.Id;
                    }
                }
            });
        }

        private List<string> FindPath(FastestPathTable table, string startNodeId, string endNodeId)
        {
            var path = new List<string> { startNodeId };
            var rowsByNodeId = table.Rows.ToDictionary(r => r.NodeId);

            var currentNodeId = startNodeId;

            while (currentNodeId != endNodeId && currentNodeId != null)
            {
                currentNodeId = rowsByNodeId[currentNodeId].NeighborNodeId;
                path.Add(currentNodeId);
            }

            if (currentNodeId == null)
            {
                path = null;
            }

            return path;
        }

        #endregion
    }
}
