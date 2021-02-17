if not exists (select 1 from sys.objects where name = 'MarginTradingAccountsComplexityWarnings' and schema_id = schema_id('dbo'))
begin
   create table dbo.MarginTradingAccountsComplexityWarnings(
		[AccountId] nvarchar (64) not null  primary key,
		[ConfirmedOrders] nvarchar (max) not null,
		[RowVersion] rowversion not null,
		[ShouldShowComplexityWarning] bit not null,
		[SwitchedToFalseAt] datetime null
	)
end

if not exists(
	select 1 from sys.indexes 
	where name = 'IX_MarginTradingAccountsComplexityWarnings_SwitchedToFalseAt'
	and object_id = OBJECT_ID('dbo.MarginTradingAccountsComplexityWarnings'))
begin
    create nonclustered index IX_MarginTradingAccountsComplexityWarnings_SwitchedToFalseAt on dbo.MarginTradingAccountsComplexityWarnings(SwitchedToFalseAt asc)
end

