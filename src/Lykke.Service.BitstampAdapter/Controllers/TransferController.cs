using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Microsoft.AspNetCore.Mvc;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.SpotController.Records;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    public class TransferController : Controller
    {
        private ILog _log;

        protected ApiClient Api => this.RestApi<ApiClient>();

        public TransferController(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        [HttpGet("balaces"), XApiKeyAuth]
        public async Task<GetWalletsResponse> GetWalletBalancesAsync()
        {
            return new GetWalletsResponse
            {
                Wallets = await Api.Balance()
            };
        }

        [HttpPost("fromSubToMain"), XApiKeyAuth]
        public async Task<TransferResponce> FromSubToMainAsync([FromBody] TransferFromSubToMainRequest request)
        {
            if (request == null)
                throw new Exception("Request cannot be null");

            if (string.IsNullOrEmpty(request.Currency))
                throw new Exception("Request.Currency cannot be null");

            if (request.Amount <=0 )
                throw new Exception("Request.Amount must be more zero");


            var res = await Api.TransferSubToMain(request.SubAccount, request.Amount, request.Currency);

            return new TransferResponce
            {
                Reason = res.Reason,
                Status = res.Status
            };
        }

        [HttpPost("fromMainToSub"), XApiKeyAuth]
        public async Task<TransferResponce> FromMainToSubAsync([FromBody] TransferFromSubToMainRequest request)
        {
            if (request == null)
                throw new Exception("Request cannot be null");

            if (string.IsNullOrEmpty(request.SubAccount))
                throw new Exception("Request.SubAccount cannot be null");

            if (string.IsNullOrEmpty(request.Currency))
                throw new Exception("Request.Currency cannot be null");

            if (request.Amount <= 0)
                throw new Exception("Request.Amount must be more zero");

            var res = await Api.TransferMainToSub(request.SubAccount, request.Amount, request.Currency);

            return new TransferResponce
            {
                Reason = res.Reason,
                Status = res.Status
            };
        }

        [HttpGet("bitcoinWithdrawal/{timedeltastr}"), XApiKeyAuth]
        public async Task<List<WithdrawalResponce>> WithdrawalRequestsAsync(string timedeltastr)
        {
            if (string.IsNullOrEmpty(timedeltastr)) timedeltastr = "0";
            if (!int.TryParse(timedeltastr, out var timedelta))
                throw new Exception("incorect time delta");

            var data = await Api.WithdrawalRequests(timedelta);

            return data.Select(res =>
                new WithdrawalResponce
                {
                    Amount = res.Amount,
                    Currency = res.Currency,
                    Status = (WithdrawalResponce.WithdrawalStatus)res.Status,
                    Address = res.Address,
                    Datetime = res.Datetime,
                    Id = res.Id,
                    TransactionId = res.TransactionId,
                    Type = (WithdrawalResponce.WithdrawalType)res.Type
                }).ToList();
        }

        [HttpGet("bitcoinDepositAddress/{asset}"), XApiKeyAuth]
        public async Task<BitcoinDepositAddress> GetDepositAddressAsync(string asset)
        {
            string res = "";

            if (asset.ToUpper() == "BTC")
            {
                res = await Api.BitcoinDepositAddress();
                return new BitcoinDepositAddress { Address = res };
            }

            if (asset.ToUpper() == "LTC")
            {
                res = await Api.LitecoinDepositAddress();
                return new BitcoinDepositAddress { Address = res };
            }

            if (asset.ToUpper() == "ETH")
            {
                res = await Api.EthDepositAddress();
                return new BitcoinDepositAddress { Address = res };
            }

            if (asset.ToUpper() == "BCH")
            {
                res = await Api.BchDepositAddress();
                return new BitcoinDepositAddress { Address = res };
            }

            if (asset.ToUpper() == "XRP")
            {
                res = await Api.XrpDepositAddress();
                return new BitcoinDepositAddress { Address = res };
            }


            throw new Exception($"Incorect asset: {asset}");
        }

        [HttpPost("createCoinsWithdrawal"), XApiKeyAuth]
        public async Task<CoinsWithdrawalResponce> CreateCoinsWithdrawalAsync([FromBody] CoinsWithdrawalRequest request)
        {
            if (request == null)
                throw new Exception("Request cannot be null");

            if (string.IsNullOrEmpty(request.Address))
                throw new Exception("Request.Address cannot be null");

            if (request.Amount <= 0)
                throw new Exception("Request.Amount must be more zero");

            if (request.Asset.ToUpper() == "BTC")
            {
                var res = await Api.CreateBitcoinWithdrawal(request.Amount, request.Address, request.SupportBitGo ?? false);
                return new CoinsWithdrawalResponce {WithdrawalId = res.Id};
            }

            if (request.Asset.ToUpper() == "ETH")
            {
                var res = await Api.CreateEthWithdrawal(request.Amount, request.Address);
                return new CoinsWithdrawalResponce { WithdrawalId = res.Id };
            }

            if (request.Asset.ToUpper() == "LTC")
            {
                var res = await Api.CreateLitecoinWithdrawal(request.Amount, request.Address);
                return new CoinsWithdrawalResponce { WithdrawalId = res.Id };
            }

            if (request.Asset.ToUpper() == "BCH")
            {
                var res = await Api.CreateBchWithdrawal(request.Amount, request.Address);
                return new CoinsWithdrawalResponce { WithdrawalId = res.Id };
            }

            if (request.Asset.ToUpper() == "XRP")
            {
                var res = await Api.CreateXrpWithdrawal(request.Amount, request.Address, request.XrpDestinationTag);
                return new CoinsWithdrawalResponce { WithdrawalId = res.Id };
            }

            throw new Exception($"Asset {request.Asset} not supported");
        }

        [HttpGet("unconfirmedBitcoinDeposits"), XApiKeyAuth]
        public async Task<UnconfirmedBitcoinDepositResponce> UnconfirmedBitcoinDepositsAsync()
        {
            var res = await Api.UnconfirmedBitcoinDeposits();

            return new UnconfirmedBitcoinDepositResponce
            {
                UnconfirmedDeposits = res.Select(e =>
                    new UnconfirmedBitcoinDepositResponce.Deposit(e.Amount, e.Address, e.Confirmations)).ToList()
            };
        }

    }

    public class CoinsWithdrawalResponce
    {
        public string WithdrawalId { get; set; }
    }

    public class CoinsWithdrawalRequest
    {
        public string Asset { get; set; }
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public string XrpDestinationTag { get; set; }
        public bool? SupportBitGo { get; set; }
    }


    public class BitcoinDepositAddress
    {
        public string Address { get; set; }
    }

    public class TransferFromSubToMainRequest
    {
        public string SubAccount { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }
    }

    public class TransferResponce
    {
        public string Status { get; set; }
        public string Reason { get; set; }
    }

    public class UnconfirmedBitcoinDepositResponce
    {
        public List<Deposit> UnconfirmedDeposits { get; set; }

        public class Deposit
        {
            public Deposit()
            {
            }

            public Deposit(decimal amount, string address, int confirmations)
            {
                Amount = amount;
                Address = address;
                Confirmations = confirmations;
            }

            public decimal Amount { get; set; }
            public string Address { get; set; }
            public int Confirmations { get; set; }
        }
    }

    public class WithdrawalResponce
    {
        public string Id { get; set; }
        public DateTime Datetime { get; set; }
        public WithdrawalType Type { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public WithdrawalStatus Status { get; set; }
        public string Address { get; set; }
        public string TransactionId { get; set; }

        public enum WithdrawalType
        {
            Sepa = 0,
            Bitcoin = 1,
            WireTransfer = 2,
            Xrp = 14,
            Litecoin = 15,
            Ethereum = 16
        }

        public enum WithdrawalStatus
        {
            Open = 0,
            InProcess = 1,
            Finished = 2,
            Canceled = 3,
            Failed = 4
        }

    }


    public static class LocalClientTokenMiddleware
    {
        private const string CredsKey = "api-credentials";

        internal static T RestApi<T>(this Controller controller)
        {
            if (controller.HttpContext.Items.TryGetValue(CredsKey, out var creds))
            {
                return (T) creds;
            }

            return default(T);
        }
    }
}
