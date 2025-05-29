using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MODELS;
using System.Collections.Generic;

namespace BLL.Interfaces
{
    /// <summary>
    /// Define as operações relacionadas com utilizadores.
    /// </summary>
    public interface IUtilizadorService
    {
        /// <summary>
        /// Valida um login com base no username e password.
        /// </summary>
        /// <param name="username">Username do utilizador.</param>
        /// <param name="password">Password introduzida.</param>
        /// <returns>Utilizador válido ou null.</returns>
        Utilizador ValidarLogin(string username, string password);

        /// <summary>
        /// Lista todos os utilizadores do sistema.
        /// </summary>
        /// <returns>Lista de utilizadores.</returns>
        List<Utilizador> ObterTodos();
    }
}

