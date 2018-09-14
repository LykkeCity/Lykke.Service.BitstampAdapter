using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.BitstampAdapter.Client.Models.Transfers;

namespace Lykke.Service.BitstampAdapter.Validators
{
    [UsedImplicitly]
    public class TransferModelValidator : AbstractValidator<TransferModel>
    {
        public TransferModelValidator()
        {
            RuleFor(o => o.Account)
                .NotEmpty()
                .WithMessage("Account is required");
            
            RuleFor(o => o.Currency)
                .NotEmpty()
                .WithMessage("Currency is required");

            RuleFor(o => o.Amount)
                .GreaterThan(0)
                .WithMessage("Amount should be greater than zero");
        }
    }
}
