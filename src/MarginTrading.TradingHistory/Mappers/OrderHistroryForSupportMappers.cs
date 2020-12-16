using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.SqlRepositories;

namespace MarginTrading.TradingHistory.Mappers
{
    public static class OrderHistroryForSupportMappers
    {
        public static OrderEventForSupportContract ToContract(
            this OrderHistoryForSupportQuery.ResultItem source)
        {
            return new OrderEventForSupportContract
            {
                ClientId = source.ClientId,
                ExecutedTimestamp = source.ExecutedTimestamp,
                CreatedTimestamp = source.CreatedTimestamp,
                Status = source.Status,
                Volume = source.Volume,
                ExecutionPrice = source.ExecutionPrice,
                AssetPairId = source.AssetPairId,
                OrderId = source.OrderId
            };
        }       
        public static OrderHistoryForSupportQuery.Criterion FromContract(
            this OrderEventsForSupportRequest source)
        {
            return new OrderHistoryForSupportQuery.Criterion
            {
                ClientId = source.ClientId,
                AssetPairIds = source.AssetPairIds,
                Take = source.Take,
                Skip = source.Skip,
                ExecutionPrice = source.ExecutionPrice,
                CreatedTimestampFrom = source.CreatedTimestampFrom,
                CreatedTimestampTo = source.CreatedTimestampTo,
                ExecutedTimestampFrom = source.ExecutedTimestampFrom,
                ExecutedTimestampTo = source.ExecutedTimestampTo,
                Id = source.Id,
                IsAscending = source.IsAscending
            };
        }        
        
    }
}
