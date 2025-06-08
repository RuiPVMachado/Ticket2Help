using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Página de gestão de tickets para técnicos
    /// </summary>
    public partial class TicketsTecnicoPage : Page, INotifyPropertyChanged
    {
        #region Campos Privados
        private readonly Utilizador _utilizadorAtual;
        private readonly ITicketService _ticketService;
        private readonly IUtilizadorService _utilizadorService;
        private List<TicketViewModel> _todosTickets;
        private List<TicketViewModel> _ticketsFiltrados;
        private TicketViewModel _ticketSelecionado;
        #endregion

        #region Propriedades
        public List<TicketViewModel> TicketsFiltrados
        {
            get => _ticketsFiltrados;
            set
            {
                _ticketsFiltrados = value;
                OnPropertyChanged();
            }
        }

        public TicketViewModel TicketSelecionado
        {
            get => _ticketSelecionado;
            set
            {
                _ticketSelecionado = value;
                OnPropertyChanged();
                AtualizarBotoesAcao();
            }
        }
        #endregion

        #region Construtor
        public TicketsTecnicoPage(Utilizador utilizador)
        {
            InitializeComponent();

            _utilizadorAtual = utilizador ?? throw new ArgumentNullException(nameof(utilizador));

            // Injeção de dependências manual
            ITicketRepository ticketRepo = new TicketRepository();
            IDetalhesHardwareRepository hwRepo = new DetalhesHardwareRepository();
            IDetalhesSoftwareRepository swRepo = new DetalhesSoftwareRepository();
            IUtilizadorRepository utilizadorRepo = new UtilizadorRepository();

            _ticketService = new TicketService(ticketRepo, hwRepo, swRepo);
            _utilizadorService = new UtilizadorService(utilizadorRepo);

            DataContext = this;

            // Carregar dados iniciais
            CarregarTickets();
        }
        #endregion

        #region Métodos de Carregamento
        private async void CarregarTickets()
        {
            try
            {
                lblStatus.Text = "Carregando tickets...";

                // Carregar todos os tickets
                var tickets = _ticketService.ObterTodos();
                var utilizadores = _utilizadorService.ObterTodos();

                // Criar ViewModels com informações completas
                _todosTickets = tickets.Select(t => new TicketViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descricao = t.Descricao,
                    Prioridade = t.Prioridade,
                    Tipo = t.Tipo,
                    Estado = t.Estado,
                    SubEstado = t.SubEstado,
                    DataCriacao = t.DataCriacao,
                    DataAtendimento = t.DataAtendimento,
                    IdColaborador = t.IdColaborador,
                    IdTecnico = t.IdTecnico,
                    NomeColaborador = utilizadores.FirstOrDefault(u => u.Id == t.IdColaborador)?.Nome ?? "N/A",
                    NomeTecnico = t.IdTecnico.HasValue ?
                        utilizadores.FirstOrDefault(u => u.Id == t.IdTecnico.Value)?.Nome ?? "N/A" :
                        "Não atribuído"
                }).ToList();

                // Aplicar filtros iniciais
                AplicarFiltros();
                AtualizarEstatisticas();

                lblStatus.Text = "Tickets carregados com sucesso";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar tickets: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Erro ao carregar tickets";
            }
        }

        private void AtualizarEstatisticas()
        {
            if (_todosTickets == null) return;

            var urgentes = _todosTickets.Count(t => t.Prioridade == Prioridade.Alta && t.Estado == EstadoTicket.PorAtender);
            var porAtender = _todosTickets.Count(t => t.Estado == EstadoTicket.PorAtender);
            var meusTickets = _todosTickets.Count(t => t.IdTecnico == _utilizadorAtual.Id);

            lblUrgentes.Text = urgentes.ToString();
            lblPorAtender.Text = porAtender.ToString();
            lblMeusTickets.Text = meusTickets.ToString();
        }
        #endregion

        #region Métodos de Filtro
        private void AplicarFiltros()
        {
            if (_todosTickets == null)
            {
                TicketsFiltrados = new List<TicketViewModel>();
                return;
            }

            var tickets = _todosTickets.AsEnumerable();

            // Filtro por estado
            var estadoSelecionado = (cmbFiltroEstado.SelectedItem as ComboBoxItem)?.Content?.ToString();
            switch (estadoSelecionado)
            {
                case "Por Atender":
                    tickets = tickets.Where(t => t.Estado == EstadoTicket.PorAtender);
                    break;
                case "Em Atendimento":
                    tickets = tickets.Where(t => t.Estado == EstadoTicket.EmAtendimento);
                    break;
                case "Atendido":
                    tickets = tickets.Where(t => t.Estado == EstadoTicket.Atendido);
                    break;
                    // "Todos" não aplica filtro
            }

            // Filtro por prioridade
            var prioridadeSelecionada = (cmbFiltroPrioridade.SelectedItem as ComboBoxItem)?.Content?.ToString();
            switch (prioridadeSelecionada)
            {
                case "Alta":
                    tickets = tickets.Where(t => t.Prioridade == Prioridade.Alta);
                    break;
                case "Media":
                    tickets = tickets.Where(t => t.Prioridade == Prioridade.Media);
                    break;
                case "Baixa":
                    tickets = tickets.Where(t => t.Prioridade == Prioridade.Baixa);
                    break;
                    // "Todas" não aplica filtro
            }

            // Ordenar: por atender primeiro, depois por prioridade crescente e data
            tickets = tickets.OrderBy(t => t.Estado) 
                             .ThenBy(t => t.Prioridade)
                             .ThenBy(t => t.DataCriacao);

            TicketsFiltrados = tickets.ToList();
            dgTickets.ItemsSource = TicketsFiltrados;

            AtualizarTotalTickets();
        }

        private void AtualizarTotalTickets()
        {
            var total = TicketsFiltrados?.Count ?? 0;
            lblTotalTickets.Text = $"Total: {total} tickets";
        }
        #endregion

        #region Event Handlers
        private void CmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void CmbFiltroPrioridade_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            CarregarTickets();
        }

        private void DgTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TicketSelecionado = dgTickets.SelectedItem as TicketViewModel;
        }

        private void BtnAtender_Click(object sender, RoutedEventArgs e)
        {
            if (TicketSelecionado == null) return;

            try
            {
                EstadoTicket novoEstado;
                string acao;
                string acaoSucesso;

                if (TicketSelecionado.Estado == EstadoTicket.PorAtender)
                {
                    novoEstado = EstadoTicket.EmAtendimento;
                    acao = "assumir";
                    acaoSucesso = "assumido";
                }
                else if (TicketSelecionado.Estado == EstadoTicket.EmAtendimento &&
                         TicketSelecionado.IdTecnico == _utilizadorAtual.Id)
                {
                    // Finalizar ticket (abre janela com detalhes)
                    var resultado = MessageBox.Show(
                        "Deseja finalizar este ticket como resolvido?",
                        "Finalizar Ticket",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.No) return;

                    novoEstado = EstadoTicket.Atendido;
                    var subEstado = resultado == MessageBoxResult.Yes ?
                        SubEstadoAtendimento.Resolvido : SubEstadoAtendimento.NaoResolvido;

                    bool sucesso = _ticketService.AtualizarEstado(
                        TicketSelecionado.Id,
                        novoEstado,
                        subEstado,
                        _utilizadorAtual.Id);

                    if (sucesso)
                    {
                        MessageBox.Show("Ticket finalizado com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CarregarTickets();
                    }
                    else
                    {
                        MessageBox.Show("Erro ao finalizar ticket.", "Erro",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                else
                {
                    MessageBox.Show("Não é possível atender este ticket.", "Ação Inválida",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Confirmar ação
                var confirmacao = MessageBox.Show(
                    $"Deseja {acao} este ticket?",
                    "Confirmar Ação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmacao == MessageBoxResult.Yes)
                {
                    bool sucesso = _ticketService.AtualizarEstado(
                        TicketSelecionado.Id,
                        novoEstado,
                        null,
                        _utilizadorAtual.Id);

                    if (sucesso)
                    {
                        MessageBox.Show($"Ticket {acaoSucesso} com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CarregarTickets();
                    }
                    else
                    {
                        MessageBox.Show($"Erro ao {acao} ticket.", "Erro",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao processar ticket: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDetalhes_Click(object sender, RoutedEventArgs e)
        {
            if (TicketSelecionado == null) return;

            try
            {
                string detalhes = $"ID: {TicketSelecionado.Id}\n" +
                                 $"Título: {TicketSelecionado.Titulo}\n" +
                                 $"Descrição: {TicketSelecionado.Descricao}\n" +
                                 $"Tipo: {TicketSelecionado.Tipo}\n" +
                                 $"Prioridade: {TicketSelecionado.Prioridade}\n" +
                                 $"Estado: {TicketSelecionado.Estado}\n" +
                                 $"Colaborador: {TicketSelecionado.NomeColaborador}\n" +
                                 $"Técnico: {TicketSelecionado.NomeTecnico}\n" +
                                 $"Criado em: {TicketSelecionado.DataCriacao:dd/MM/yyyy HH:mm}";

                if (TicketSelecionado.DataAtendimento.HasValue)
                {
                    detalhes += $"\nAtendido em: {TicketSelecionado.DataAtendimento:dd/MM/yyyy HH:mm}";
                }

                MessageBox.Show(detalhes, "Detalhes do Ticket",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar detalhes: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Métodos Auxiliares
        private void AtualizarBotoesAcao()
        {
            if (TicketSelecionado == null)
            {
                btnAtender.IsEnabled = false;
                btnDetalhes.IsEnabled = false;
                btnEliminar.IsEnabled = false;
                btnAtender.Content = "✋ Atender Ticket";
                return;
            }

            btnDetalhes.IsEnabled = true;
            btnEliminar.IsEnabled = TicketSelecionado.Estado != EstadoTicket.Atendido;

            // Lógica para botão de atender
            switch (TicketSelecionado.Estado)
            {
                case EstadoTicket.PorAtender:
                    btnAtender.IsEnabled = true;
                    btnAtender.Content = "✋ Assumir Ticket";
                    break;

                case EstadoTicket.EmAtendimento:
                    // Só pode finalizar se for o técnico responsável
                    if (TicketSelecionado.IdTecnico == _utilizadorAtual.Id)
                    {
                        btnAtender.IsEnabled = true;
                        btnAtender.Content = "✅ Finalizar Ticket";
                    }
                    else
                    {
                        btnAtender.IsEnabled = false;
                        btnAtender.Content = "🔒 Ticket de Outro Técnico";
                    }
                    break;

                case EstadoTicket.Atendido:
                    btnAtender.IsEnabled = false;
                    btnAtender.Content = "✅ Ticket Finalizado";
                    break;

                default:
                    btnAtender.IsEnabled = false;
                    btnAtender.Content = "✋ Atender Ticket";
                    break;
            }
        }
        #endregion
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (TicketSelecionado == null) return;

            try
            {
                var confirmacao = MessageBox.Show(
                    "Tem certeza que deseja eliminar este ticket?\nEsta ação não pode ser desfeita.",
                    "Confirmar Eliminação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmacao == MessageBoxResult.Yes)
                {
                    bool sucesso = _ticketService.EliminarTicket(TicketSelecionado.Id);

                    if (sucesso)
                    {
                        MessageBox.Show("Ticket eliminado com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CarregarTickets();
                    }
                    else
                    {
                        MessageBox.Show("Não foi possível eliminar o ticket.", "Erro",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao eliminar ticket: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    #region ViewModel
    /// <summary>
    /// ViewModel para exibição de tickets na interface
    /// </summary>
    public class TicketViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public Prioridade Prioridade { get; set; }
        public TipoTicket Tipo { get; set; }
        public EstadoTicket Estado { get; set; }
        public SubEstadoAtendimento? SubEstado { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtendimento { get; set; }
        public int IdColaborador { get; set; }
        public int? IdTecnico { get; set; }
        public string NomeColaborador { get; set; }
        public string NomeTecnico { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    #endregion
}