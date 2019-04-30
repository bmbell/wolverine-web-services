using System.Collections.Generic;

namespace Wolverine.Models.GraphTheory
{
    /// <summary>
    /// The request for finding the shortest path between two nodes in a graph
    /// </summary>
    public class FindShortestPathRequest
    {
        #region Public Properties

        /// <summary>
        /// The start node id
        /// </summary>
        public string StartNodeId { get; set; }

        /// <summary>
        /// The end node id
        /// </summary>
        public string EndNodeId { get; set; }

        /// <summary>
        /// The list of nodes in the graph
        /// </summary>
        public List<Node> Nodes { get; set; }

        #endregion
    }
}
