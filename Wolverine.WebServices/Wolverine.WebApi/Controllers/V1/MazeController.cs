using System.Collections.Generic;
using Bell.Common.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Models.Mazes;
using Wolverine.Services.Mazes;

namespace Wolverine.WebApi.Controllers.V1
{
    /// <summary>
    /// Maze Services
    /// </summary>
    [Route("api/v1/mazes")]
    public class MazeController: BaseApiController
    {
        #region Private Fields

        private readonly IMazeService _mazeService;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the maze controller
        /// </summary>
        public MazeController(IMazeService recursiveBacktrackingService)
        {
            _mazeService = recursiveBacktrackingService;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates a maze based on the given height and weight
        /// </summary>
        /// <param name="height">The height of the maze [optional (default: 11)]</param>
        /// <param name="width">The width of the maze [optional (default: 11)]</param>
        /// <returns>The randomly generated maze</returns>
        [HttpGet]
        public Maze GenerateMaze([FromQuery]int? height, [FromQuery]int? width)
        {
            return _mazeService.GenerateMaze(height ?? 11, width?? 11);
        }

        /// <summary>
        /// Solves any maze, given the maze request data
        /// </summary>
        /// <returns>The list of node ids that represents the solution path</returns>
        [HttpPost("solve")]
        public List<string> FindSolution([FromBody] SolveMazeRequest request)
        {
            return _mazeService.FindSolution(request);
        }

        #endregion
    }
}
