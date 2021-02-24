using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alura.ListaLeitura.Api.Controllers {
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LivrosController : ControllerBase {
        
        private readonly IRepository<Livro> _repo;

        public LivrosController(IRepository<Livro> repo) {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult Recuperar() {
            var header = this.HttpContext.Request.Headers;
            var lista = _repo.All.Select(l => l.ToApi()).ToList();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public IActionResult Recuperar(int id) {
            var model = _repo.Find(id);
            if (model == null)
                return NotFound();

            return Ok(model.ToApi());
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
            return BadRequest(); // 400
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