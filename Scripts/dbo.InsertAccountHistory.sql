CREATE OR ALTER PROCEDURE [dbo].[SP_InsertAccountHistory](@Id [nvarchar](128),
                                                          @AccountId [nvarchar](64),
                                                          @ChangeTimestamp [datetime],
                                                          @ClientId [nvarchar](64),
                                                          @ChangeAmount decimal(24, 12),
                                                          @Balance decimal(24, 12),
                                                          @WithdrawTransferLimit decimal(24, 12),
                                                          @Comment [nvarchar](MAX),
                                                          @ReasonType [nvarchar](64),
                                                          @EventSourceId [nvarchar](128),
                                                          @LegalEntity [nvarchar](64),
                                                          @AuditLog [nvarchar](MAX),
                                                          @Instrument [nvarchar](64),
                                                          @TradingDate [datetime])
AS
BEGIN
    SET NOCOUNT ON;

    IF @eventSourceId IS NULL AND @reasonType IN ('Swap', 'Commission', 'OnBehalf', 'Tax')
        RAISERROR ('EventSourceId was null, reason type [%s]', 1, 1, @reasonType);
        
    INSERT INTO [dbo].[AccountHistory]
    (Id, AccountId, ChangeTimestamp, ClientId, ChangeAmount, Balance, WithdrawTransferLimit, Comment,
     ReasonType, EventSourceId, LegalEntity, AuditLog, Instrument, TradingDate)
    VALUES
    (@Id, @AccountId, @ChangeTimestamp, @ClientId, @ChangeAmount, @Balance, @WithdrawTransferLimit, @Comment,
     @ReasonType, @EventSourceId, @LegalEntity, @AuditLog, @Instrument, @TradingDate)

    IF @reasonType = 'Swap'
        BEGIN   
            UPDATE [dbo].[Deals]
            SET [OvernightFees] =
                    (SELECT CONVERT(DECIMAL(24,13), Sum(swapHistory.SwapValue / ABS(swapHistory.Volume)) * ABS(deal.Volume))
                     FROM dbo.[Deals] AS deal,
                          dbo.PositionsHistory AS position,
                          dbo.OvernightSwapHistory AS swapHistory
                     WHERE position.Id = @eventSourceId
                       AND deal.DealId = position.DealId
                       AND position.Id = swapHistory.PositionId AND swapHistory.IsSuccess = 1
                     GROUP BY deal.DealId, ABS(deal.Volume)
                    )
            FROM dbo.PositionsHistory AS position
            WHERE [Deals].DealId = position.DealId AND position.Id = @eventSourceId
        END

    IF @reasonType = 'Commission'
        BEGIN
            WITH selectedAccounts AS
                     (
                         SELECT account.EventSourceId,
                                account.ReasonType,
                                account.ChangeAmount
                         FROM dbo.[Deals] AS deal, dbo.AccountHistory AS account
                         WHERE(account.EventSourceId IN (deal.OpenTradeId, deal.CloseTradeId) AND account.ReasonType = 'Commission')
                     )
            UPDATE [dbo].[Deals]
            SET [Commission] = data.amount
            FROM (
                     SELECT DISTINCT deal.DealId, CONVERT(DECIMAL(24,13), ((ISNULL(openingCommission.ChangeAmount, 0.0) / ABS(deal.OpenOrderVolume)
                         + ISNULL(closingCommission.ChangeAmount, 0.0) / ABS(deal.CloseOrderVolume))
                         * ABS(deal.Volume))) amount
                     FROM dbo.[Deals] AS deal
                              INNER JOIN selectedAccounts openingCommission
                                         ON deal.OpenTradeId = openingCommission.EventSourceId AND openingCommission.ReasonType = 'Commission'
                              LEFT OUTER JOIN selectedAccounts closingCommission
                                              ON deal.CloseTradeId = closingCommission.EventSourceId AND closingCommission.ReasonType = 'Commission'
                     WHERE deal.OpenTradeId = @eventSourceId OR deal.CloseTradeId = @eventSourceId
                 ) data
            WHERE [dbo].[Deals].DealId = data.DealId
        END

    IF @reasonType = 'OnBehalf'
        BEGIN
            WITH selectedAccounts AS
                     (
                         SELECT DISTINCT account.EventSourceId,
                                         account.ReasonType,
                                         account.ChangeAmount
                         FROM dbo.[Deals] AS deal, dbo.AccountHistory AS account
                         WHERE(account.EventSourceId IN (deal.OpenTradeId, deal.CloseTradeId) AND account.ReasonType = 'OnBehalf')
                     )
            UPDATE [dbo].[Deals]
            SET [OnBehalfFee] = data.amount
            FROM (
                     SELECT DISTINCT deal.DealId, CONVERT(DECIMAL(24,13), ((ISNULL(openingOnBehalf.ChangeAmount, 0.0) / ABS(deal.OpenOrderVolume)
                         + ISNULL(closingOnBehalf.ChangeAmount, 0.0) / ABS(deal.CloseOrderVolume))
                         * ABS(deal.Volume))) amount
                     FROM [dbo].[Deals] deal
                              LEFT OUTER JOIN selectedAccounts openingOnBehalf
                                              ON deal.OpenTradeId = openingOnBehalf.EventSourceId AND openingOnBehalf.ReasonType = 'OnBehalf'
                              LEFT OUTER JOIN selectedAccounts closingOnBehalf
                                              ON deal.CloseTradeId = closingOnBehalf.EventSourceId AND closingOnBehalf.ReasonType = 'OnBehalf'
                     WHERE deal.OpenTradeId = @eventSourceId OR deal.CloseTradeId = @eventSourceId
                 ) data
            WHERE [dbo].[Deals].DealId = data.DealId
        END

    IF @reasonType = 'Tax'
        BEGIN
            UPDATE [dbo].[Deals]
            SET [Taxes] =
                    (
                        SELECT CONVERT(DECIMAL(24,13), ISNULL(account.ChangeAmount, 0.0))
                        FROM [dbo].[Deals] deal, [dbo].[AccountHistory] account
                        WHERE account.EventSourceId = deal.DealId AND account.ReasonType = 'Tax'
                          AND deal.DealId = @eventSourceId
                    )
            WHERE [Deals].DealId = @eventSourceId -- it could also be CompensationId, so it is automatically skipped
        END

END;