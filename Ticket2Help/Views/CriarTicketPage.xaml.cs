using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BLL.Interfaces;
using BLL.Services;
using DAL.Interfaces;
using DAL.Repositories;
using MODELS;

namespace Ticket2Help.Views
{
    /// <summary>
    /// Página para criação de novos tickets
    /// </summary>
    public partial class CriarTicketPage : Page
    {
        private readonly Utilizador _utilizadorAtual;
        private ITicketService _ticketService;
        private IUtilizadorService _utilizadorService;
        private List<Utilizador> _colaboradores;

        /// <summary>
        /// Construtor da página de criação de tickets
        /// </summary>
        /// <param name="utilizador">Utilizador atual logado</param>
        public CriarTicketPage(Utilizador utilizador)
        {
            InitializeComponent();
            _utilizadorAtual = utilizador;

            // Configurar serviços (injeção de dependências manual)
            ConfigurarServicos();

            // Configurar interface baseada no tipo de utilizador
            ConfigurarInterface();
        }

        /// <summary>
        /// Configura os serviços necessários para a página
        /// </summary>
        private void ConfigurarServicos()
        {
            // Repositórios
            ITicketRepository ticketRepo = new TicketRepository();
            IDetalhesHardwareRepository hwRepo = new DetalhesHardwareRepository();
            IDetalhesSoftwareRepository swRepo = new DetalhesSoftwareRepository();
            IUtilizadorRepository utilizadorRepo = new UtilizadorRepository();

            // Serviços
            _ticketService = new TicketService(ticketRepo, hwRepo, swRepo);
            _utilizadorService = new UtilizadorService(utilizadorRepo);
        }

        /// <summary>
        /// Configura a interface baseada no tipo de utilizador
        /// </summary>
        private void ConfigurarInterface()
        {
            if (_utilizadorAtual.Tipo == TipoUtilizador.Tecnico)
            {
                // Técnico pode selecionar colaborador
                pnlColaborador.Visibility = Visibility.Visible;
                CarregarColaboradores();
            }
            else
            {
                // Colaborador cria ticket para si mesmo
                pnlColaborador.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Carrega a lista de colaboradores para seleção (apenas para técnicos)
        /// </summary>
        private void CarregarColaboradores()
        {
            try
            {
                var todosUtilizadores = _utilizadorService.ObterTodos();
                _colaboradores = todosUtilizadores.Where(u => u.Tipo == TipoUtilizador.Colaborador).ToList();

                cbColaborador.ItemsSource = _colaboradores;
                cbColaborador.DisplayMemberPath = "Nome";
                cbColaborador.SelectedValuePath = "Id";

                if (_colaboradores.Count == 0)
                {
                    MostrarErro("Não foram encontrados colaboradores no sistema.");
                    btnCriar.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao carregar colaboradores: {ex.Message}");
                btnCriar.IsEnabled = false;
            }
        }

        /// <summary>
        /// Manipula a mudança de seleção do tipo de ticket
        /// </summary>
        private void CbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTipo.SelectedItem is ComboBoxItem selectedItem)
            {
                string tipoSelecionado = selectedItem.Tag.ToString();

                // Mostrar/esconder painéis baseado no tipo
                if (tipoSelecionado == "Hardware")
                {
                    pnlHardware.Visibility = Visibility.Visible;
                    pnlSoftware.Visibility = Visibility.Collapsed;
                }
                else if (tipoSelecionado == "Software")
                {
                    pnlHardware.Visibility = Visibility.Collapsed;
                    pnlSoftware.Visibility = Visibility.Visible;
                }
                else
                {
                    pnlHardware.Visibility = Visibility.Collapsed;
                    pnlSoftware.Visibility = Visibility.Collapsed;
                }
            }

            // Esconder erro ao mudar tipo
            EsconderErro();
        }

        /// <summary>
        /// Manipula o clique do botão criar ticket
        /// </summary>
        private async void BtnCriar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar formulário
                if (!ValidarFormulario())
                    return;

                // Desabilitar botão durante criação
                btnCriar.IsEnabled = false;
                btnCriar.Content = "Criando...";

                // Obter dados do formulário
                var dadosTicket = ObterDadosFormulario();

                int ticketId;
                string tipoSelecionado = ((ComboBoxItem)cbTipo.SelectedItem).Tag.ToString();

                // Criar ticket usando os métodos específicos do factory
                if (tipoSelecionado == "Hardware")
                {
                    ticketId = _ticketService.CriarTicketHardware(
                        dadosTicket.Titulo,
                        dadosTicket.Descricao,
                        dadosTicket.Prioridade,
                        dadosTicket.ColaboradorId,
                        dadosTicket.Equipamento,
                        dadosTicket.Avaria,
                        _utilizadorAtual.Id,
                        _utilizadorAtual.Tipo == TipoUtilizador.Tecnico
                    );
                }
                else
                {
                    ticketId = _ticketService.CriarTicketSoftware(
                        dadosTicket.Titulo,
                        dadosTicket.Descricao,
                        dadosTicket.Prioridade,
                        dadosTicket.ColaboradorId,
                        dadosTicket.Aplicacao,
                        dadosTicket.Necessidade,
                        _utilizadorAtual.Id,
                        _utilizadorAtual.Tipo == TipoUtilizador.Tecnico
                    );
                }

                // Sucesso - mostrar mensagem e voltar
                string mensagemSucesso = $"Ticket #{ticketId} criado com sucesso!\n\n" +
                                       $"Título: {dadosTicket.Titulo}\n" +
                                       $"Tipo: {tipoSelecionado}\n" +
                                       $"Prioridade: {dadosTicket.Prioridade}";

                if (_utilizadorAtual.Tipo == TipoUtilizador.Tecnico)
                {
                    var colaboradorSelecionado = _colaboradores.FirstOrDefault(c => c.Id == dadosTicket.ColaboradorId);
                    mensagemSucesso += $"\nColaborador: {colaboradorSelecionado?.Nome}";
                }

                MessageBox.Show(mensagemSucesso, "Ticket Criado", MessageBoxButton.OK, MessageBoxImage.Information);

                // Voltar para a página anterior ou principal
                VoltarPaginaAnterior();
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao criar ticket: {ex.Message}");
            }
            finally
            {
                // Reabilitar botão
                btnCriar.IsEnabled = true;
                btnCriar.Content = "Criar Ticket";
            }
        }

        /// <summary>
        /// Manipula o clique do botão cancelar
        /// </summary>
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tem a certeza de que deseja cancelar a criação do ticket?\nTodas as informações serão perdidas.",
                                        "Confirmar Cancelamento", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                VoltarPaginaAnterior();
            }
        }

        /// <summary>
        /// Valida os campos do formulário
        /// </summary>
        private bool ValidarFormulario()
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MostrarErro("O título do ticket é obrigatório.");
                txtTitulo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescricao.Text))
            {
                MostrarErro("A descrição do ticket é obrigatória.");
                txtDescricao.Focus();
                return false;
            }

            if (cbPrioridade.SelectedItem == null)
            {
                MostrarErro("Selecione a prioridade do ticket.");
                cbPrioridade.Focus();
                return false;
            }

            if (cbTipo.SelectedItem == null)
            {
                MostrarErro("Selecione o tipo do ticket.");
                cbTipo.Focus();
                return false;
            }

            // Validação específica para técnicos
            if (_utilizadorAtual.Tipo == TipoUtilizador.Tecnico)
            {
                if (cbColaborador.SelectedItem == null)
                {
                    MostrarErro("Selecione o colaborador para quem está a criar o ticket.");
                    cbColaborador.Focus();
                    return false;
                }
            }

            // Validações específicas por tipo
            string tipoSelecionado = ((ComboBoxItem)cbTipo.SelectedItem).Tag.ToString();

            if (tipoSelecionado == "Hardware")
            {
                if (string.IsNullOrWhiteSpace(txtEquipamento.Text))
                {
                    MostrarErro("O equipamento é obrigatório para tickets de hardware.");
                    txtEquipamento.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtAvaria.Text))
                {
                    MostrarErro("A descrição da avaria é obrigatória para tickets de hardware.");
                    txtAvaria.Focus();
                    return false;
                }
            }
            else if (tipoSelecionado == "Software")
            {
                if (string.IsNullOrWhiteSpace(txtAplicacao.Text))
                {
                    MostrarErro("A aplicação/sistema é obrigatória para tickets de software.");
                    txtAplicacao.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtNecessidade.Text))
                {
                    MostrarErro("A descrição da necessidade é obrigatória para tickets de software.");
                    txtNecessidade.Focus();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Classe auxiliar para transportar dados do formulário
        /// </summary>
        private class DadosFormulario
        {
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public Prioridade Prioridade { get; set; }
            public int ColaboradorId { get; set; }
            public string Equipamento { get; set; }
            public string Avaria { get; set; }
            public string Aplicacao { get; set; }
            public string Necessidade { get; set; }
        }

        /// <summary>
        /// Obtém os dados do formulário
        /// </summary>
        private DadosFormulario ObterDadosFormulario()
        {
            string prioridadeStr = ((ComboBoxItem)cbPrioridade.SelectedItem).Tag.ToString();

            // Determinar ID do colaborador
            int colaboradorId;
            if (_utilizadorAtual.Tipo == TipoUtilizador.Tecnico)
            {
                colaboradorId = (int)cbColaborador.SelectedValue;
            }
            else
            {
                colaboradorId = _utilizadorAtual.Id;
            }

            return new DadosFormulario
            {
                Titulo = txtTitulo.Text.Trim(),
                Descricao = txtDescricao.Text.Trim(),
                Prioridade = Enum.Parse<Prioridade>(prioridadeStr),
                ColaboradorId = colaboradorId,
                Equipamento = txtEquipamento?.Text?.Trim(),
                Avaria = txtAvaria?.Text?.Trim(),
                Aplicacao = txtAplicacao?.Text?.Trim(),
                Necessidade = txtNecessidade?.Text?.Trim()
            };
        }

        /// <summary>
        /// Mostra uma mensagem de erro na interface
        /// </summary>
        private void MostrarErro(string mensagem)
        {
            lblError.Text = mensagem;
            pnlError.Visibility = Visibility.Visible;

            // Scroll até o erro
            if (pnlError.Parent is StackPanel stackPanel && stackPanel.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToTop();
            }
        }

        /// <summary>
        /// Esconde a mensagem de erro
        /// </summary>
        private void EsconderErro()
        {
            pnlError.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Volta para a página anterior
        /// </summary>
        private void VoltarPaginaAnterior()
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Se não há página anterior, navegar para a página de tickets
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.WelcomePanel.Visibility = Visibility.Visible;
                    mainWindow.MainFrame.Content = null;
                }
            }
        }

        /// <summary>
        /// Event handlers para esconder erro quando o utilizador começa a digitar
        /// </summary>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            EsconderErro();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EsconderErro();
        }

        // Eventos para esconder erro automaticamente
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Adicionar event handlers para esconder erro
            txtTitulo.TextChanged += OnTextChanged;
            txtDescricao.TextChanged += OnTextChanged;
            txtEquipamento.TextChanged += OnTextChanged;
            txtAvaria.TextChanged += OnTextChanged;
            txtAplicacao.TextChanged += OnTextChanged;
            txtNecessidade.TextChanged += OnTextChanged;
            cbPrioridade.SelectionChanged += OnSelectionChanged;
            cbColaborador.SelectionChanged += OnSelectionChanged;
        }
    }
}