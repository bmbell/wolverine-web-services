namespace Wolverine.Models.GraphTheory
{
    /// <summary>
    /// Represents a single row in the fastest path table generated using Dijkstra's Algorithm
    /// </summary>
    public class FastestPathRow
    {
        #region Public Properties

        /// <summary>
        /// The node's id
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// The lowest cost to the primary node from this node
        /// </summary>
        public int LowestCost { get; set; }

        /// <summary>
        /// The neighbor node id to use to trace back to the primary node
        /// </summary>
        public string NeighborNodeId { get; set; }

        #endregion
    }
}
