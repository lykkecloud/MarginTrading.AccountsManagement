﻿using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.IntegrationalTests.Infrastructure;

namespace MarginTrading.AccountsManagement.IntegrationalTests.WorkflowTests
{
    public static class TestsHelpers
    {
        public const string ClientId = "IntergationalTestsClient";
        public const string AccountId = "IntergationalTestsAccount-1";

        static TestsHelpers()
        {
            var sett = SettingsUtil.Settings.MarginTradingAccountManagement.Cqrs;
            var connectionString = sett.ConnectionString;
            var eventsExchange = $"{sett.EnvironmentName}.{sett.ContextNames.AccountsManagement}.events.exchange";
            
            RabbitUtil.ListenCqrsMessages<AccountBalanceChangedEvent>(connectionString, eventsExchange);
            RabbitUtil.ListenCqrsMessages<DepositCompletedEvent>(connectionString, eventsExchange);
            RabbitUtil.ListenCqrsMessages<WithdrawalCompletedEvent>(connectionString, eventsExchange);
            RabbitUtil.ListenCqrsMessages<WithdrawalFailedEvent>(connectionString, eventsExchange);

            // todo: register other messages
        }

        public static async Task<AccountContract> EnsureAccountState(decimal needBalance = 0)
        {
            var account = await ClientUtil.AccountsApi.GetByClientAndId(ClientId, AccountId);
            if (account == null)
            {
                account = await ClientUtil.AccountsApi.Create(ClientId, new CreateAccountRequest
                {
                    AccountId = AccountId,
                    BaseAssetId = "USD",
                });
            }

            if (account.Balance != needBalance)
            {
                await ChargeManually(needBalance - account.Balance);
                account.Balance = needBalance;
            }

            if (account.IsDisabled)
            {
                account = await ClientUtil.AccountsApi.Change(ClientId, AccountId, new ChangeAccountRequest
                {
                    IsDisabled = false,
                });
            }

            return account;
        }

        public static async Task ChargeManually(decimal delta)
        {
            var operationId = await ClientUtil.AccountsApi.BeginChargeManually(ClientId, AccountId,
                new AccountChargeManuallyRequest
                {
                    AmountDelta = delta,
                    Reason = "intergational tests"
                });

            await RabbitUtil.WaitForCqrsMessage<AccountBalanceChangedEvent>(m => m.OperationId == operationId);
        }

        public static Task<AccountContract> GetAccount()
        {
            return ClientUtil.AccountsApi.GetByClientAndId(ClientId, AccountId);
        }
    }
}