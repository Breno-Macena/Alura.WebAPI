using Alura.ListaLeitura.Modelos;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Lista = Alura.ListaLeitura.Modelos.ListaLeitura;

namespace Alura.ListaLeitura.HttpClients {
    public class LivroApiClient {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _accessor;

        public LivroApiClient(HttpClient httpClient, IHttpContextAccessor accessor) {
            _httpClient = httpClient;
            _accessor = accessor;
        }

        private void AddBearerToken() {
            var token = _accessor.HttpContext.User.Claims.First(c => c.Type == "Token").Value;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<Lista> GetListaLeituraAsync(TipoListaLeitura tipo) {
            AddBearerToken();
            var httpResponse = await _httpClient.GetAsync($"listasleitura/{tipo}");
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsAsync<Lista>();
        }

        public async Task DeleteLivroAsync(int id) {
            AddBearerToken();
            var httpResponse = await _httpClient.DeleteAsync($"livros/{id}");
            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task<LivroApi> GetLivroAsync(int id) {
            AddBearerToken();
            var httpResponse = await _httpClient.GetAsync($"livros/{id}");
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsAsync<LivroApi>();
        }

        public async Task<byte[]> GetCapaLivroAsync(int id) {
            AddBearerToken();
            var httpResponse = await _httpClient.GetAsync($"livros/{id}/capa");
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsByteArrayAsync();
        }

        public async Task PostLivroAsync(LivroUpload model) {
            AddBearerToken();
            HttpContent content = CreateMultipartFormDataContent(model);
            var httpResponse = await _httpClient.PostAsync("livros", content);
            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task PutLivroAsync(LivroUpload model) {
            AddBearerToken();
            HttpContent content = CreateMultipartFormDataContent(model);
            var httpResponse = await _httpClient.PutAsync("livros", content);
            httpResponse.EnsureSuccessStatusCode();
        }

        private string NormalizeStringParameter(string valor) {
            return $"\"{valor}\"";
        }

        private HttpContent CreateMultipartFormDataContent(LivroUpload model) {
            var content = new MultipartFormDataContent {
                { new StringContent(model.Titulo), NormalizeStringParameter("titulo") },
                { new StringContent(model.Lista.ParaString()), NormalizeStringParameter("lista") }
            };
            if (model.Id > 0) 
                content.Add(new StringContent(model.Id.ToString()), NormalizeStringParameter("id"));

            if (!string.IsNullOrWhiteSpace(model.Subtitulo))
                content.Add(new StringContent(model.Subtitulo), NormalizeStringParameter("subtitulo"));

            if (!string.IsNullOrWhiteSpace(model.Autor))
                content.Add(new StringContent(model.Autor), NormalizeStringParameter("autor"));

            if (!string.IsNullOrWhiteSpace(model.Resumo))
                content.Add(new StringContent(model.Resumo), NormalizeStringParameter("resumo"));

            if (model.Capa != null) {
                var imageContent = new ByteArrayContent(model.Capa.ConvertToBytes());
                imageContent.Headers.Add("content-type", "image/png");
                content.Add(
                    imageContent, 
                    NormalizeStringParameter("capa"),
                    NormalizeStringParameter("capa.png")
                );
            }

            return content;
        }
    }
}
