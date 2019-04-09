using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IOrdersHistoryRepository
    {
        Task AddAsync(IOrderHistory order, ITrade trade);
        
        Task<IEnumerable<IOrderHistoryWithAdditional>> GetHistoryAsync(string orderId, 
            OrderStatus? status = null);

        Task<PaginatedResponse<IOrderHistoryWithAdditional>> GetHistoryByPagesAsync(string accountId, string assetPairId,
            List<OrderStatus> statuses, List<OrderType> orderTypes, List<OriginatorType> originatorTypes,
			string parentOrderId = null,
            DateTime? createdTimeStart = null, DateTime? createdTimeEnd = null,
            DateTime? modifiedTimeStart = null, DateTime? modifiedTimeEnd = null,
            int? skip = null, int? take = null, bool isAscending = true);
    }
}
