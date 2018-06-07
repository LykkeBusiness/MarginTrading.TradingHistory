using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    [Route("api/positions-history")]
    public class PositionsController : Controller, IPositionsHistoryApi
    {
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        private readonly IConvertService _convertService;
        
        public PositionsController(
            IOrdersHistoryRepository ordersHistoryRepository,
            IConvertService convertService)
        {
            _ordersHistoryRepository = ordersHistoryRepository;
            _convertService = convertService;
        }
        
        /// <summary> 
        /// Get closed positions with optional filtering 
        /// </summary> 
        [HttpGet, Route("")] 
        public async Task<List<PositionContract>> PositionHistory(
            [FromQuery] string accountId, [FromQuery] string instrument)
        {
            var orders = await _ordersHistoryRepository.GetHistoryAsync(x =>
                x.OrderUpdateType == OrderUpdateType.Close &&
                (string.IsNullOrEmpty(accountId) || x.AccountId == accountId)
                && (string.IsNullOrEmpty(instrument) || x.Instrument == instrument));
            
            return orders.Select(Convert).ToList();
        }

        /// <summary>
        /// Get closed position by Id
        /// </summary>
        /// <param name="positionId"></param>
        /// <returns></returns>
        [HttpGet, Route("byId/{positionId}")]
        public async Task<PositionContract> PositionById(string positionId)
        {
            if (string.IsNullOrWhiteSpace(positionId))
            {
                throw new ArgumentException("Position id must be set", nameof(positionId));
            }
            
            var orders = await _ordersHistoryRepository.GetHistoryAsync(x =>
                x.OrderUpdateType == OrderUpdateType.Close &&
                (x.OpenExternalOrderId == positionId
                 || x.CloseExternalOrderId == positionId));
            
            return orders.Select(Convert).FirstOrDefault();
        }

        private PositionContract Convert(IOrderHistory orderHistory)
        {
            return new PositionContract
            {
                Id = orderHistory.Id,
                AccountId = orderHistory.AccountId,
                Instrument = orderHistory.Instrument,
                Timestamp = orderHistory.CloseDate ?? orderHistory.CreateDate,
                Direction = ConvertDirection(orderHistory.Type),
                Price = orderHistory.ClosePrice == default ? orderHistory.OpenPrice : orderHistory.ClosePrice,
                Volume = -orderHistory.Volume,
                PnL = orderHistory.PnL,
                FxRate = orderHistory.QuoteRate,
                Margin = orderHistory.MarginMaintenance,
                TradeId = orderHistory.Id, //TODO need to be fixed
                RelatedOrders = new List<string>(),//TODO need to be fixed
            };
        }

        private PositionDirection ConvertDirection(OrderDirection type)
        {
            switch (type)
            {
                case OrderDirection.Buy: return PositionDirection.Long;
                case OrderDirection.Sell: return PositionDirection.Short;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
