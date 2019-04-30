using System.Collections.Generic;

namespace Wolverine.Models.Mazes
{
    /// <summary>
    /// Represents a cell in the maze
    /// </summary>
    public class MazeCell
    {

        #region Constructors

        /// <summary>
        /// Constructs the maze cell
        /// </summary>
        public MazeCell()
        {
            Passages = new List<Direction>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The id of the cell
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Indicates the directions where there are passages in relation to this cell
        /// </summary>
        public List<Direction> Passages { get; set; }

        #endregion
    }
}
