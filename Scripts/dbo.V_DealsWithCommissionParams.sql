CREATE OR ALTER VIEW [dbo].[V_DealsWithCommissionParams]
AS
SELECT deal.DealId,
deal.Created,
deal.AccountId,
deal.AssetPairId,
deal.OpenTradeId,
deal.CloseTradeId,
deal.Direction,
deal.Volume,
deal.Originator,
deal.OpenPrice,
deal.OpenFxPrice,
deal.ClosePrice,
deal.CloseFxPrice,
deal.Fpl,
deal.AdditionalInfo,
deal.OpenOrderType,
deal.OpenOrderVolume,
deal.OpenOrderExpectedPrice,
deal.CloseOrderType,
deal.CloseOrderVolume,
deal.CloseOrderExpectedPrice,
deal.PnlOfTheLastDay,
dcp.OvernightFees,
dcp.Commission,
dcp.OnBehalfFee,
dcp.Taxes
FROM [dbo].[Deals] deal
LEFT JOIN [dbo].[DealCommissionParams] dcp
ON deal.DealId = dcp.DealId