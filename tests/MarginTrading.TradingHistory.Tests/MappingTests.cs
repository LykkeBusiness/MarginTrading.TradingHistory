// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.TradingHistory.Services;
using Xunit;

namespace MarginTrading.TradingHistory.Tests
{
    public class MappingTests
    {
        [Fact]
        public void ShouldHaveValidMappingConfiguration()
        {
            var converter = new ConvertService();
            
            var ex = Record.Exception(() => converter.AssertConfigurationIsValid());
            
            Assert.Null(ex);
        }
    }
}
