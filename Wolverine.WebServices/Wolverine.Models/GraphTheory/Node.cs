using System.Collections.Generic;

namespace Wolverine.Models.GraphTheory
{
    /// <summary>
    /// The node (or vertex)
    /// </summary>
    public class Node
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Node()
        {
            Neighbors = new List<Neighbor>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The node's id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The node's adjacent neighbors
        /// </summary>
        /// <remarks>A neighbor is only adjacent if it is an undirected graph 
        /// or there is and edge that is not directed towards the neighbor 
        /// (i.e. the neighbor can reach this node directly)</remarks>
        public List<Neighbor> Neighbors { get; set; }

        #endregion
    }
}
