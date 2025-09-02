namespace BLL.Database.Migrations
{
    /// <summary>
    /// Adiciona campos de auditoria às tabelas principais
    /// </summary>
    public class Migration_20250102130000_AddAuditFields : Migration
    {
        public override string Version => "20250102130000";
        public override string Description => "Adiciona campos de auditoria e triggers de atualização";

        public override string UpScript => @"
-- Adiciona campos de auditoria à tabela Tickets
ALTER TABLE [Tickets] ADD [UltimaAtualizacaoPor] INT NULL;
ALTER TABLE [Tickets] ADD FOREIGN KEY ([UltimaAtualizacaoPor]) REFERENCES [Utilizadores]([Id]);
GO

-- Cria trigger para atualizar automaticamente o campo AtualizadoEm
CREATE TRIGGER [TR_Tickets_UpdateTimestamp]
ON [Tickets]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Tickets] 
    SET [AtualizadoEm] = GETUTCDATE()
    FROM [Tickets] t
    INNER JOIN inserted i ON t.[Id] = i.[Id];
END;
GO

-- Cria trigger similar para Utilizadores
CREATE TRIGGER [TR_Utilizadores_UpdateTimestamp]
ON [Utilizadores]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Utilizadores] 
    SET [AtualizadoEm] = GETUTCDATE()
    FROM [Utilizadores] u
    INNER JOIN inserted i ON u.[Id] = i.[Id];
END;
GO

-- Adiciona tabela de log de ações
CREATE TABLE [LogAcoes] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketId] INT NULL,
    [UtilizadorId] INT NOT NULL,
    [Acao] NVARCHAR(50) NOT NULL, -- 'CRIADO', 'ATUALIZADO', 'ELIMINADO', 'ESTADO_ALTERADO'
    [Detalhes] NVARCHAR(500) NULL,
    [ValorAnterior] NVARCHAR(200) NULL,
    [ValorNovo] NVARCHAR(200) NULL,
    [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([UtilizadorId]) REFERENCES [Utilizadores]([Id])
);
GO

-- Índice para consultas de log por ticket
CREATE INDEX [IX_LogAcoes_TicketId] ON [LogAcoes]([TicketId]);
CREATE INDEX [IX_LogAcoes_UtilizadorId] ON [LogAcoes]([UtilizadorId]);
CREATE INDEX [IX_LogAcoes_CriadoEm] ON [LogAcoes]([CriadoEm]);
GO";

        public override string DownScript => @"
-- Remove índices da tabela de log
DROP INDEX IF EXISTS [IX_LogAcoes_CriadoEm] ON [LogAcoes];
DROP INDEX IF EXISTS [IX_LogAcoes_UtilizadorId] ON [LogAcoes];
DROP INDEX IF EXISTS [IX_LogAcoes_TicketId] ON [LogAcoes];
GO

-- Remove tabela de log
DROP TABLE IF EXISTS [LogAcoes];
GO

-- Remove triggers
DROP TRIGGER IF EXISTS [TR_Utilizadores_UpdateTimestamp];
DROP TRIGGER IF EXISTS [TR_Tickets_UpdateTimestamp];
GO

-- Remove campos de auditoria
ALTER TABLE [Tickets] DROP CONSTRAINT IF EXISTS [FK_Tickets_UltimaAtualizacaoPor];
ALTER TABLE [Tickets] DROP COLUMN IF EXISTS [UltimaAtualizacaoPor];
GO";
    }
}