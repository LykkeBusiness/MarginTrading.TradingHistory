IF NOT EXISTS(SELECT 'X'
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_NAME = 'Trades'
                AND TABLE_SCHEMA = 'dbo')
    BEGIN

        CREATE TABLE [dbo].[Trades]
        (
            [OID]                [bigint]        NOT NULL IDENTITY (1,1) PRIMARY KEY,
            [Id]                 [nvarchar](64)  NOT NULL,
            [AccountId]          [nvarchar](64)  NOT NULL,
            [OrderId]            [nvarchar](64)  NOT NULL,
            [AssetPairId]        [nvarchar](64)  NOT NULL,
            [OrderCreatedDate]   [datetime]      NOT NULL,
            [OrderType]          [nvarchar](64)  NOT NULL,
            [Type]               [nvarchar](64)  NOT NULL,
            [Originator]         [nvarchar](64)  NOT NULL,
            [TradeTimestamp]     [datetime]      NOT NULL,
            [Price]              [float]         NULL,
            [Volume]             [float]         NULL,
            [OrderExpectedPrice] [float]         NULL,
            [FxRate]             [float]         NULL,
            [AdditionalInfo]     [nvarchar](MAX) NULL,
            [CancelledBy]        [nvarchar](64)  NULL,
            INDEX IX_Trades_Base (AccountId, AssetPairId)
        );

    END
	
	
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Trades]') 
         AND name = 'ExternalOrderId'
)
BEGIN
	ALTER TABLE [dbo].[Trades]
	ADD ExternalOrderId nvarchar(64) NULL;
END