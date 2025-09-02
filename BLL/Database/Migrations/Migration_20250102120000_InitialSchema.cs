namespace BLL.Database.Migrations
{
    /// <summary>
    /// Migração inicial - Criação do schema base
    /// </summary>
    public class Migration_20250102120000_InitialSchema : Migration
    {
        public override string Version => "20250102120000";
        public override string Description => "Criação do schema inicial do sistema";

        public override string UpScript => @"
-- Criação da tabela de Utilizadores
CREATE TABLE [Utilizadores] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Username] NVARCHAR(50) NOT NULL UNIQUE,
    [Password] NVARCHAR(255) NOT NULL,
    [Nome] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL,
    [Tipo] INT NOT NULL, -- 0=Colaborador, 1=Tecnico
    [Ativo] BIT NOT NULL DEFAULT 1,
    [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [AtualizadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- Criação da tabela de Tickets
CREATE TABLE [Tickets] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Titulo] NVARCHAR(200) NOT NULL,
    [Descricao] NVARCHAR(1000) NOT NULL,
    [Estado] INT NOT NULL DEFAULT 0, -- 0=Pendente, 1=EmAtendimento, 2=Atendido
    [Prioridade] INT NOT NULL DEFAULT 1, -- 0=Baixa, 1=Media, 2=Alta, 3=Critica
    [ColaboradorId] INT NOT NULL,
    [TecnicoId] INT NULL,
    [CriadoPorId] INT NOT NULL,
    [CriadoPorTecnico] BIT NOT NULL DEFAULT 0,
    [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [AtualizadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([ColaboradorId]) REFERENCES [Utilizadores]([Id]),
    FOREIGN KEY ([TecnicoId]) REFERENCES [Utilizadores]([Id]),
    FOREIGN KEY ([CriadoPorId]) REFERENCES [Utilizadores]([Id])
);
GO

-- Criação da tabela de Detalhes de Hardware
CREATE TABLE [DetalhesHardware] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketId] INT NOT NULL UNIQUE,
    [Equipamento] NVARCHAR(100) NOT NULL,
    [Avaria] NVARCHAR(500) NOT NULL,
    [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([Id]) ON DELETE CASCADE
);
GO

-- Criação da tabela de Detalhes de Software
CREATE TABLE [DetalhesSoftware] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketId] INT NOT NULL UNIQUE,
    [Aplicacao] NVARCHAR(100) NOT NULL,
    [Necessidade] NVARCHAR(500) NOT NULL,
    [CriadoEm] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([Id]) ON DELETE CASCADE
);
GO

-- Índices para melhor performance
CREATE INDEX [IX_Tickets_Estado] ON [Tickets]([Estado]);
CREATE INDEX [IX_Tickets_Prioridade] ON [Tickets]([Prioridade]);
CREATE INDEX [IX_Tickets_ColaboradorId] ON [Tickets]([ColaboradorId]);
CREATE INDEX [IX_Tickets_TecnicoId] ON [Tickets]([TecnicoId]);
CREATE INDEX [IX_Tickets_CriadoEm] ON [Tickets]([CriadoEm]);
GO

-- Dados iniciais - Utilizador administrador
INSERT INTO [Utilizadores] ([Username], [Password], [Nome], [Email], [Tipo]) 
VALUES ('admin', 'admin123', 'Administrador', 'admin@ticket2help.com', 1);
GO

-- Dados iniciais - Utilizador colaborador de teste
INSERT INTO [Utilizadores] ([Username], [Password], [Nome], [Email], [Tipo]) 
VALUES ('user.colaborador', 'user456', 'João Silva', 'joao.silva@empresa.com', 0);
GO";

        public override string DownScript => @"
-- Remove índices
DROP INDEX IF EXISTS [IX_Tickets_CriadoEm] ON [Tickets];
DROP INDEX IF EXISTS [IX_Tickets_TecnicoId] ON [Tickets];
DROP INDEX IF EXISTS [IX_Tickets_ColaboradorId] ON [Tickets];
DROP INDEX IF EXISTS [IX_Tickets_Prioridade] ON [Tickets];
DROP INDEX IF EXISTS [IX_Tickets_Estado] ON [Tickets];
GO

-- Remove tabelas (ordem inversa devido às foreign keys)
DROP TABLE IF EXISTS [DetalhesSoftware];
DROP TABLE IF EXISTS [DetalhesHardware];
DROP TABLE IF EXISTS [Tickets];
DROP TABLE IF EXISTS [Utilizadores];
GO";
    }
}