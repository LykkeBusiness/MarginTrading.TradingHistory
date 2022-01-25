// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IOrdersHistoryRepository
    {
        Task AddAsync(IOrderHistory order, ITrade trade);

        Task<IEnumerable<string>> GetCreatedByOnBehalfListAsync();

        Task<PaginatedResponse<IOrderHistoryForOrderBlotterWithAdditionalData>> GetOrderBlotterAsync(
            DateTime relevanceTimestamp,
            string accountId,
            string assetName,
            string createdBy,
            List<OrderStatus> statuses,
            List<OrderType> orderTypes,
            List<OriginatorType> originatorTypes,
            DateTime? createdOnFrom,
            DateTime? createdOnTo,
            DateTime? modifiedOnFrom,
            DateTime? modifiedOnTo,
            int skip,
            int take,
            OrderBlotterSortingColumn sortingColumn,
            SortingOrder sortingOrder);
        
        Task<IEnumerable<IOrderHistoryWithAdditional>> GetHistoryAsync(string orderId, 
            OrderStatus? status = null);

        Task<PaginatedResponse<IOrderHistoryWithAdditional>> GetHistoryByPagesAsync(string accountId, string assetPairId,
            List<OrderStatus> statuses, List<OrderType> orderTypes, List<OriginatorType> originatorTypes,
			string parentOrderId = null,
            DateTime? createdTimeStart = null, DateTime? createdTimeEnd = null,
            DateTime? modifiedTimeStart = null, DateTime? modifiedTimeEnd = null,
            int? skip = null, int? take = null, bool isAscending = true,
            bool executedOrdersEssentialFieldsOnly = false);
    }
}
