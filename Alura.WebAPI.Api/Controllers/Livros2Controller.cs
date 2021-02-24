using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Alura.WebAPI.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alura.WebAPI.Api.Controllers {
    [Authorize]
    [ApiController]
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("api/v{version:apiVersion}/livros")]
    public class Livros2Controller : ControllerBase {
        private readonly IRepository<Livro> _repo;

        public Livros2Controller(IRepository<Livro> repo) {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult ListaDeLivros(
            [FromQuery] LivroFiltro filtro, 
            [FromQuery] LivroOrdem ordem, 
            [FromQuery] LivroPaginacao paginacao) {

            var livroPaginado = _repo.All
                .AplicaFiltro(filtro)
                .AplicaOrdem(ordem)
                .Select(l => l.ToApi())
                .ToLivroPaginado(paginacao);

            return Ok(livroPaginado);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 404)]
        public IActionResult Recuperar(int id) {
            var model = _repo.Find(id);
            if (model == null)
                return NotFound();

            return Ok(model);
        }

        [HttpGet("{id}/capa")]
        public IActionResult ImagemCapa(int id) {
            byte[] img = _repo.All
                .Where(l => l.Id == id)
                .Select(l => l.ImagemCapa)
                .FirstOrDefault();
            if (img != null) {
                return File(img, "image/png");
            }
            return File("~/images/capas/capa-vazia.png", "image/png");
        }

        [HttpPost]
        public IActionResult Incluir([FromForm] LivroUpload model) {
            if (ModelState.IsValid) {
                var livro = model.ToLivro();
                _repo.Incluir(livro);
                var uri = Url.Action("Recuperar", new { id = livro.Id });
                return Created(uri, livro); // 201
            }
            return BadRequest(ErrorResponse.FromModelState(ModelState)); // 400
        }

        [HttpPut]
        public IActionResult Alterar([FromForm] LivroUpload model) {
            if (ModelState.IsValid) {
                var livro = model.ToLivro();
                if (model.Capa == null) {
                    livro.ImagemCapa = _repo.All
                        .Where(l => l.Id == livro.Id)
                        .Select(l => l.ImagemCapa)
                        .FirstOrDefault();
                }
                _repo.Alterar(livro);
                return Ok(); // 200
            }
            return BadRequest(); // 400
        }

        [HttpDelete("{id}")]
        public IActionResult Remover(int id) {
            var model = _repo.Find(id);
            if (model == null) {
                return NotFound(); // 404
            }
            _repo.Excluir(model);
            return NoContent(); // 204
        }
    }
}