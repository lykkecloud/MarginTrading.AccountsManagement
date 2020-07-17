-- Copyright (c) 2020 BNP Paribas Arbitrage. All rights reserved.

-- Create the table if the table doesn't already exist
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE [name] = 'TaxFileMissing' AND schema_id = schema_id('dbo'))
BEGIN
   CREATE TABLE [dbo].[TaxFileMissing](
		[Oid] [bigint] IDENTITY(1,1) NOT NULL,
		[TradingDate] [date] NOT NULL
	PRIMARY KEY CLUSTERED 
	(
		[Oid] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]
END

-- Create index on TradingDate column.
IF NOT EXISTS(
	SELECT * FROM sys.indexes 
	WHERE name = 'IX_TaxFileMissing_TradingDate'
	AND object_id = OBJECT_ID('dbo.TaxFileMissing'))
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [IX_TaxFileMissing_TradingDate] ON [dbo].[TaxFileMissing]
		(
			[TradingDate] ASC
		)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
    END

