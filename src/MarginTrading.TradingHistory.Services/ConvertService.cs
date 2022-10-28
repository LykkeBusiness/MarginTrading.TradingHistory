// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.Services
{
    [UsedImplicitly]
    public class ConvertService : IConvertService
    {
        private readonly IMapper _mapper = CreateMapper();

        private static IMapper CreateMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<IDealWithCommissionParams, DealContract>()
                    .ForMember(dest => dest.Direction,
                        opt => opt.MapFrom(src => src.Direction.ToType<PositionDirectionContract>()));
                cfg.CreateMap<IAggregatedDeal, AggregatedDealContract>();
            }).CreateMapper();
        }

        public TResult Convert<TSource, TResult>(TSource source)
        {
            return _mapper.Map<TSource, TResult>(source);
        }

        public void AssertConfigurationIsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
