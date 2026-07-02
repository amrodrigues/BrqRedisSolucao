using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configuração e Injeção do Redis com tratamento simples de conexão
try
{
    var multiplexer = ConnectionMultiplexer.Connect("localhost:6379,connectTimeout=2000,allowAdmin=true");
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
}
catch (Exception ex)
{
    Console.WriteLine($"[AVISO] Não foi possível conectar ao Redis na inicialização: {ex.Message}");
}

var app = builder.Build();

// Endpoint para consultar o saldo da conta (Padrão Cache-Aside com Resiliência)
app.MapGet("/api/contas/{id}", async (string id, [FromServices] IConnectionMultiplexer? redis) =>
{
    string cacheKey = $"conta:saldo:{id}";
    IDatabase? cache = redis?.GetDatabase();

    // 1. PASSO: Tenta buscar no Redis
    if (cache != null)
    {
        try
        {
            var dadosEmCache = await cache.StringGetAsync(cacheKey);
            if (!dadosEmCache.IsNullOrEmpty)
            {
                return Results.Ok(new { Fonte = "Redis (Cache)", Dados = JsonSerializer.Deserialize<ContaSaldo>(dadosEmCache!.ToString()) });
            }
        }
        catch (RedisConnectionException ex)
        {
            // SUSTENTAÇÃO: Se o Redis falhar, logamos o erro e o fluxo continua para o banco
            Console.WriteLine($"[Erro de Conexão Redis]: {ex.Message}. Redirecionando para o Banco Relacional...");
        }
    }

    // 2. PASSO: Cache Miss ou Redis fora do ar. Busca na base de dados (Simulada)
    var dadosDoBanco = await SimularConsultaSqlServerLenta(id);

    if (dadosDoBanco == null) return Results.NotFound(new { Mensagem = "Conta não encontrada" });

    // 3. PASSO: Salva no Redis de forma assíncrona para a próxima requisição, com expiração (TTL) de 1 minuto
    if (cache != null)
    {
        try
        {
            var stringItem = JsonSerializer.Serialize(dadosDoBanco);
            await cache.StringSetAsync(cacheKey, stringItem, TimeSpan.FromMinutes(1));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Erro ao gravar no Redis]: {ex.Message}");
        }
    }

    return Results.Ok(new { Fonte = "SQL Server (Banco Principal)", Dados = dadosDoBanco });
});

app.Run();

// Métodos auxiliares e modelos para o teste
static async Task<ContaSaldo?> SimularConsultaSqlServerLenta(string id)
{
    // Simula a latência de uma query pesada/complexa no SQL Server (2 segundos de delay)
    await Task.Delay(2000);

    return new ContaSaldo(id, "Anna Maria G. B. Rodrigues", 15450.75m, DateTime.Now);
}

public record ContaSaldo(string ContaId, string Titular, decimal Saldo, DateTime UltimaAtualizacao);