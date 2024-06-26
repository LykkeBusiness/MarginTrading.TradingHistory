IF NOT EXISTS(SELECT 'X'
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_NAME = 'PositionsHistory'
                AND TABLE_SCHEMA = 'dbo')
    BEGIN
        CREATE TABLE [dbo].[PositionsHistory]
        (
            [OID]                    [bigint]         NOT NULL IDENTITY (1,1) PRIMARY KEY,
            [Id]                     [nvarchar](64)   NOT NULL,
            [DealId]                 [nvarchar](128)  NULL,
            [Code]                   [bigint]         NULL,
            [AssetPairId]            [nvarchar](64)   NULL,
            [Direction]              [nvarchar](64)   NULL,
            [Volume]                 [float]          NULL,
            [AccountId]              [nvarchar](64)   NULL,
            [TradingConditionId]     [nvarchar](64)   NULL,
            [AccountAssetId]         [nvarchar](64)   NULL,
            [ExpectedOpenPrice]      [float]          NULL,
            [OpenMatchingEngineId]   [nvarchar](64)   NULL,
            [OpenDate]               [datetime2]      NULL,
            [OpenTradeId]            [nvarchar](64)   NULL,
            [OpenOrderType]          [nvarchar](64)   NULL,
            [OpenOrderVolume]        [float]          NULL,
            [OpenPrice]              [float]          NULL,
            [OpenFxPrice]            [float]          NULL,
            [EquivalentAsset]        [nvarchar](64)   NULL,
            [OpenPriceEquivalent]    [float]          NULL,
            [RelatedOrders]          [nvarchar](MAX)  NULL,
            [LegalEntity]            [nvarchar](64)   NULL,
            [OpenOriginator]         [nvarchar](64)   NULL,
            [ExternalProviderId]     [nvarchar](64)   NULL,
            [SwapCommissionRate]     [float]          NULL,
            [OpenCommissionRate]     [float]          NULL,
            [CloseCommissionRate]    [float]          NULL,
            [CommissionLot]          [float]          NULL,
            [CloseMatchingEngineId]  [nvarchar](64)   NULL,
            [ClosePrice]             [float]          NULL,
            [CloseFxPrice]           [float]          NULL,
            [ClosePriceEquivalent]   [float]          NULL,
            [StartClosingDate]       [datetime]       NULL,
            [CloseDate]              [datetime]       NULL,
            [CloseOriginator]        [nvarchar](64)   NULL,
            [CloseReason]            [nvarchar](256)  NULL,
            [CloseComment]           [nvarchar](MAX)  NULL,
            [CloseTrades]            [nvarchar](MAX)  NULL,
            [FxAssetPairId]          [nvarchar](64)   NULL,
            [FxToAssetPairDirection] [nvarchar](64)   NULL,
            [LastModified]           [datetime]       NULL,
            [TotalPnL]               [float]          NULL,
            [ChargedPnl]             [float]          NULL,
            [AdditionalInfo]         [nvarchar](MAX)  NULL,
            [HistoryType]            [nvarchar](64)   NULL,
            [DealInfo]               [nvarchar](MAX)  NULL,
            [HistoryTimestamp]       [datetime]       NULL,
            [ForceOpen]              [bit]            NULL,
            INDEX IX_PositionsHistory_Base (Id, AccountId, AssetPairId)
        );
    END;

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[PositionsHistory]') 
         AND name = 'CorrelationId'
)
BEGIN
ALTER TABLE [dbo].[PositionsHistory]
    ADD CorrelationId nvarchar(250) NULL;
END;
    
IF NOT EXISTS(
    SELECT 'X'
    FROM sys.indexes
    WHERE name = 'IX_PositionHistory_Id_DealId_AccountId_AssetPairId_Direction_Volume_HistoryTimestamp'
      AND object_id = OBJECT_ID('dbo.PositionsHistory'))
BEGIN
    CREATE UNIQUE INDEX IX_PositionHistory_Id_DealId_AccountId_AssetPairId_Direction_Volume_HistoryTimestamp
        ON PositionsHistory (Id, DealId, AccountId, AssetPairId, Direction, Volume, HistoryTimestamp)
END;

IF NOT EXISTS(
    SELECT 'X'
    FROM sys.indexes
    WHERE name = 'IX_PositionsHistory_HistoryType'
      AND object_id = OBJECT_ID('dbo.PositionsHistory'))
BEGIN
    CREATE INDEX [IX_PositionsHistory_HistoryType] 
        ON [dbo].[PositionsHistory] ([HistoryType])
END;

IF NOT EXISTS(
    SELECT 'X'
    FROM sys.indexes
    WHERE name = 'IX_PositionsHistory_AccountId_OpenDate_CloseDate'
      AND object_id = OBJECT_ID('dbo.PositionsHistory'))
BEGIN
    CREATE INDEX [IX_PositionsHistory_AccountId_OpenDate_CloseDate] 
        ON [dbo].[PositionsHistory] ([AccountId],[OpenDate], [CloseDate])
END;

-- for nvarchar columns the real length is this value divided by 2
-- 128 means it's [nvarchar](64)
if COL_LENGTH('[dbo].[PositionsHistory]', 'AssetPairId') = 128
BEGIN
ALTER TABLE PositionsHistory
ALTER COLUMN AssetPairId NVARCHAR(100)
END;