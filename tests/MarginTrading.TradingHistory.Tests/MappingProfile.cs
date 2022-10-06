// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using DealContract = MarginTrading.Backend.Contracts.Positions.DealContract;

namespace MarginTrading.TradingHistory.Tests
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<IDeal, DealContract>(MemberList.None);
            CreateMap<IAggregatedDeal, AggregatedDealContract>(MemberList.None);
            CreateMap<IPositionHistory, PositionHistoryEntity>(MemberList.None);
        }
    }
}
