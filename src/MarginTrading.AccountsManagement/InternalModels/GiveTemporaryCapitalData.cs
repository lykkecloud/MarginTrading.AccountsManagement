namespace MarginTrading.AccountsManagement.InternalModels
{
    public class GiveTemporaryCapitalData : OperationDataBase<GiveTemporaryCapitalState>
    {
        public string OperationId { get; set; }

        public string EventSourceId { get; set; }

        public string AccountId { get; set; }

        public decimal Amount { get; set; }

        public string Reason { get; set; }

        public string AuditLog { get; set; }
    }
}