namespace Wolverine.Models.Mazes
{
    /// <summary>
    /// A request for solving a maze
    /// </summary>
    public class SolveMazeRequest
    {
        #region Public Properties

        /// <summary>
        /// The maze to solve
        /// </summary>
        public Maze Maze { get; set; }

        /// <summary>
        /// The start point's maze cell id
        /// </summary>
        public string StartPointId { get; set; }

        /// <summary>
        /// The end point's maze cell id (finish point)
        /// </summary>
        public string EndPointId { get; set; }

        #endregion
    }
}
