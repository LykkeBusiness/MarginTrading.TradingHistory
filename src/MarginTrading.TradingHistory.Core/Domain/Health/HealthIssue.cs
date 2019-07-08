// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.TradingHistory.Core.Domain.Health
{
    public class HealthIssue
    {
        public string Type { get; private set; }
        public string Value { get; private set; }

        public static HealthIssue Create(string type, string value)
        {
            return new HealthIssue
            {
                Type = type,
                Value = value
            };
        }
    }
}
