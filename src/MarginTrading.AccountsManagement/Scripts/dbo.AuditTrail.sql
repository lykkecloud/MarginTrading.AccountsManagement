if not exists (select 1 from sys.objects where name = 'MarginTradingAccountsAuditTrail' and schema_id = schema_id('dbo'))
begin
CREATE TABLE [dbo].[MarginTradingAccountsAuditTrail](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Timestamp] [datetime2](7) NOT NULL,
    [CorrelationId] [nvarchar](max) NOT NULL,
    [UserName] [nvarchar](max) NOT NULL,
    [Type] [nvarchar](max) NOT NULL,
    [DataType] [nvarchar](max) NOT NULL,
    [DataReference] [nvarchar](max) NOT NULL,
    [DataDiff] [nvarchar](max) NOT NULL);

    ALTER TABLE [dbo].[MarginTradingAccountsAuditTrail] ADD  CONSTRAINT [PK_MarginTradingAccountsAuditTrail] PRIMARY KEY CLUSTERED ([Id] ASC)
end

if not exists(
	select 1 from sys.indexes 
	where name = 'IX_MarginTradingAccountsAuditTrail_Timestamp'
	and object_id = OBJECT_ID('dbo.MarginTradingAccountsAuditTrail'))
begin
    create nonclustered index IX_MarginTradingAccountsAuditTrail_Timestamp on dbo.MarginTradingAccountsAuditTrail([Timestamp] asc)
end