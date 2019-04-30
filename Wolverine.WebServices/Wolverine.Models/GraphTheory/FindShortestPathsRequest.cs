using System.Collections.Generic;

namespace Wolverine.Models.GraphTheory
{
    /// <summary>
    /// The request for finding the shortest paths to a primary node in a graph
    /// </summary>
    public class FindShortestPathsRequest
    {
        #region Public Properties

        /// <summary>
        /// The primary node's id
        /// </summary>
        public string PrimaryNodeId { get; set; }

        /// <summary>
        /// The list of nodes in the graph
        /// </summary>
        public List<Node> Nodes { get; set; }

        #endregion
    }
}
