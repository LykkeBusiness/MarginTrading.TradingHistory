-- dbo.Trades table update

DROP INDEX IX_Trades_Id_AccountId_AssetPairId_OrderCreatedDate ON [dbo].[Trades];

ALTER TABLE [dbo].[Trades]
    ALTER COLUMN OrderCreatedDate datetime2 NOT NULL;

CREATE UNIQUE INDEX IX_Trades_Id_AccountId_AssetPairId_OrderCreatedDate
    ON [dbo].[Trades] (Id, AccountId, AssetPairId, OrderCreatedDate);

DROP INDEX IX_Trades on [dbo].[Trades];
DROP INDEX IX_Trades_Id_AccountId_AssetPairId_TradeTimestamp_Volume on [dbo].[Trades];

ALTER TABLE [dbo].[Trades]
    ALTER COLUMN TradeTimestamp datetime2 NOT NULL;

CREATE INDEX IX_Trades
    ON dbo.Trades (TradeTimestamp);

CREATE UNIQUE INDEX IX_Trades_Id_AccountId_AssetPairId_TradeTimestamp_Volume
    ON [dbo].[Trades] (Id, AccountId, AssetPairId, TradeTimestamp, Volume);

-- dbo.PositionsHistory table update

DROP INDEX IX_PositionsHistory_AccountId_OpenDate_CloseDate_15A9F on [dbo].[PositionsHistory]
DROP INDEX IX_PositionsHistory_AccountId_HistoryTimestamp_2DFD0 on [dbo].[PositionsHistory]
DROP INDEX IX_PositionsHistory_AccountId_OpenDate_CloseDate on [dbo].[PositionsHistory]

ALTER TABLE [dbo].[PositionsHistory]
    ALTER COLUMN OpenDate datetime2 NOT NULL;

create index IX_PositionsHistory_AccountId_OpenDate_CloseDate_15A9F
    on dbo.PositionsHistory (AccountId, OpenDate, CloseDate) include (Id, AssetPairId, Direction, Volume, OpenTradeId,
                                                                      OpenPrice, OpenFxPrice, ClosePrice, CloseFxPrice)

create index IX_PositionsHistory_AccountId_HistoryTimestamp_2DFD0
    on dbo.PositionsHistory (AccountId, HistoryTimestamp) include (Id, DealId, Code, AssetPairId, Direction, Volume,
                                                                   TradingConditionId, AccountAssetId,
                                                                   ExpectedOpenPrice, OpenMatchingEngineId, OpenDate,
                                                                   OpenTradeId, OpenPrice, OpenFxPrice, EquivalentAsset,
                                                                   OpenPriceEquivalent, RelatedOrders, LegalEntity,
                                                                   OpenOriginator, ExternalProviderId,
                                                                   SwapCommissionRate, OpenCommissionRate,
                                                                   CloseCommissionRate, CommissionLot,
                                                                   CloseMatchingEngineId, ClosePrice, CloseFxPrice,
                                                                   ClosePriceEquivalent, StartClosingDate, CloseDate,
                                                                   CloseOriginator, CloseReason, CloseComment,
                                                                   CloseTrades, LastModified, TotalPnL, ChargedPnl,
                                                                   HistoryType, DealInfo, OpenOrderType,
                                                                   OpenOrderVolume, FxAssetPairId,
                                                                   FxToAssetPairDirection, AdditionalInfo, ForceOpen,
                                                                   CorrelationId)

create index IX_PositionsHistory_AccountId_OpenDate_CloseDate
    on dbo.PositionsHistory (AccountId, OpenDate, CloseDate)