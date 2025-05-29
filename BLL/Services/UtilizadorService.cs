using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLL.Interfaces;
using DAL.Interfaces;
using MODELS;
using System.Collections.Generic;

namespace BLL.Services
{
    /// <summary>
    /// Serviço responsável pela lógica de utilizadores.
    /// </summary>
    public class UtilizadorService : IUtilizadorService
    {
        private readonly IUtilizadorRepository _repo;

        /// <summary>
        /// Construtor com injeção do repositório de utilizadores.
        /// </summary>
        /// <param name="repo">Implementação de IUtilizadorRepository.</param>
        public UtilizadorService(IUtilizadorRepository repo)
        {
            _repo = repo;
        }

        /// <inheritdoc />
        public Utilizador ValidarLogin(string username, string password)
        {
            var user = _repo.ObterPorUsername(username);
            if (user != null && user.PasswordHash == password)
                return user;
            return null;
        }

        /// <inheritdoc />
        public List<Utilizador> ObterTodos()
        {
            return _repo.ObterTodos();
        }
    }
}
