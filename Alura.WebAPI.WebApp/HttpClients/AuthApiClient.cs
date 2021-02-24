using Alura.ListaLeitura.Seguranca;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alura.ListaLeitura.HttpClients {
    public class LoginResult {
        public bool Succeeded { get; set; }
        public string Token { get; set; }
    }

    public class AuthApiClient {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<LoginResult> PostLoginAsync(LoginModel model) {
            var httpResponse = await _httpClient.PostAsJsonAsync("login", model);
            return new LoginResult {
                Succeeded = httpResponse.IsSuccessStatusCode,
                Token = await httpResponse.Content.ReadAsStringAsync()
            };
        }
    }
}
