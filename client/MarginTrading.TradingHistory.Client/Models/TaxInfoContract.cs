// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.TradingHistory.Client.Models
{
    public class TaxInfoContract
    {
        public decimal? TotalTaxes { get; set; }

        public decimal? CapitalGainsTax { get; set; }

        public decimal? SolidaritySurcharge { get; set; }

        public decimal? ChurchTaxGroup1 { get; set; }

        public decimal? ChurchTaxGroup2 { get; set; }
    }
}
