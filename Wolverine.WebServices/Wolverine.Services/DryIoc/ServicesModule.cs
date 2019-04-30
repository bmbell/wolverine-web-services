using DryIoc;
using Wolverine.Services.GraphTheory;
using Wolverine.Services.Mazes;

namespace Wolverine.Services.DryIoc
{
    public static class ServicesModule
    {
        public static void Register(IContainer container)
        {
            container.Register<IDijkstraService, DijkstraService>();
            container.Register<IFindShortestPathRequestValidator, FindShortestPathRequestValidator>();
            container.Register<IFindShortestPathsRequestValidator, FindShortestPathsRequestValidator>();

            container.Register<IMazeService, MazeService>();
            container.Register<IMazeValidator, MazeValidator>();
        }
    }
}
