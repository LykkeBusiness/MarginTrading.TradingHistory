IF NOT EXISTS(SELECT 'X'
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_NAME = 'Correlations'
                AND TABLE_SCHEMA = 'dbo')
BEGIN

CREATE TABLE [dbo].[Correlations]
(
    [Id]                        [nvarchar](64)  NOT NULL PRIMARY KEY,
    [CorrelationId]             [nvarchar](250) NOT NULL,
    [EntityType]                [nvarchar](100) NOT NULL,
    [EntityId]                  [nvarchar](250) NOT NULL,
    [Timestamp]                 [datetime]      NOT NULL
    );

END;