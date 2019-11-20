CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateDealCommissionParamsOnAccountHistory](
    @ChangeAmount decimal(24, 12),
    @ReasonType [nvarchar](64),
    @EventSourceId [nvarchar](128)
)
AS
BEGIN
    SET NOCOUNT ON;

    /*
    CAUTION: similar calculation logic is duplicated here and in SP_UpdateDealCommissionParamsOnDeal
    */
    IF @reasonType = 'Swap'
        BEGIN
            UPDATE [dbo].[DealCommissionParams]
            SET [OvernightFees] = data.amount
            FROM (
                     SELECT DISTINCT deal.DealId,
                                     CONVERT(DECIMAL(24,13), 
                                         SUM(swapHistory.SwapValue / ABS(swapHistory.Volume)) 
                                             * ABS(deal.Volume)) amount
                     FROM dbo.[Deals] AS deal,
                          dbo.PositionsHistory AS position,
                          dbo.OvernightSwapHistory AS swapHistory
                     WHERE position.Id = @EventSourceId
                       AND deal.DealId = position.DealId
                       AND position.Id = swapHistory.PositionId AND swapHistory.IsSuccess = 1
                     GROUP BY deal.DealId, ABS(deal.Volume)
                    ) data
            WHERE [dbo].[DealCommissionParams].DealId = data.DealId
        END

    IF @reasonType = 'Commission'
        BEGIN
            UPDATE [dbo].[DealCommissionParams]
            SET [Commission] = data.amount
            FROM (
                     SELECT DISTINCT deal.DealId,
                                     CONVERT(DECIMAL(24,13),
                                             ((ISNULL(openingCommission.ChangeAmount, 0.0) / ABS(deal.OpenOrderVolume)
                                                 + ISNULL(closingCommission.ChangeAmount, 0.0) / ABS(deal.CloseOrderVolume))
                                                 * ABS(deal.Volume))) amount
                     FROM dbo.[Deals] AS deal
                              JOIN [dbo].[AccountHistory] openingCommission
                                   ON deal.OpenTradeId = openingCommission.EventSourceId AND openingCommission.ReasonType = 'Commission'
                              LEFT JOIN [dbo].[AccountHistory] closingCommission
                                        ON deal.CloseTradeId = closingCommission.EventSourceId AND closingCommission.ReasonType = 'Commission'
                     WHERE deal.OpenTradeId = @EventSourceId OR deal.CloseTradeId = @EventSourceId
                 ) data
            WHERE [dbo].[DealCommissionParams].DealId = data.DealId
        END

    IF @reasonType = 'OnBehalf'
        BEGIN
            UPDATE [dbo].[DealCommissionParams]
            SET [OnBehalfFee] = data.amount
            FROM (
                     SELECT DISTINCT deal.DealId,
                                     CONVERT(DECIMAL(24,13),
                                             ((ISNULL(openingOnBehalf.ChangeAmount, 0.0) / ABS(deal.OpenOrderVolume)
                                                 + ISNULL(closingOnBehalf.ChangeAmount, 0.0) / ABS(deal.CloseOrderVolume))
                                                 * ABS(deal.Volume))) amount
                     FROM [dbo].[Deals] deal
                              LEFT OUTER JOIN [dbo].[AccountHistory] openingOnBehalf
                                   ON deal.OpenTradeId = openingOnBehalf.EventSourceId AND openingOnBehalf.ReasonType = 'OnBehalf'
                              LEFT OUTER JOIN [dbo].[AccountHistory] closingOnBehalf
                                        ON deal.CloseTradeId = closingOnBehalf.EventSourceId AND closingOnBehalf.ReasonType = 'OnBehalf'
                     WHERE deal.OpenTradeId = @EventSourceId OR deal.CloseTradeId = @EventSourceId
                 ) data
            WHERE [dbo].[DealCommissionParams].DealId = data.DealId
        END

    IF @reasonType = 'Tax'
        BEGIN
            UPDATE [dbo].[DealCommissionParams]
            SET [Taxes] = CONVERT(DECIMAL(24,13), ISNULL(@ChangeAmount, 0.0))
            WHERE [dbo].[DealCommissionParams].DealId = @EventSourceId -- it could also be CompensationId, so it is automatically skipped
        END

END;