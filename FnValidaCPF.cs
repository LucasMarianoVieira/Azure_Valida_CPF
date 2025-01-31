using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace http_valida_cpf
{
    public class FnValidaCPF
    {
        private readonly ILogger<FnValidaCPF> _logger;

        public FnValidaCPF(ILogger<FnValidaCPF> logger)
        {
            _logger = logger;
        }

        [Function("FnValidaCPF")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Iniciando a validação do CPF...");

            string request_body = String.Empty;
            using (StreamReader stream_reader = new StreamReader(req.Body))
            {
                request_body = await stream_reader.ReadToEndAsync();
            }
            dynamic? data = JsonConvert.DeserializeObject(request_body);
            if(data == null) {
                return new BadRequestObjectResult("Por favor, informe o CPF.");
            }
            string? cpf = data?.cpf;

            _logger.LogInformation($"CPF: {cpf}");

            if( ! valida_cpf(cpf) ){
                return new BadRequestObjectResult("CPF inválido!");
            } else {
                string response_message = "CPF válido!";
                return new OkObjectResult(response_message);
            }
        }

        public static bool valida_cpf(string? cpf)
        {
            // Rejeitar strings vazias ou nulas
            if (string.IsNullOrEmpty(cpf)) {
                return false;
            }

            // Remover marcadores
            // Verificar comprimento da string
            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
                return false;

            // Verificar se todos os dígitos são iguais
            if (new string(cpf[0], 11) == cpf)
                return false;

            // Calcular primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (10 - i);
            }
            int primeiro_digito = (soma * 10) % 11;
            if (primeiro_digito == 10)
                primeiro_digito = 0;

            if (primeiro_digito != int.Parse(cpf[9].ToString()))
                return false;

            // Calcular segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (11 - i);
            }
            int segundo_digito = (soma * 10) % 11;
            if (segundo_digito == 10)
                segundo_digito = 0;

            return segundo_digito == int.Parse(cpf[10].ToString());
        }

    }
}
