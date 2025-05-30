﻿using MODELS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IUtilizadorRepository
    {
        Utilizador ObterPorUsername(string username);
        List<Utilizador> ObterTodos();
    }
}

