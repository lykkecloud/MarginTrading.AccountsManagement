-- Copyright (c) 2020 BNP Paribas Arbitrage. All rights reserved.

-- exec sp_helptext [dbo.removeTaxFileMissing]
-- exec sp_help [dbo.removeTaxFileMissing]
-- exec [dbo].[removeTaxFileMissing] 'yyyy-mm-dd', 

CREATE OR ALTER PROCEDURE [dbo].[getTaxFileMissing]
AS
BEGIN
    SELECT TradingDate
    FROM [dbo].[TaxFileMissing]
    ORDER BY TradingDate
END