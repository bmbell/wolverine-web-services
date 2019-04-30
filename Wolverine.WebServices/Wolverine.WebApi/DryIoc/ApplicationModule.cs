using Bell.Common.DryIoc;
using Bell.Common.WebApi.DryIoc;
using DryIoc;
using Wolverine.Services.DryIoc;

namespace Wolverine.WebApi.DryIoc
{
    /// <summary>
    /// The application module
    /// </summary>
    public class ApplicationModule : IDryIocModule
    {
        /// <inheritdoc />
        /// <summary>
        /// Register's the application module
        /// </summary>
        /// <param name="container">The reference to the DryIOC container</param>
        public void Register(IContainer container)
        {
            var configuration = Startup.Configuration;

            CommonWebApiModules.Register(container, configuration);
            ServicesModule.Register(container);
        }
    }
}
