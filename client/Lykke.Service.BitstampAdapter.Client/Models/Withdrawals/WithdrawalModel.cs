using System;
using JetBrains.Annotations;

namespace Lykke.Service.BitstampAdapter.Client.Models.Withdrawals
{
    /// <summary>
    /// Describes a withdrawal
    /// </summary>
    [PublicAPI]
    public class WithdrawalModel
    {
        /// <summary>
        /// The identifier of withdrawal 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The date of withdrawal
        /// </summary>
        public DateTime Datetime { get; set; }

        /// <summary>
        /// The type of withdrawal
        /// </summary>
        public WithdrawalType Type { get; set; }

        /// <summary>
        /// The currency
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The status
        /// </summary>
        public WithdrawalStatus Status { get; set; }

        /// <summary>
        /// The address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The withdrawal transaction identifier
        /// </summary>
        public string TransactionId { get; set; }
    }
}
