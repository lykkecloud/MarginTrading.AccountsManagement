-- Copyright (c) 2020 BNP Paribas Arbitrage. All rights reserved.

-- exec sp_helptext [dbo.sp_removeTaxFileMissing]
-- exec sp_help [dbo.sp_removeTaxFileMissing]
-- exec [dbo].[sp_removeTaxFileMissing] 'yyyy-mm-dd', 

CREATE OR ALTER PROCEDURE [dbo].[sp_removeTaxFileMissing] (
    @TradingDate DATE
)
AS
BEGIN
    DELETE FROM [dbo].[TaxFileMissing]
    WHERE TradingDate = @TradingDate;
END