// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Contracts.Responses;
using MarginTrading.TradingHistory.Client.Common;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    [PublicAPI]
    public interface IOrderBlotterApi
    {
        [Get("/api/order-blotter/created-by-on-behalf")]
        Task<IEnumerable<string>> GetCreatedByOnBehalfList();
        
        [Get("/api/order-blotter")]
        Task<PaginatedResponse<OrderForOrderBlotterContract>> Get(
            [Query, NotNull] DateTime? relevanceTimestamp,
            [Query, CanBeNull] string accountIdOrName,
            [Query, CanBeNull] string assetName,
            [Query, CanBeNull] string createdBy,
            [Query(CollectionFormat.Multi), CanBeNull] List<OrderStatusContract> statuses,
            [Query(CollectionFormat.Multi), CanBeNull] List<OrderTypeContract> orderTypes,
            [Query(CollectionFormat.Multi), CanBeNull] List<OriginatorTypeContract> originatorTypes,
            [Query, CanBeNull] DateTime? createdOnFrom,
            [Query, CanBeNull] DateTime? createdOnTo,
            [Query, CanBeNull] DateTime? modifiedOnFrom,
            [Query, CanBeNull] DateTime? modifiedOnTo,
            [Query, CanBeNull] int skip,
            [Query, CanBeNull] int take,
            [Query, CanBeNull] OrderBlotterSortingColumnContract sortingColumn,
            [Query, CanBeNull] SortingOrderContract sortingOrder);
    }
}
