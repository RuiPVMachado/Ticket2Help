using System.Windows;
using DAL;
using Ticket2Help.Views;

namespace Ticket2Help
{
    /// <summary>
    /// Lógica de interação para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Verificar conexão com a base de dados na inicialização
            if (!DatabaseConfig.TestConnection())
            {
                MessageBox.Show(
                    "Não foi possível conectar à base de dados.\n\n" +
                    "Verifique se:\n" +
                    "• SQL Server está em execução\n" +
                    "• A base de dados 'Tick2DeskDB' existe\n" +
                    "• A string de conexão está correta",
                    "Erro de Conexão",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Encerrar a aplicação se não conseguir conectar à BD
                Shutdown();
                return;
            }

            // Se a conexão estiver OK, mostrar a janela de login
            try
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao inicializar a aplicação: {ex.Message}",
                    "Erro de Inicialização",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Aqui pode adicionar lógica de limpeza se necessário
            base.OnExit(e);
        }
    }
}