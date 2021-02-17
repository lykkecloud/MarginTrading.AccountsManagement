using MarginTrading.Backend.Contracts.Orders;
using Newtonsoft.Json;

namespace MarginTrading.AccountsManagement.Extensions.AdditionalInfo
{
    public static class OrderAdditionalInfoExtensions
    {
        public static bool ProductComplexityConfirmationReceived(this OrderContract order, bool defaultValue = false)
        {
            try
            {
                var model = JsonConvert.DeserializeAnonymousType(order.AdditionalInfo,
                    new
                    {
                        ProductComplexityConfirmationReceived = (bool?)null
                    });

                return model.ProductComplexityConfirmationReceived ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
