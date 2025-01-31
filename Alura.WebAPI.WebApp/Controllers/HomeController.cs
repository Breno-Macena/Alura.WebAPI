﻿using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Alura.ListaLeitura.HttpClients;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Alura.ListaLeitura.WebApp.Controllers {
    [Authorize]
    public class HomeController : Controller {
        private readonly LivroApiClient _api;

        public HomeController(LivroApiClient api) {
            _api = api;
        }

        private async Task<IEnumerable<LivroApi>> ListaDoTipoAsync(TipoListaLeitura tipo) {
            var lista = await _api.GetListaLeituraAsync(tipo);
            return lista.Livros;
        }

        public async Task<IActionResult> Index() {
            var token = HttpContext.User.Claims.First(c => c.Type == "Token").Value;
            System.Console.WriteLine($"TOKEN: {token}");

            var model = new HomeViewModel {
                ParaLer = await ListaDoTipoAsync(TipoListaLeitura.ParaLer),
                Lendo = await ListaDoTipoAsync(TipoListaLeitura.Lendo),
                Lidos = await ListaDoTipoAsync(TipoListaLeitura.Lidos)
            };
            return View(model);
        }
    }
}