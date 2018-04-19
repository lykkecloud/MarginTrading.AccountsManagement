using System;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class OperationState
    {
        public OperationState(string operationName, string id, string state)
        {
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public string OperationName { get; }
        public string Id { get; }
        public string State { get; }
    }
}