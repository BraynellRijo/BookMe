using Application.Interfaces.Repositories.Users;
using Domain.Entities.Users;
using FluentValidation;
using System.Threading;

namespace Application.Validators.UserValidators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator(IQueryUserRepository queryUserRepository)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email format is required.")
                .MustAsync(async (user, email, cancellation) =>
                {
                    var existingUser = await queryUserRepository.GetUserByEmail(email);

                    return existingUser == null || existingUser.Id == user.Id;
                })
                .WithMessage("This email address is already registered.");

        }
    }
}