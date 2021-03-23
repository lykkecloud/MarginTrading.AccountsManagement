using System.ComponentModel.DataAnnotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class UpdateClientTradingConditionRequest
    {
        /// <summary>
        /// The client id to update trading condition for
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// The trading condition id
        /// </summary>
        [Required]
        public string TradingConditionId { get; set; }
        
        /// <summary>
        /// Name of the user who sent the request
        /// </summary>
        [Required]
        public string Username { get; set; }

    }
}
