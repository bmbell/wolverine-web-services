using System.Collections.Generic;

namespace Wolverine.Models.Mazes
{
    /// <summary>
    /// Represents a maze
    /// </summary>
    public class Maze
    {
        /// <summary>
        /// The cells in the grid, represented as nodes.
        /// They are ordered, left-to-right, top-to-bottom.
        /// </summary>
        public List<MazeCell> Cells { get; set; }

        /// <summary>
        /// The height of the grid
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The width of the grid
        /// </summary>
        public int Width { get; set; }
    }
}
