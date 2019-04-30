using Bell.Common.Validation;
using FluentValidation;
using Wolverine.Models.GraphTheory;

namespace Wolverine.Services.GraphTheory
{
    public interface IFindShortestPathRequestValidator : IValidatorBase<FindShortestPathRequest>
    {

    }

    public class FindShortestPathRequestValidator: ValidatorBase<FindShortestPathRequest>, IFindShortestPathRequestValidator
    {

        public FindShortestPathRequestValidator()
        {
            RuleFor(request => request.StartNodeId).NotEmpty();
            RuleFor(request => request.EndNodeId).NotEmpty();
            RuleFor(request => request.Nodes).NotNull();
        }
    }
}
