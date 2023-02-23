-- Copyright (c) 2019 Lykke Corp.
-- See the LICENSE file in the project root for more information.

-- exec sp_helptext [dbo.getDealDetails]
-- exec sp_help [dbo.Deals]
-- exec [dbo].[getDealDetails] 'XZKGZGW5ZY_A6NEZZHYUY'

CREATE OR ALTER PROCEDURE [dbo].[getDealDetails] (
    @DealId NVARCHAR(128)
)
AS
   SET NOCOUNT ON;

WITH position AS
         (
             SELECT
                 positionsHist.Id AS PositionId,
                 positionsHist.DealId,
                 positionsHist.OpenDate AS OpenTimestamp
             FROM dbo.PositionsHistory AS positionsHist (NOLOCK)
             WHERE positionsHist.DealId = @DealId
         ),
     deal AS
         (
             SELECT
                 -- Header
                 DealId AS DealId,
                 AccountId as AccountId,
                 AssetPairId AS AssetPairId,
                 OpenTradeId AS OpeningTradeId,
                 CloseTradeId AS ClosingTradeId,
                 ABS(Volume) AS Size,
    CONVERT(INT, ABS(Volume) * IIF(Direction IN ('Long', 'Buy'), 1, -1)) AS DealSize, -- probably 
     -- it is better to know the contract and use 'Long' or 'Buy', but not both?
    Created AS DealTimestamp,

     -- Opening side
    IIF(Direction IN ('Long', 'Buy'), 'Buy', 'Sell') AS OpenDirection,
    CONVERT(INT, ABS(Volume) * IIF(Direction IN ('Long', 'Buy'), 1, -1)) AS OpenSize, -- Using closing side Volume because it's always one deal for each closing trade
    CONVERT(DECIMAL(24,13), OpenPrice) AS OpenPrice,
    CONVERT(DECIMAL(24,13), ABS(Volume) * OpenPrice) AS OpenContractVolume,

     -- Total Opening side (Non-sense because current design is one deal for each closing trade)
    IIF(Direction IN ('Long', 'Buy'), 'Buy', 'Sell') AS TotalOpenDirection,
    CONVERT(INT, ABS(Volume) * IIF(Direction IN ('Long', 'Buy'), 1, -1)) AS TotalOpenSize,
    CONVERT(DECIMAL(24,13), OpenPrice) AS TotalOpenPrice,
    CONVERT(DECIMAL(24,13), ABS(Volume) * OpenPrice) AS TotalOpenContractVolume,

     -- Closing side
    Created AS CloseTimestamp,
    IIF(Direction IN ('Long', 'Buy'), 'Sell', 'Buy') AS CloseDirection,
    CONVERT(INT, ABS(Volume) * IIF(Direction IN ('Long', 'Buy'), -1, 1)) AS CloseSize,
    CONVERT(DECIMAL(24,13), ClosePrice) AS ClosePrice,
    CONVERT(DECIMAL(24,13), ABS(Volume) * ClosePrice) AS CloseContractVolume,
    CONVERT(DECIMAL(24,13), (ClosePrice - OpenPrice) * ABS(Volume) * IIF(Direction IN ('Long', 'Buy'), 1, -1)) AS GrossPnLTc,
    CONVERT(DECIMAL(24,13), IIF(CloseFxPrice = 0, 0, 1 / CloseFxPrice)) AS GrossPnLFxPrice,
    CONVERT(DECIMAL(24,13), Fpl) AS GrossPnLSc, -- PnL always in SC
    CONVERT(DECIMAL(24,13), Fpl) AS RealisedPnLBtxSc -- PnL always in SC

FROM dbo.Deals AS deals (NOLOCK)
WHERE deals.DealId = @DealId
    ),
    trades AS
    (
SELECT deal.DealId, trade.Id, Volume = ABS(trade.Volume)
FROM deal
    INNER JOIN dbo.Trades (NOLOCK) AS trade
ON trade.Id IN (deal.OpeningTradeId, deal.ClosingTradeId)
    ),
    accountHistory AS
    (
SELECT
    account.EventSourceId,
    account.ReasonType,
    account.ChangeAmount,
    account.AuditLog
FROM deal
    INNER JOIN dbo.AccountHistory (NOLOCK) AS account
ON account.EventSourceId IN (deal.DealId, deal.OpeningTradeId, deal.ClosingTradeId)
    AND account.ReasonType IN ('Tax', 'Commission','OnBehalf','RealizedPnL')
    ),
    fees AS
    (
SELECT deal.DealId,
    taxInfo.AuditLog as TaxInfo,
    CONVERT(DECIMAL(24,13), ((ISNULL(openingCommission.ChangeAmount, 0.0) / openTrade.Volume + ISNULL(closingCommission.ChangeAmount, 0.0) / closeTrade.Volume) * deal.Size)) AS OverallCommissions,
    CONVERT(DECIMAL(24,13), ((ISNULL(openingOnBehalf.ChangeAmount, 0.0) / openTrade.Volume + ISNULL(closingOnBehalf.ChangeAmount, 0.0) / closeTrade.Volume) * deal.Size)) AS OverallOnBehalfFees
FROM deal
    INNER JOIN trades AS openTrade
ON deal.OpeningTradeId = openTrade.Id
    INNER JOIN trades AS closeTrade
    ON deal.ClosingTradeId = closeTrade.Id
    LEFT JOIN accountHistory openingCommission
    ON deal.OpeningTradeId = openingCommission.EventSourceId AND openingCommission.ReasonType = 'Commission'
    LEFT OUTER JOIN accountHistory closingCommission
    ON deal.ClosingTradeId = closingCommission.EventSourceId AND closingCommission.ReasonType = 'Commission'
    LEFT OUTER JOIN accountHistory openingOnBehalf
    ON deal.OpeningTradeId = openingOnBehalf.EventSourceId AND openingOnBehalf.ReasonType = 'OnBehalf'
    LEFT OUTER JOIN accountHistory closingOnBehalf
    ON deal.ClosingTradeId = closingOnBehalf.EventSourceId AND closingOnBehalf.ReasonType = 'OnBehalf'
    LEFT OUTER JOIN accountHistory AS taxInfo
    ON deal.DealId = taxInfo.EventSourceId AND taxInfo.ReasonType = 'Tax'
    ),
    swaps AS
    (
SELECT deal.DealId,
    CONVERT(DECIMAL(24,13), Sum(swapHistory.SwapValue / ABS(swapHistory.Volume)) * deal.Size) AS OverallFinancingCost
FROM deal
    INNER JOIN position
ON deal.DealId = position.DealId
    INNER JOIN dbo.OvernightSwapHistory (NOLOCK) AS swapHistory
    ON position.PositionId = swapHistory.PositionId AND swapHistory.IsSuccess = 1
WHERE FLOOR(CAST(swapHistory.TradingDay as float)) < FLOOR(CAST(deal.DealTimestamp as float))
GROUP BY deal.DealId, deal.Size
    ),
    pnl AS
    (
SELECT deal.DealId,
    CONVERT(DECIMAL(24,13), ISNULL(accountHistory.ChangeAmount, 0.0)) AS RealizedPnLDaySc,
    CONVERT(DECIMAL(24,13), ISNULL(accountHistory.ChangeAmount, 0.0) - ISNULL(deal.RealisedPnLBtxSc, 0.0)) AS NettingOfPreviouslySettledPnLs
FROM deal
    INNER JOIN accountHistory
ON accountHistory.EventSourceId = deal.DealId
    AND accountHistory.ReasonType = 'RealizedPnL'
    )

SELECT
    deal.AccountId,
    deal.AssetPairId,
    position.PositionId,
    deal.DealId,
    deal.DealSize,
    deal.DealTimestamp,

    OpenTradeId = deal.OpeningTradeId,
    position.OpenTimestamp,
    deal.OpenDirection,
    deal.OpenSize,
    deal.OpenPrice,
    deal.OpenContractVolume,

    deal.TotalOpenDirection,
    deal.TotalOpenSize,
    deal.TotalOpenPrice,
    deal.TotalOpenContractVolume,

    CloseTradeId = deal.ClosingTradeId,
    deal.CloseTimestamp,
    deal.CloseDirection,
    deal.CloseSize,
    deal.ClosePrice,
    deal.CloseContractVolume,
    deal.GrossPnLTc,
    deal.GrossPnLFxPrice,
    deal.GrossPnLSc,

    fees.TaxInfo,
    fees.OverallCommissions,
    fees.OverallOnBehalfFees,
    swaps.OverallFinancingCost,

    pnl.RealizedPnLDaySc,
    deal.RealisedPnLBtxSc,
    pnl.NettingOfPreviouslySettledPnLs

FROM deal
         INNER JOIN position
                    ON position.DealId = deal.DealId
         LEFT JOIN fees
                   ON fees.DealId = deal.DealId
         LEFT JOIN swaps
                   ON swaps.DealId = deal.DealId
         LEFT JOIN pnl
                   ON pnl.DealId = deal.DealId