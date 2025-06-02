using System.Windows;
using System.Windows.Controls;
using MODELS;
using Ticket2Help.Views;

namespace Ticket2Help
{
    /// <summary>
    /// Janela principal do sistema Ticket2Help
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Utilizador _utilizadorAtual;

        /// <summary>
        /// Construtor da janela principal
        /// </summary>
        /// <param name="utilizador">Utilizador autenticado</param>
        public MainWindow(Utilizador utilizador)
        {
            InitializeComponent();
            _utilizadorAtual = utilizador;
            ConfigurarInterface();
        }

        /// <summary>
        /// Configura a interface baseada no tipo de utilizador
        /// </summary>
        private void ConfigurarInterface()
        {
            // Exibir informações do utilizador
            lblUtilizador.Text = $"Olá, {_utilizadorAtual.Nome} ({_utilizadorAtual.Tipo})";

            // Configurar visibilidade baseada no tipo de utilizador
            if (_utilizadorAtual.Tipo == TipoUtilizador.Tecnico)
            {
                // Técnicos podem ver dashboard
                btnDashboard.Visibility = Visibility.Visible;
                btnDashboardWelcome.Visibility = Visibility.Visible;
            }
            else
            {
                // Colaboradores não veem dashboard
                btnDashboard.Visibility = Visibility.Collapsed;
                btnDashboardWelcome.Visibility = Visibility.Collapsed;
            }

            // Atualizar status
            statusText.Text = $"Conectado como {_utilizadorAtual.Tipo}";
        }

        /// <summary>
        /// Navegação para a página de tickets
        /// </summary>
        private void BtnTickets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WelcomePanel.Visibility = Visibility.Collapsed;

                if (_utilizadorAtual.Tipo == TipoUtilizador.Tecnico)
                {
                    var ticketsPage = new Views.TicketsTecnicoPage(_utilizadorAtual);
                    MainFrame.Navigate(ticketsPage);
                }
                else
                {
                    var ticketsPage = new Views.TicketsColaboradorPage(_utilizadorAtual);
                    MainFrame.Navigate(ticketsPage);
                }
                

                statusText.Text = "Visualizando tickets";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar tickets: {ex.Message}", "Erro",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Navegação para criação de ticket
        /// </summary>
        private void BtnCriarTicket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WelcomePanel.Visibility = Visibility.Collapsed;

                // Por enquanto, mostrar uma mensagem até a página ser criada
                MessageBox.Show($"Formulário de criação de ticket será carregado aqui.\nUtilizador: {_utilizadorAtual.Nome}",
                               "Em Desenvolvimento", MessageBoxButton.OK, MessageBoxImage.Information);

                // TODO: Implementar quando a página estiver pronta
                /*
                var criarTicketPage = new Pages.CriarTicketPage(_utilizadorAtual);
                MainFrame.Navigate(criarTicketPage);
                */

                statusText.Text = "Criando novo ticket";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar página de criação: {ex.Message}", "Erro",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Navegação para dashboard (apenas técnicos)
        /// </summary>
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_utilizadorAtual.Tipo != TipoUtilizador.Tecnico)
            {
                MessageBox.Show("Apenas técnicos podem aceder ao dashboard.", "Acesso Negado",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                WelcomePanel.Visibility = Visibility.Collapsed;

                // Por enquanto, mostrar uma mensagem até a página ser criada
                MessageBox.Show("Dashboard estatístico será carregado aqui.",
                               "Em Desenvolvimento", MessageBoxButton.OK, MessageBoxImage.Information);

                // TODO: Implementar quando a página estiver pronta
                /*
                var dashboardPage = new Pages.DashboardPage(_utilizadorAtual);
                MainFrame.Navigate(dashboardPage);
                */

                statusText.Text = "Visualizando dashboard";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dashboard: {ex.Message}", "Erro",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Terminar sessão e voltar ao login
        /// </summary>
        private void BtnSair_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tem a certeza de que deseja sair?", "Confirmar Saída",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        /// <summary>
        /// Propriedade para acesso ao utilizador atual (para as páginas)
        /// </summary>
        public Utilizador UtilizadorAtual => _utilizadorAtual;
    }
}