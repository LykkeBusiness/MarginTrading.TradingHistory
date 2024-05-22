-- dbo.Trades table update

IF EXISTS (SELECT 'X'
           FROM sys.indexes
           WHERE name = 'IX_Trades_Id_AccountId_AssetPairId_OrderCreatedDate'
             AND object_id = OBJECT_ID('dbo.Trades'))
    DROP INDEX IX_Trades_Id_AccountId_AssetPairId_OrderCreatedDate ON [dbo].[Trades];

ALTER TABLE [dbo].[Trades]
    ALTER COLUMN OrderCreatedDate datetime2 NOT NULL;

CREATE UNIQUE INDEX IX_Trades_Id_AccountId_AssetPairId_OrderCreatedDate
    ON [dbo].[Trades] (Id, AccountId, AssetPairId, OrderCreatedDate);

IF EXISTS (SELECT 'X'
           FROM sys.indexes
           WHERE name = 'IX_Trades'
             AND object_id = OBJECT_ID('dbo.Trades'))
    DROP INDEX IX_Trades ON [dbo].[Trades];
    
IF EXISTS (SELECT 'X'
           FROM sys.indexes
           WHERE name = 'IX_Trades_Id_AccountId_AssetPairId_TradeTimestamp_Volume'
             AND object_id = OBJECT_ID('dbo.Trades'))
    DROP INDEX IX_Trades_Id_AccountId_AssetPairId_TradeTimestamp_Volume on [dbo].[Trades];

ALTER TABLE [dbo].[Trades]
    ALTER COLUMN TradeTimestamp datetime2 NOT NULL;

CREATE INDEX IX_Trades
    ON [dbo].[Trades] (TradeTimestamp);

CREATE UNIQUE INDEX IX_Trades_Id_AccountId_AssetPairId_TradeTimestamp_Volume
    ON [dbo].[Trades] (Id, AccountId, AssetPairId, TradeTimestamp, Volume);

-- dbo.PositionsHistory table update

/*
- These indexes are only used in Lykke dev environment
DROP INDEX IX_PositionsHistory_AccountId_OpenDate_CloseDate_15A9F on [dbo].[PositionsHistory]
DROP INDEX IX_PositionsHistory_AccountId_HistoryTimestamp_2DFD0 on [dbo].[PositionsHistory]
 */

IF EXISTS (SELECT 'X'
           FROM sys.indexes
           WHERE name = 'IX_PositionsHistory_AccountId_OpenDate_CloseDate'
             AND object_id = OBJECT_ID('dbo.PositionsHistory'))
    DROP INDEX IX_PositionsHistory_AccountId_OpenDate_CloseDate on [dbo].[PositionsHistory]

ALTER TABLE [dbo].[PositionsHistory]
    ALTER COLUMN OpenDate datetime2 NOT NULL;

/*
- These indexes are only used in Lykke dev environment
CREATE INDEX IX_PositionsHistory_AccountId_OpenDate_CloseDate_15A9F
    ON dbo.PositionsHistory (AccountId, OpenDate, CloseDate) INCLUDE (Id, AssetPairId, Direction, Volume, OpenTradeId,
                                                                      OpenPrice, OpenFxPrice, ClosePrice, CloseFxPrice)

CREATE INDEX IX_PositionsHistory_AccountId_HistoryTimestamp_2DFD0
    ON dbo.PositionsHistory (AccountId, HistoryTimestamp) INCLUDE (Id, DealId, Code, AssetPairId, Direction, Volume,
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
 */

CREATE INDEX IX_PositionsHistory_AccountId_OpenDate_CloseDate
    ON [dbo].[PositionsHistory] (AccountId, OpenDate, CloseDate)