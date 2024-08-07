// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;

using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core.Domain;

using Xunit;

namespace MarginTrading.TradingHistory.Tests
{
    public class OrderRejectReasonEnumsTests
    {
        [Fact]
        public void DomainAndContractEnumsShouldMatch()
        {
            var domainEnumValues = Enum.GetValues<OrderRejectReason>().Select(x => x.ToString());
            var contractEnumValues = Enum.GetValues<OrderRejectReasonContract>().Select(x => x.ToString());

            Assert.Equal(contractEnumValues, domainEnumValues);
        }
    }
}