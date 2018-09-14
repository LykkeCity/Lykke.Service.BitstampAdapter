using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.BitstampAdapter.Client.Models.Withdrawals;

namespace Lykke.Service.BitstampAdapter.Validators
{
    [UsedImplicitly]
    public class CreateWithdrawalModelValidator : AbstractValidator<CreateWithdrawalModel>
    {
        public CreateWithdrawalModelValidator()
        {
            RuleFor(o => o.Address)
                .NotEmpty()
                .WithMessage("Address is required");

            RuleFor(o => o.Amount)
                .GreaterThan(0)
                .WithMessage("Amount should be greater than zero");
        }
    }
}
