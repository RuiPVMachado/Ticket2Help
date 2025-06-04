using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using BLL.Interfaces;
using BLL.Services;
using DAL.Interfaces;
using DAL.Repositories;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MODELS;
using SkiaSharp;

namespace Ticket2Help.Views
{
    /// <summary>
    /// Página de dashboard com estatísticas dos tickets
    /// </summary>
    public partial class DashboardPage : Page
    {
        private readonly Utilizador _utilizadorAtual;
        private readonly ITicketService _ticketService;

        // Propriedades para os gráficos
        public ISeries[] EstadosSeries { get; set; } = Array.Empty<ISeries>();
        public ISeries[] PrioridadesSeries { get; set; } = Array.Empty<ISeries>();
        public ISeries[] TiposSeries { get; set; } = Array.Empty<ISeries>();
        public Axis[] TiposXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] TiposYAxes { get; set; } = Array.Empty<Axis>();

        // Classe para o resumo detalhado
        public class ResumoItem
        {
            public string Categoria { get; set; } = string.Empty;
            public int Quantidade { get; set; }
            public string Percentual { get; set; } = string.Empty;
        }

        public DashboardPage(Utilizador utilizador)
        {
            InitializeComponent();
            _utilizadorAtual = utilizador;

            // Injeção de dependências manual
            ITicketRepository ticketRepo = new TicketRepository();
            IDetalhesHardwareRepository hwRepo = new DetalhesHardwareRepository();
            IDetalhesSoftwareRepository swRepo = new DetalhesSoftwareRepository();
            _ticketService = new TicketService(ticketRepo, hwRepo, swRepo);

            DataContext = this;
            CarregarDados();
        }

        /// <summary>
        /// Carrega todos os dados do dashboard
        /// </summary>
        private void CarregarDados()
        {
            try
            {
                var tickets = _ticketService.ObterTodos();
                AtualizarEstatisticasRapidas(tickets);
                AtualizarGraficos(tickets);
                AtualizarResumoDetalhado(tickets);
                lblLastUpdate.Text = $"Última atualização: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados do dashboard: {ex.Message}",
                               "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Atualiza as estatísticas rápidas (cards superiores)
        /// </summary>
        private void AtualizarEstatisticasRapidas(List<Ticket> tickets)
        {
            lblTotalTickets.Text = tickets.Count.ToString();
            lblTicketsPorAtender.Text = tickets.Count(t => t.Estado == EstadoTicket.PorAtender).ToString();
            lblTicketsEmAtendimento.Text = tickets.Count(t => t.Estado == EstadoTicket.EmAtendimento).ToString();
            lblTicketsAtendidos.Text = tickets.Count(t => t.Estado == EstadoTicket.Atendido).ToString();
        }

        /// <summary>
        /// Atualiza todos os gráficos
        /// </summary>
        private void AtualizarGraficos(List<Ticket> tickets)
        {
            AtualizarGraficoEstados(tickets);
            AtualizarGraficoPrioridades(tickets);
            AtualizarGraficoTipos(tickets);
        }

        /// <summary>
        /// Gráfico de pizza - distribuição por estados
        /// </summary>
        private void AtualizarGraficoEstados(List<Ticket> tickets)
        {
            var estadosData = tickets
                .GroupBy(t => t.Estado)
                .Select(g => new { Estado = g.Key, Quantidade = g.Count() })
                .ToList();

            var series = new List<ISeries>();

            foreach (var item in estadosData)
            {
                var color = item.Estado switch
                {
                    EstadoTicket.PorAtender => SKColors.Orange,
                    EstadoTicket.EmAtendimento => SKColors.LimeGreen,
                    EstadoTicket.Atendido => SKColors.Green,
                    _ => SKColors.Gray
                };

                series.Add(new PieSeries<ObservableValue>
                {
                    Values = new[] { new ObservableValue(item.Quantidade) },
                    Name = GetEstadoDisplayName(item.Estado),
                    Fill = new SolidColorPaint(color)
                });
            }

            EstadosSeries = series.ToArray();
            EstadosChart.Series = EstadosSeries;
        }

        /// <summary>
        /// Gráfico de pizza - distribuição por prioridades
        /// </summary>
        private void AtualizarGraficoPrioridades(List<Ticket> tickets)
        {
            var prioridadesData = tickets
                .GroupBy(t => t.Prioridade)
                .Select(g => new { Prioridade = g.Key, Quantidade = g.Count() })
                .ToList();

            var series = new List<ISeries>();

            foreach (var item in prioridadesData)
            {
                var color = item.Prioridade switch
                {
                    Prioridade.Alta => SKColors.Red,
                    Prioridade.Media => SKColors.Orange,
                    Prioridade.Baixa => SKColors.LightBlue,
                    _ => SKColors.Gray
                };

                series.Add(new PieSeries<ObservableValue>
                {
                    Values = new[] { new ObservableValue(item.Quantidade) },
                    Name = GetPrioridadeDisplayName(item.Prioridade),
                    Fill = new SolidColorPaint(color)
                });
            }

            PrioridadesSeries = series.ToArray();
            PrioridadesChart.Series = PrioridadesSeries;
        }

        /// <summary>
        /// Gráfico de barras - Hardware vs Software
        /// </summary>
        private void AtualizarGraficoTipos(List<Ticket> tickets)
        {
            var tiposData = tickets
                .GroupBy(t => t.Tipo)
                .Select(g => new { Tipo = g.Key, Quantidade = g.Count() })
                .ToList();

            var values = new List<int>();
            var labels = new List<string>();

            foreach (var item in tiposData)
            {
                values.Add(item.Quantidade);
                labels.Add(GetTipoDisplayName(item.Tipo));
            }

            TiposSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    Name = "Tickets por Tipo"
                }
            };

            TiposXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0
                }
            };

            TiposYAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0
                }
            };

            TiposChart.Series = TiposSeries;
            TiposChart.XAxes = TiposXAxes;
            TiposChart.YAxes = TiposYAxes;
        }

        /// <summary>
        /// Atualiza a tabela de resumo detalhado
        /// </summary>
        private void AtualizarResumoDetalhado(List<Ticket> tickets)
        {
            var resumoItems = new List<ResumoItem>();
            int total = tickets.Count;

            if (total == 0)
            {
                dgResumo.ItemsSource = resumoItems;
                return;
            }

            // Estados
            var estadosGroups = tickets.GroupBy(t => t.Estado);
            foreach (var group in estadosGroups)
            {
                var percentual = (double)group.Count() / total * 100;
                resumoItems.Add(new ResumoItem
                {
                    Categoria = GetEstadoDisplayName(group.Key),
                    Quantidade = group.Count(),
                    Percentual = $"{percentual:F1}%"
                });
            }

            // Separador
            resumoItems.Add(new ResumoItem { Categoria = "--- Prioridades ---", Quantidade = 0, Percentual = "" });

            // Prioridades
            var prioridadesGroups = tickets.GroupBy(t => t.Prioridade);
            foreach (var group in prioridadesGroups)
            {
                var percentual = (double)group.Count() / total * 100;
                resumoItems.Add(new ResumoItem
                {
                    Categoria = GetPrioridadeDisplayName(group.Key),
                    Quantidade = group.Count(),
                    Percentual = $"{percentual:F1}%"
                });
            }

            // Separador
            resumoItems.Add(new ResumoItem { Categoria = "--- Tipos ---", Quantidade = 0, Percentual = "" });

            // Tipos
            var tiposGroups = tickets.GroupBy(t => t.Tipo);
            foreach (var group in tiposGroups)
            {
                var percentual = (double)group.Count() / total * 100;
                resumoItems.Add(new ResumoItem
                {
                    Categoria = GetTipoDisplayName(group.Key),
                    Quantidade = group.Count(),
                    Percentual = $"{percentual:F1}%"
                });
            }

            dgResumo.ItemsSource = resumoItems;
        }

        /// <summary>
        /// Manipula o clique do botão atualizar
        /// </summary>
        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            btnAtualizar.IsEnabled = false;
            btnAtualizar.Content = "🔄 Atualizando...";

            try
            {
                CarregarDados();
            }
            finally
            {
                btnAtualizar.IsEnabled = true;
                btnAtualizar.Content = "🔄 Atualizar Dados";
            }
        }

        #region Métodos Auxiliares para Display Names

        private string GetEstadoDisplayName(EstadoTicket estado)
        {
            return estado switch
            {
                EstadoTicket.PorAtender => "Por Atender",
                EstadoTicket.EmAtendimento => "Em Atendimento",
                EstadoTicket.Atendido => "Atendido",
                _ => estado.ToString()
            };
        }

        private string GetPrioridadeDisplayName(Prioridade prioridade)
        {
            return prioridade switch
            {
                Prioridade.Alta => "Alta",
                Prioridade.Media => "Média",
                Prioridade.Baixa => "Baixa",
                _ => prioridade.ToString()
            };
        }

        private string GetTipoDisplayName(TipoTicket tipo)
        {
            return tipo switch
            {
                TipoTicket.Hardware => "Hardware",
                TipoTicket.Software => "Software",
                _ => tipo.ToString()
            };
        }

        #endregion
    }
}