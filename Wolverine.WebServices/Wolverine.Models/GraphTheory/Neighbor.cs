namespace Wolverine.Models.GraphTheory
{
    /// <summary>
    /// Represents a node's neighbor
    /// </summary>
    public class Neighbor
    {
        /// <summary>
        /// The neighbor node's id
        /// </summary>
        public string NodeId { get; set;}

        /// <summary>
        /// The cost it takes to reach the neighbor
        /// </summary>
        public int Cost { get; set; }
    }
}
