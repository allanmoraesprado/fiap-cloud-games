# FIAP Cloud Games (FCG) — Tech Challenge MVP

API REST em **.NET 8** para a plataforma FIAP Cloud Games: uma plataforma de jogos digitais voltada à educação em tecnologia. Este MVP permite que usuários se cadastrem, façam login, consultem jogos, adquiram jogos para a sua biblioteca pessoal e, no caso de administradores, gerenciem o catálogo e as promoções.

> **Entrega individual.** Esta é uma entrega individual do Tech Challenge da pós-graduação. Por isso, organizei o escopo para ser realista, objetivo e alinhado aos requisitos da fase, mantendo qualidade, clareza e uma estrutura preparada para evolução.

---

## Tecnologias utilizadas

- .NET 8 / ASP.NET Core (Controllers MVC — não Minimal APIs)
- Entity Framework Core 8 + Npgsql (PostgreSQL)
- JWT Bearer + autorização baseada em papéis
- Swagger / OpenAPI (Swashbuckle)
- Serilog (logs estruturados)
- xUnit + Moq + FluentAssertions (testes unitários)
- Docker Compose (PostgreSQL e MongoDB)

**Componentes opcionais (melhorias adicionais):**
- **MongoDB** — armazenamento de logs de auditoria.
- **Dapper** — consultas de leitura otimizadas.

---

## Arquitetura

Estrutura em camadas inspirada em **DDD**:

```
src/
  FiapCloudGames.Api/             ← Controllers, Middleware, Swagger, DI, Program.cs
  FiapCloudGames.Application/     ← DTOs, Services, Interfaces, Validators
  FiapCloudGames.Domain/          ← Entities, Enums, Domain Exceptions, Repository contracts
  FiapCloudGames.Infrastructure/  ← EF Core, Repositories, Dapper, Mongo, Security (JWT, Hasher)

tests/
  FiapCloudGames.Tests/           ← Testes unitários com xUnit

docs/
  DDD.md                          ← Linguagem ubíqua, bounded contexts e Event Storming
  DELIVERY_REPORT.md              ← Relatório de entrega
```

### Por que monolito?

Por se tratar da primeira fase do projeto, o **monolito** é a escolha mais adequada:

- Permite consolidar o domínio antes de qualquer decomposição em serviços.
- Possui unidade de implantação única, simples de executar localmente.
- A separação por camadas e contextos delimitados facilita uma futura divisão em microsserviços.
- Reduz a sobrecarga operacional, mantendo o foco nas regras de negócio.

---

## Como executar PostgreSQL e MongoDB (Docker Compose)

> **Importante:** apenas as bases de dados (PostgreSQL e MongoDB) rodam em contêineres via Docker Compose. A **API .NET roda localmente** (via `dotnet run`, Visual Studio, Rider ou VS Code) e se conecta a esses contêineres.

```bash
docker compose up -d
```

Isso sobe:
- **PostgreSQL** em `localhost:5432` (usuário `fcg`, senha `fcg`, banco `fcg`)
- **MongoDB** em `localhost:27017` (usuário `fcg`, senha `fcg`)

Para parar:
```bash
docker compose down
```

---

## Configuração (`appsettings.Development.json`)

As configurações de desenvolvimento já correspondem às credenciais do Docker Compose. Ajuste se mudar portas ou credenciais.

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=fcg;Username=fcg;Password=fcg"
  },
  "Mongo": {
    "ConnectionString": "mongodb://fcg:fcg@localhost:27017",
    "Database": "fcg_audit",
    "AuditCollection": "audit_logs"
  },
  "Jwt": {
    "Issuer": "FiapCloudGames",
    "Audience": "FiapCloudGames",
    "SecretKey": "dev-secret-key-please-change-in-production-32-bytes-min",
    "ExpirationMinutes": 120
  }
}
```

> Em produção, utilize uma **chave secreta forte e aleatória** (≥ 32 bytes).

---

## Migrações do EF Core

Instale o EF Core CLI uma vez (apenas na primeira execução):

```bash
dotnet tool install --global dotnet-ef
```

Na raiz do repositório, gere a migração inicial:

```bash
dotnet ef migrations add InitialCreate ^
  --project src/FiapCloudGames.Infrastructure ^
  --startup-project src/FiapCloudGames.Api
```

Aplique as migrações no banco:

```bash
dotnet ef database update ^
  --project src/FiapCloudGames.Infrastructure ^
  --startup-project src/FiapCloudGames.Api
```

> A API também executa `Database.MigrateAsync()` e popula dados de exemplo no startup, por conveniência durante o desenvolvimento.

---

## Executar a API

```bash
dotnet run --project src/FiapCloudGames.Api
```

Abra o Swagger:

```
https://localhost:<porta>/swagger
```

(A porta exata é exibida no console na inicialização.)

---

## Executar os testes

```bash
dotnet test
```

---

## Credenciais de exemplo (seed)

| Papel | E-mail          | Senha      |
|-------|-----------------|------------|
| Admin | admin@fcg.com   | Admin@123  |
| User  | user@fcg.com    | User@123   |

As senhas são armazenadas apenas como hashes PBKDF2 — nunca em texto puro.

---

## Usando o Swagger (fluxo sugerido)

1. `POST /api/auth/login` com as credenciais de **Admin**.
2. Copie o `token` da resposta.
3. Clique em **Authorize** no canto superior direito e cole: `Bearer <token>`.
4. `POST /api/games` para criar um jogo.
5. `POST /api/promotions` para criar uma promoção.
6. Faça logout / Authorize novamente com as credenciais de **User**.
7. `POST /api/library/acquire/{gameId}` para adquirir um jogo.
8. `GET /api/library/my-games` para visualizar a biblioteca.

> Os endpoints públicos (`/api/auth/register` e `/api/auth/login`) **não** exibem o cadeado no Swagger. Apenas os endpoints protegidos exibem.

---

## Principais endpoints

### Auth (públicos)
- `POST /api/auth/register`
- `POST /api/auth/login`

### Games
- `GET /api/games` — autenticado
- `GET /api/games/{id}` — autenticado
- `POST /api/games` — Admin
- `PUT /api/games/{id}` — Admin
- `DELETE /api/games/{id}` — Admin (soft delete: `IsActive = false`)

### Library
- `POST /api/library/acquire/{gameId}` — autenticado
- `GET /api/library/my-games` — autenticado
- `GET /api/library/user/{userId}` — Admin

### Promotions
- `GET /api/promotions` — autenticado
- `GET /api/promotions/{id}` — autenticado
- `POST /api/promotions` — Admin
- `PUT /api/promotions/{id}` — Admin
- `DELETE /api/promotions/{id}` — Admin

### Users
- `GET /api/users` — Admin
- `GET /api/users/{id}` — Admin
- `DELETE /api/users/{id}` — Admin

---

## Uso do Entity Framework Core

O EF Core é responsável por **toda a persistência relacional** do modelo de escrita:

- Cadastro de usuários
- Criação, atualização e soft delete de jogos
- Aquisição de jogos (escrita em `UserGame`)
- Criação e atualização de promoções

As configurações de entidades ficam em `src/FiapCloudGames.Infrastructure/Persistence/Configurations`. O `FcgDbContext` também implementa `IUnitOfWork` para deixar a fronteira transacional explícita na camada de aplicação.

---

## Componentes opcionais (melhorias adicionais)

> Os itens abaixo são **requisitos opcionais** do Tech Challenge. Foram implementados como melhorias para enriquecer o MVP.

### MongoDB (auditoria)

Implementado em `MongoAuditLogger`. Registra logs de auditoria para as ações relevantes:

- `UserRegistered`
- `UserLoggedIn`
- `GameCreated`
- `GameUpdated`
- `GameDeleted`
- `GameAcquired`
- `PromotionCreated`

A escrita de auditoria é resiliente: falhas no MongoDB não bloqueiam o caso de uso principal — são apenas registradas como warnings.

### Dapper (consultas otimizadas)

Implementado em `GameQueryService`, é utilizado em duas leituras críticas:

- `GET /api/games` → `GameQueryService.ListActiveGamesAsync`
- `GET /api/library/my-games` → `GameQueryService.ListUserLibraryAsync`

Mantém o caminho de leitura enxuto, sem o overhead do change tracker do EF Core, enquanto o EF Core continua sendo o responsável pelo modelo de escrita.

---

## Testes unitários

Os testes unitários validam as **principais regras de negócio** do projeto:

- Validação de cadastro (nome, formato de e-mail, força de senha)
- E-mail duplicado
- Login com credenciais inválidas
- Usuário não pode adquirir o mesmo jogo duas vezes
- Não-admin não pode criar jogos / promoções
- Admin pode criar jogos / promoções
- Desconto de promoção precisa estar entre 1 e 100
- `EndDate` da promoção precisa ser maior que `StartDate`

O objetivo é cobrir as regras críticas de negócio com testes automatizados, garantindo qualidade e previsibilidade do comportamento.

---

## DDD e Event Storming

A solução aplica uma estrutura simples inspirada em DDD: linguagem ubíqua, bounded contexts, agregados e exceções de domínio explícitas. Os fluxos de **Event Storming** dos casos de uso principais (cadastro de usuário, criação de jogo, aquisição de jogo e criação de promoção) estão documentados em [docs/DDD.md](docs/DDD.md).

---

## Status do projeto

Esta é uma **entrega acadêmica individual** — um MVP objetivo e bem estruturado, pronto para evoluir nas próximas fases (separação em microsserviços, integração de pagamento, motor de recomendações, entre outros).
