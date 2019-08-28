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
            [OvernightFees]           [float]         NULL,
            [Commission]              [float]         NULL,
            [OnBehalfFee]             [float]         NULL,
            [Taxes]                   [float]         NULL,
            INDEX IX_Deals_Base (DealId, AccountId, AssetPairId, Created)
        );
    END;