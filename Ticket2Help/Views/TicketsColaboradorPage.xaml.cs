using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MODELS;
using BLL.Services;
using BLL.Interfaces;
using DAL.Repositories;

namespace Ticket2Help.Views
{
    /// <summary>
    /// Página de gestão de tickets para colaboradores
    /// </summary>
    public partial class TicketsColaboradorPage : Page
    {
        private readonly Utilizador _utilizadorAtual;
        private readonly ITicketService _ticketService;
        private readonly IUtilizadorService _utilizadorService;
        private List<Ticket> _todosTickets;
        private List<Ticket> _ticketsFiltrados;

        public TicketsColaboradorPage(Utilizador utilizador)
        {
            InitializeComponent();
            _utilizadorAtual = utilizador;

            // Inicializar serviços (assumindo injeção de dependência ou factory)
            var ticketRepo = new TicketRepository();
            var hwRepo = new DetalhesHardwareRepository();
            var swRepo = new DetalhesSoftwareRepository();
            var userRepo = new UtilizadorRepository();

            _ticketService = new TicketService(ticketRepo, hwRepo, swRepo);
            _utilizadorService = new UtilizadorService(userRepo);

            CarregarTickets();
            AtualizarEstatisticas();
        }

        /// <summary>
        /// Carrega todos os tickets do colaborador atual
        /// </summary>
        private void CarregarTickets()
        {
            try
            {
                lblStatus.Text = "Carregando tickets...";

                // Carregar apenas os tickets do colaborador logado
                _todosTickets = _ticketService.ObterTodos()
                    .Where(t => t.IdColaborador == _utilizadorAtual.Id)
                    .ToList();

                // Enriquecer com nomes dos utilizadores
                EnriquecerTicketsComNomes();

                // Aplicar filtros atuais
                AplicarFiltros();

                lblStatus.Text = "Tickets carregados com sucesso";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar tickets: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Erro ao carregar tickets";
            }
        }

        /// <summary>
        /// Enriquece os tickets com nomes dos colaboradores e técnicos
        /// </summary>
        private void EnriquecerTicketsComNomes()
        {
            var utilizadores = _utilizadorService.ObterTodos();

            foreach (var ticket in _todosTickets)
            {
                var colaborador = utilizadores.FirstOrDefault(u => u.Id == ticket.IdColaborador);
                ticket.NomeColaborador = colaborador?.Nome ?? "Desconhecido";

                if (ticket.IdTecnico.HasValue)
                {
                    var tecnico = utilizadores.FirstOrDefault(u => u.Id == ticket.IdTecnico.Value);
                    ticket.NomeTecnico = tecnico?.Nome ?? "Não atribuído";
                }
                else
                {
                    ticket.NomeTecnico = "Não atribuído";
                }
            }
        }

        /// <summary>
        /// Aplica os filtros selecionados
        /// </summary>
        private void AplicarFiltros()
        {
            if (_todosTickets == null)
            {
                _ticketsFiltrados = new List<Ticket>();
                if (dgTickets != null)
                {
                    dgTickets.ItemsSource = _ticketsFiltrados;
                }
                AtualizarTotalTickets();
                return;
            }

            var tickets = _todosTickets.AsEnumerable();

            // Filtro por estado
            var estadoSelecionado = (cmbFiltroEstado?.SelectedItem as ComboBoxItem)?.Content?.ToString();
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
            var prioridadeSelecionada = (cmbFiltroPrioridade?.SelectedItem as ComboBoxItem)?.Content?.ToString();
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

            _ticketsFiltrados = tickets.ToList();

            // Atualizar grid
            if (dgTickets != null)
            {
                dgTickets.ItemsSource = _ticketsFiltrados;
            }
            AtualizarTotalTickets();
        }

        /// <summary>
        /// Atualiza o total de tickets exibidos
        /// </summary>
        private void AtualizarTotalTickets()
        {
            var total = _ticketsFiltrados?.Count ?? 0;
            if (lblTotalTickets != null)
            {
                lblTotalTickets.Text = $"Total: {total} tickets";
            }
        }

        /// <summary>
        /// Atualiza as estatísticas rápidas
        /// </summary>
        private void AtualizarEstatisticas()
        {
            if (_todosTickets == null) return;

            // Tickets urgentes (prioridade alta)
            var urgentes = _todosTickets.Count(t => t.Prioridade == Prioridade.Alta &&
                                                   t.Estado != EstadoTicket.Atendido);
            lblUrgentes.Text = urgentes.ToString();

            // Tickets por atender
            var porAtender = _todosTickets.Count(t => t.Estado == EstadoTicket.PorAtender);
            lblPorAtender.Text = porAtender.ToString();

            // Tickets em atendimento
            var emAtendimento = _todosTickets.Count(t => t.Estado == EstadoTicket.EmAtendimento);
            lblEmAtendimento.Text = emAtendimento.ToString();
        }

        #region Event Handlers

        /// <summary>
        /// Atualizar lista de tickets
        /// </summary>
        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            CarregarTickets();
            AtualizarEstatisticas();
        }

        /// <summary>
        /// Ver detalhes do ticket selecionado
        /// </summary>
        private void BtnDetalhes_Click(object sender, RoutedEventArgs e)
        {
            if (dgTickets.SelectedItem is Ticket ticketSelecionado)
            {
                try
                {
                    // TODO: Implementar janela de detalhes
                    MessageBox.Show($"Detalhes do Ticket #{ticketSelecionado.Id}\n\n" +
                                   $"Título: {ticketSelecionado.Titulo}\n" +
                                   $"Descrição: {ticketSelecionado.Descricao}\n" +
                                   $"Prioridade: {ticketSelecionado.Prioridade}\n" +
                                   $"Estado: {ticketSelecionado.Estado}\n" +
                                   $"Técnico: {ticketSelecionado.NomeTecnico}\n" +
                                   $"Criado em: {ticketSelecionado.DataCriacao:dd/MM/yyyy HH:mm}",
                                   "Detalhes do Ticket", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar detalhes: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecione um ticket.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Filtro por estado alterado
        /// </summary>
        private void CmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        /// <summary>
        /// Filtro por prioridade alterado
        /// </summary>
        private void CmbFiltroPrioridade_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        /// <summary>
        /// Seleção no grid alterada
        /// </summary>
        private void DgTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool temSelecao = dgTickets.SelectedItem != null;
            btnDetalhes.IsEnabled = temSelecao;
            btnEliminar.IsEnabled = temSelecao &&
                ((Ticket)dgTickets.SelectedItem)?.Estado != EstadoTicket.Atendido;
        }
        /// <summary>
        /// Eliminar ticket selecionado
        /// </summary>
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var ticketSelecionado = dgTickets.SelectedItem as Ticket;
            if (ticketSelecionado == null) return;

            try
            {
                // Confirmação antes de eliminar
                var confirmacao = MessageBox.Show(
                    "Tem certeza que deseja eliminar este ticket?\nEsta ação não pode ser desfeita.",
                    "Confirmar Eliminação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmacao == MessageBoxResult.Yes)
                {
                    // Verificar se o ticket pode ser eliminado
                    if (ticketSelecionado.Estado == EstadoTicket.Atendido)
                    {
                        MessageBox.Show(
                            "Não é possível eliminar tickets já atendidos.",
                            "Operação não permitida",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    bool sucesso = _ticketService.EliminarTicket(ticketSelecionado.Id);

                    if (sucesso)
                    {
                        MessageBox.Show(
                            "Ticket eliminado com sucesso!",
                            "Sucesso",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // Recarregar a lista de tickets e atualizar estatísticas
                        CarregarTickets();
                        AtualizarEstatisticas();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Não foi possível eliminar o ticket.",
                            "Erro",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao eliminar ticket: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        #endregion
    }
}