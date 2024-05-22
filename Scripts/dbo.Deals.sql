IF NOT EXISTS(SELECT 'X'
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_NAME = 'Deals'
                AND TABLE_SCHEMA = 'dbo')
    BEGIN
        CREATE TABLE [dbo].[Deals]
        (
            [OID]                     [bigint]        NOT NULL IDENTITY (1,1) PRIMARY KEY,
            [DealId]                  [nvarchar](64)  NOT NULL,
            [Created]                 [datetime]      NOT NULL,
            [AccountId]               [nvarchar](64)  NOT NULL,
            [AssetPairId]             [nvarchar](64)  NOT NULL,
            [OpenTradeId]             [nvarchar](64)  NOT NULL,
            [OpenOrderType]           [nvarchar](64)  NULL,
            [OpenOrderVolume]         [float]         NULL,
            [OpenOrderExpectedPrice]  [float]         NULL,
            [CloseTradeId]            [nvarchar](64)  NULL,
            [CloseOrderType]          [nvarchar](64)  NULL,
            [CloseOrderVolume]        [float]         NULL,
            [CloseOrderExpectedPrice] [float]         NULL,
            [Direction]               [nvarchar](64)  NOT NULL,
            [Volume]                  [float]         NULL,
            [Originator]              [nvarchar](64)  NOT NULL,
            [OpenPrice]               [float]         NULL,
            [OpenFxPrice]             [float]         NULL,
            [ClosePrice]              [float]         NULL,
            [CloseFxPrice]            [float]         NULL,
            [Fpl]                     [float]         NULL,
            [PnlOfTheLastDay]         [float]         NULL,
            [AdditionalInfo]          [nvarchar](MAX) NULL,
            INDEX IX_Deals_Base (DealId, AccountId, AssetPairId, Created)
        );
    END;

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Deals]') 
         AND name = 'CorrelationId'
)
BEGIN
ALTER TABLE [dbo].[Deals]
    ADD CorrelationId nvarchar(250) NULL;
END
    
IF NOT EXISTS(
    SELECT 'X'
    FROM sys.indexes
    WHERE name = 'IX_Deals_DealId_AccountId_AssetPairId_Direction_Volume_Created'
      AND object_id = OBJECT_ID('dbo.Deals'))
BEGIN
    CREATE UNIQUE INDEX IX_Deals_DealId_AccountId_AssetPairId_Direction_Volume_Created
        ON Deals (DealId, AccountId, AssetPairId, Direction, Volume, Created)
END;


-- for nvarchar columns the real length is this value divided by 2
-- 128 means it's [nvarchar](64)
if COL_LENGTH('[dbo].[Deals]', 'AssetPairId') = 128
BEGIN
ALTER TABLE Deals
ALTER COLUMN AssetPairId NVARCHAR(100)
END;