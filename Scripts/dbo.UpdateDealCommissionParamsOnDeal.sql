CREATE OR ALTER PROCEDURE [dbo].[UpdateDealCommissionParamsOnDeal](
                                                @DealId [nvarchar](64),
                                                @OpenTradeId [nvarchar](64),
                                                @OpenOrderVolume [float],
                                                @CloseTradeId [nvarchar](64),
                                                @CloseOrderVolume [float],
                                                @Volume [float]
)
AS
BEGIN
    SET NOCOUNT ON;

    /*
    CAUTION: similar calculation logic is duplicated here and in UpdateDealCommissionParamsOnAccountHistory
    */
    WITH selectedAccounts AS
             (
                 SELECT account.EventSourceId,
                        account.ReasonType,
                        account.ChangeAmount
                 FROM dbo.AccountHistory AS account
                 WHERE account.EventSourceId IN (@OpenTradeId, @CloseTradeId)
             )
    UPDATE [dbo].[DealCommissionParams]
    SET [OvernightFees] =
            (SELECT CONVERT(DECIMAL(24, 13), Sum(swapHistory.SwapValue / ABS(swapHistory.Volume)) * ABS(@Volume))
             FROM dbo.PositionsHistory AS position,
                  dbo.OvernightSwapHistory AS swapHistory
             WHERE position.DealId = @DealId
               AND position.Id = swapHistory.PositionId
               AND swapHistory.IsSuccess = 1
            ),
        [Commission]    = (
            SELECT CONVERT(DECIMAL(24, 13),
                           ((ISNULL((SELECT TOP (1) ChangeAmount FROM selectedAccounts openingCommission
                                     WHERE openingCommission.EventSourceId = @OpenTradeId
                                       AND openingCommission.ReasonType = 'Commission'), 0.0)
                                 / ABS(@OpenOrderVolume)
                               + ISNULL((SELECT TOP (1) ChangeAmount FROM selectedAccounts closingCommission
                                         WHERE closingCommission.EventSourceId = @CloseTradeId
                                           AND closingCommission.ReasonType = 'Commission'), 0.0)
                                 / ABS(@CloseOrderVolume))
                               * ABS(@Volume)))
        ),
        [OnBehalfFee]   = (
            SELECT CONVERT(DECIMAL(24, 13),
                           ((ISNULL((SELECT TOP(1) ChangeAmount FROM selectedAccounts openingOnBehalf
                                     WHERE openingOnBehalf.EventSourceId = @OpenTradeId 
                                       AND openingOnBehalf.ReasonType = 'OnBehalf'), 0.0)
                                 / ABS(@OpenOrderVolume)
                               + ISNULL((SELECT TOP(1) ChangeAmount FROM selectedAccounts closingOnBehalf
                                         WHERE closingOnBehalf.EventSourceId = @CloseTradeId 
                                           AND closingOnBehalf.ReasonType = 'OnBehalf'), 0.0)
                                 / ABS(@CloseOrderVolume))
                               * ABS(@Volume)))
        ),
        [Taxes]         = (
            SELECT CONVERT(DECIMAL(24, 13), ISNULL(account.ChangeAmount, 0.0))
            FROM [dbo].[AccountHistory] account
            WHERE account.EventSourceId = @DealId
              AND account.ReasonType = 'Tax' -- it could also be CompensationId, so it is automatically skipped
        )
    WHERE [dbo].[DealCommissionParams].DealId = @DealId;

END;