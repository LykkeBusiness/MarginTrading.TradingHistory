// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using AutoMapper;
using Xunit;

namespace MarginTrading.TradingHistory.Tests
{
    public class MappingTests
    {
        [Fact]
        public void ShouldHaveValidMappingConfiguration()
        {
            var mockMapper = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
            var mapper = mockMapper.CreateMapper();

            // act
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            // assert
            Assert.True(true);
        }
    }
}
