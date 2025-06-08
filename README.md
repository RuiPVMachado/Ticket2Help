# Tick2Desk System

Sistema de gerenciamento de tickets para suporte técnico de TI, permitindo o registro e acompanhamento de problemas de hardware e software.

## 🚀 Funcionalidades

- Autenticação de usuários (técnicos e colaboradores)
- Criação e gerenciamento de tickets
  - Suporte para problemas de Hardware
  - Suporte para problemas de Software
- Dashboard para acompanhamento
- Interface WPF moderna e intuitiva
- Sistema de priorização de tickets
- Acompanhamento de status em tempo real

## 📋 Pré-requisitos

- .NET 8.0 ou superior
- SQL Server (para banco de dados)
- Visual Studio 2022 ou superior

## 🛠️ Tecnologias Utilizadas

- C# 12
- WPF (Windows Presentation Foundation)
- Entity Framework Core
- SQL Server
- xUnit (para testes)
- LiveCharts (para visualizações)

## 📦 Estrutura do Projeto

O projeto segue uma arquitetura em camadas:

- **BLL** - Business Logic Layer (Regras de Negócio)
- **DAL** - Data Access Layer (Acesso a Dados)
- **Models** - Classes de Domínio
- **Tests** - Testes Unitários e de Performance
- **Ticket2Help** - Interface do Usuário (WPF)

## ⚙️ Instalação

1. Clone o repositório
2. Abra a solução no Visual Studio
3. Configure a string de conexão no arquivo `DatabaseConfig.cs`
4. Execute as migrações do banco de dados
5. Compile e execute o projeto

## 📌 Documentação

O projeto utiliza Doxygen para documentação do código. Para gerar a documentação:

1. Instale o Doxygen
2. Execute o comando: `doxygen Doxyfile`
3. A documentação será gerada na pasta `docs`

## 🔍 Testes

O projeto inclui testes unitários e de performance utilizando xUnit: