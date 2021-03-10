using System;
using System.Collections.Generic;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class ComplexityWarningState
    {
        private ComplexityWarningState()
        {
            ConfirmedOrders = new Dictionary<string, OrderInfo>();
        }

        public static ComplexityWarningState Start(string accountId)
        {
            return new ComplexityWarningState
            {
                AccountId = accountId,
                ShouldShowComplexityWarning = true
            };
        }

        public static ComplexityWarningState Restore(string accountId, 
            byte[] rowVersion,
            Dictionary<string, OrderInfo> confirmedOrders,
            bool shouldShowComplexityWarning,
            DateTime? switchedToFalseAt)
        {
            if (confirmedOrders == null)
            {
                throw new ArgumentNullException(nameof(confirmedOrders));
            }

            return new ComplexityWarningState
            {
                RowVersion = rowVersion,
                AccountId = accountId,
                ConfirmedOrders = confirmedOrders,
                ShouldShowComplexityWarning = shouldShowComplexityWarning,
                SwitchedToFalseAt = switchedToFalseAt
            };
        }

        public string AccountId { get; private set; }

        public byte[] RowVersion { get; private set; }

        public Dictionary<string, OrderInfo> ConfirmedOrders { get; private set; }

        public bool ShouldShowComplexityWarning { get; private set; }

        public DateTime? SwitchedToFalseAt { get; private set; }

        public void OnConfirmedOrderReceived(string orderId, DateTime ts, int minOrdersToSwitchFlag, out  bool confirmationFlagSwitched)
        {
            if (minOrdersToSwitchFlag <= 0)
            {
                throw new ArgumentException("Should be positive", nameof(minOrdersToSwitchFlag));
            }
            confirmationFlagSwitched = false;

            if (!ConfirmedOrders.ContainsKey(orderId))
            {
                ConfirmedOrders.Add(orderId, new OrderInfo
                {
                    OrderId = orderId,
                    Timestamp = ts
                });

                if (ShouldShowComplexityWarning && ConfirmedOrders.Count == minOrdersToSwitchFlag)
                {
                    confirmationFlagSwitched = true;
                    ShouldShowComplexityWarning = false;
                    SwitchedToFalseAt = ts;
                }
            }
        }

        public void ResetConfirmation()
        {
            ShouldShowComplexityWarning = true;
            SwitchedToFalseAt = null;
            ConfirmedOrders.Clear();
        }

        public class OrderInfo
        {
            public string OrderId { get; set; }

            public DateTime Timestamp { get; set; }
        }
    }
}