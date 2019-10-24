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

        --this is a form of migration. no harm would be done when deploying new instance of the system.
        INSERT INTO [dbo].[DealCommissionParams]
        SELECT d.DealId, d.OvernightFees, d.Commission, d.OnBehalfFee, d.Taxes FROM [dbo].[Deals] d;
        
    END;