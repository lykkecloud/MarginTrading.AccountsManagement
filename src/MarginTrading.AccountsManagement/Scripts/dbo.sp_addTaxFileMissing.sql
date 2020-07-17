-- Copyright (c) 2020 BNP Paribas Arbitrage. All rights reserved.

-- exec sp_helptext [dbo.sp_addTaxFileMissing]
-- exec sp_help [dbo.sp_addTaxFileMissing]
-- exec [dbo].[sp_addTaxFileMissing] 'yyyy-mm-dd', 

CREATE OR ALTER PROCEDURE [dbo].[sp_addTaxFileMissing] (
    @TradingDate DATE
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
            SET NOCOUNT OFF;
    
            IF NOT EXISTS (
                SELECT 1
                FROM [dbo].[TaxFileMissing] (NOLOCK)
                WHERE TradingDate = @TradingDate
            )
            BEGIN
                INSERT 
                INTO [dbo].[TaxFileMissing] (TradingDate)
                VALUES (@TradingDate)
            END

    	COMMIT
    END TRY
    BEGIN CATCH
    	IF @@TRANCOUNT > 0
    		ROLLBACK;
    	THROW;
    END CATCH
END