IF NOT EXISTS(SELECT 'X'
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_NAME = 'OrdersHistory'
                AND TABLE_SCHEMA = 'dbo')
    BEGIN

        CREATE TABLE [dbo].[OrdersHistory]
        (
            [OID]                       [bigint]        NOT NULL IDENTITY (1,1),
            [Id]                        [nvarchar](64)  NOT NULL,
            [Code]                      [bigint]        NULL,
            [AccountId]                 [nvarchar](64)  NULL,
            [AssetPairId]               [nvarchar](64)  NULL,
            [ParentOrderId]             [nvarchar](64)  NULL,
            [PositionId]                [nvarchar](64)  NULL,
            [Direction]                 [nvarchar](64)  NULL,
            [Type]                      [nvarchar](64)  NULL,
            [Status]                    [nvarchar](64)  NULL,
            [FillType]                  [nvarchar](64)  NULL,
            [Originator]                [nvarchar](64)  NULL,
            [CancellationOriginator]    [nvarchar](64)  NULL,
            [Volume]                    [float]         NULL,
            [ExpectedOpenPrice]         [float]         NULL,
            [ExecutionPrice]            [float]         NULL,
            [FxRate]                    [float]         NULL,
            [FxAssetPairId]             [nvarchar](64)  NULL,
            [FxToAssetPairDirection]    [nvarchar](64)  NULL,
            [ForceOpen]                 [bit]           NULL,
            [ValidityTime]              [datetime]      NULL,
            [CreatedTimestamp]          [datetime]      NULL,
            [ModifiedTimestamp]         [datetime]      NULL,
            [ActivatedTimestamp]        [datetime]      NULL,
            [ExecutionStartedTimestamp] [datetime]      NULL,
            [ExecutedTimestamp]         [datetime]      NULL,
            [CanceledTimestamp]         [datetime]      NULL,
            [Rejected]                  [datetime]      NULL,
            [TradingConditionId]        [nvarchar](64)  NULL,
            [AccountAssetId]            [nvarchar](64)  NULL,
            [EquivalentAsset]           [nvarchar](64)  NULL,
            [EquivalentRate]            [float]         NULL,
            [RejectReason]              [nvarchar](64)  NULL,
            [RejectReasonText]          [nvarchar](MAX) NULL,
            [Comment]                   [nvarchar](MAX) NULL,
            [ExternalOrderId]           [nvarchar](64)  NULL,
            [ExternalProviderId]        [nvarchar](64)  NULL,
            [MatchingEngineId]          [nvarchar](64)  NULL,
            [LegalEntity]               [nvarchar](64)  NULL,
            [UpdateType]                [nvarchar](64)  NULL,
            [MatchedOrders]             [nvarchar](MAX) NULL,
            [RelatedOrderInfos]         [nvarchar](MAX) NULL,
            [AdditionalInfo]            [nvarchar](MAX) NULL,
            [CorrelationId]             [nvarchar](64)  NULL,
            [PendingOrderRetriesCount]  [int]           NULL,
            CONSTRAINT PK_OrdersHistory_OID PRIMARY KEY CLUSTERED (OID DESC),
            INDEX IX_OrdersHistory_Base (Id, AccountId, AssetPairId, Status, ParentOrderId, ExecutedTimestamp,
                                         CreatedTimestamp, ModifiedTimestamp, Type, Originator)
        );

        CREATE INDEX IX_OrdersHistory_Child ON [dbo].[OrdersHistory]
            (ParentOrderId, Type) include (Id, Status, ExpectedOpenPrice, ModifiedTimestamp);
        CREATE INDEX IX_OrdersHistory_ExecutedTimestamp ON OrdersHistory(ExecutedTimestamp DESC);

    END;

BEGIN
    ALTER TABLE [dbo].[OrdersHistory]
    ALTER COLUMN CorrelationId NVARCHAR(250) NULL;
END;

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'CreatedBy'
          AND Object_ID = Object_ID(N'[dbo].[OrdersHistory]'))
BEGIN
    alter table [dbo].[OrdersHistory] add CreatedBy as CASE WHEN ISJSON(AdditionalInfo) > 0 THEN CAST(json_value(AdditionalInfo, '$.CreatedBy') AS NVARCHAR(256)) ELSE null END
    create NONCLUSTERED INDEX IX_ParsedCreatedBy on [dbo].[OrdersHistory] (CreatedBy)
END;