using Bell.Common.Validation;
using FluentValidation;
using Wolverine.Models.Mazes;

namespace Wolverine.Services.Mazes
{
    public interface IMazeValidator : IValidatorBase<Maze>
    {

    }

    public class MazeValidator : ValidatorBase<Maze>, IMazeValidator
    {

        public MazeValidator()
        {
            RuleFor(m => m.Height).GreaterThan(0);
            RuleFor(m => m.Width).GreaterThan(0);
            RuleFor(m => m.Cells).Must(m => m != null && m.Count > 0);
        }
    }
}
