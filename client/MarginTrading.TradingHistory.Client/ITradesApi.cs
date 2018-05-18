using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    [PublicAPI]
    public interface ITradesApi
    {
        Task<TradeContract> Get(string tradeId); 
        Task<List<TradeContract>> List([Query] string orderId, [Query] string positionId); 
    }
}
