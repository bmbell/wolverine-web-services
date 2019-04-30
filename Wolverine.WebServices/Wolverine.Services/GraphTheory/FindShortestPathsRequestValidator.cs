using Bell.Common.Validation;
using FluentValidation;
using Wolverine.Models.GraphTheory;

namespace Wolverine.Services.GraphTheory
{
    public interface IFindShortestPathsRequestValidator : IValidatorBase<FindShortestPathsRequest>
    {

    }

    public class FindShortestPathsRequestValidator : ValidatorBase<FindShortestPathsRequest>, IFindShortestPathsRequestValidator
    {

        public FindShortestPathsRequestValidator()
        {
            RuleFor(request => request.PrimaryNodeId).NotEmpty();
            RuleFor(request => request.Nodes).NotNull();
        }
    }
}
