IF NOT EXISTS(SELECT 'X'
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_NAME = 'DealCommissionParams'
                AND TABLE_SCHEMA = 'dbo')
    BEGIN

        CREATE TABLE [dbo].[DealCommissionParams]
        (
            [OID]                     [bigint]        NOT NULL IDENTITY (1,1) PRIMARY KEY,
            [DealId]                  [nvarchar](64)  NOT NULL UNIQUE,
            [OvernightFees]           [float]         NULL,
            [Commission]              [float]         NULL,
            [OnBehalfFee]             [float]         NULL,
            [Taxes]                   [float]         NULL,
            INDEX IX_DealCommissionParams_Base (DealId)
        );
        
    END;