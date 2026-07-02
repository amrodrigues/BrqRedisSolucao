# 🚀 API de Consulta de Saldo Bancário com Resiliência e Cache (Redis + .NET 8)

Este é um projeto laboratório que simula o comportamento de um sistema de **Missão Crítica** e **Baixa Latência** no contexto de sistemas bancários modernos (**RTB/SCB**). 

A aplicação demonstra a implementação prática do padrão **Cache-Aside** utilizando o **Redis** como camada de cache de alta performance sobreposta a um banco de dados relacional simulado, focando em conceitos fundamentais de sustentação e suporte à produção (N2/N3), como **resiliência** e **degradação suave** (*graceful degradation*).

---

## 🛠️ Tecnologias Utilizadas

* **Plataforma:** .NET 8 (Web API / Minimal APIs)
* **Linguagem:** C# 12
* **Cache em Memória:** Redis (via pacote oficial `StackExchange.Redis`)
* **Persistência de Dados:** SQL Server (Camada simulada com injeção de latência)
* **Containers:** Docker & Docker Compose
* **Formatos:** JSON (System.Text.Json)

---

## 📈 Arquitetura e Cenários de Resiliência Demonstrados

### 1. Padrão Cache-Aside (Cenário de Alta Disponibilidade)
Ao efetuar uma requisição de consulta de saldo, a aplicação valida a existência do dado na memória RAM do Redis (*Cache Hit*). Caso não encontre (*Cache Miss*), busca na base relacional lenta, alimenta o Redis com um **TTL (Time to Live) de 1 minuto** para evitar dados obsoletos e retorna instantaneamente as requisições subsequentes.

### 2. Degradação Suave (Cenário de Incidente de Produção)
O código foi estruturado com políticas de tratamento de exceções específicas para infraestrutura (`RedisConnectionException`). Caso o cluster do Redis sofra uma queda ou instabilidade de rede no ambiente produtivo:
* A aplicação captura a falha de forma segura.
* Registra o erro nos logs do sistema para atuação do suporte.
* **Não interrompe o fluxo do usuário:** Redireciona a requisição diretamente para o banco de dados principal de forma transparente, garantindo a continuidade do negócio (*Business Continuity*).

---

## 🚀 Como Executar o Projeto Localmente

### Pré-requisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) ativo na máquina.

### Passo 1: Subir a Infraestrutura do Redis
Na raiz do projeto onde se encontra o arquivo `docker-compose.yml`, execute o comando abaixo para iniciar o container do Redis em segundo plano:

```bash
docker-compose up -d
```
### Passo 2: Executar a API .NET
Ainda no terminal, inicie a aplicação web:

```bash
dotnet run
```

### Passo 3: Testar os Cenários no Postman / Navegador
Realize uma requisição GET para o endpoint de contas (ex: substituindo pela sua porta local):

```bash
http://localhost:5130/api/contas/123
```

### Com o Docker ligado faça a conexão:

<img width="1345" height="519" alt="docker ligado" src="https://github.com/user-attachments/assets/b632d3a7-fa04-4aef-9446-2f4f57cf8ad9" />


* Primeira Execução: O sistema apresentará uma latência de 2 segundos (Simulação de query pesada no SQL Server) e o JSON indicará Fonte: SQL Server.

<img width="937" height="606" alt="BrqSql" src="https://github.com/user-attachments/assets/467b0b31-00f7-4c75-a3c8-5bdce84d0045" />

* Segunda Execução (Imediata): O retorno será instantâneo (0ms), exibindo Fonte: Redis (Cache).

<img width="925" height="444" alt="BrqRedis" src="https://github.com/user-attachments/assets/17d06077-3817-452a-aabf-c0edaec30553" />

###  Com o Docker desligado:

<img width="1345" height="494" alt="Dockedesligado" src="https://github.com/user-attachments/assets/170c1e03-b6f6-46ac-8d11-9a6ef2072d6e" />

* Simulação de Falha: Execute docker-compose stop no terminal para derrubar o Redis e repita a requisição. A aplicação continuará respondendo (via SQL Server), demonstrando a resiliência do ecossistema.

<img width="937" height="606" alt="BrqSql" src="https://github.com/user-attachments/assets/467b0b31-00f7-4c75-a3c8-5bdce84d0045" />





