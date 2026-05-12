# Relatório de Entrega — FIAP Cloud Games

| Campo                          | Valor                                          |
|--------------------------------|------------------------------------------------|
| **Nome do grupo**              | Entrega Individual — FIAP Cloud Games          |
| **Participante**               | Allan Moraes Prado |
| **Usuário do Discord**         | allan4653 |
| **Link do repositório**        | https://github.com/allanmoraesprado/fiap-cloud-games |
| **Link do vídeo (10–15 min)**  | https://youtu.be/MemEp5_tmeg |

---

## Descrição do projeto

O **FIAP Cloud Games (FCG)** é o MVP de uma plataforma de jogos digitais voltada à educação em tecnologia. Esta entrega corresponde à **primeira fase**: uma API REST monolítica em .NET 8 que cobre cadastro e autenticação de usuários, autorização por papel (User e Admin), gestão de catálogo de jogos, biblioteca pessoal de jogos adquiridos e promoções. O projeto também aplica conceitos de DDD (linguagem ubíqua, bounded contexts e Event Storming) e inclui testes unitários para as principais regras de negócio.

> Esta é uma **entrega individual**. Por isso, organizei o escopo para ser realista, objetivo e alinhado aos requisitos da fase, mantendo qualidade, clareza e uma estrutura preparada para evolução.

---

## Tecnologias utilizadas

- .NET 8 / ASP.NET Core (Controllers MVC)
- Entity Framework Core 8 + Npgsql + PostgreSQL
- Dapper (consultas otimizadas)
- MongoDB (logs de auditoria)
- JWT Bearer + Authorization Policies
- Swagger / OpenAPI (Swashbuckle) com suporte ao botão Authorize
- Serilog (logs estruturados)
- xUnit + Moq + FluentAssertions
- Docker Compose (PostgreSQL e MongoDB)

---

## Resumo dos requisitos implementados

### Cadastro de usuário
- Campos: nome, e-mail e senha.
- Validações: formato de e-mail, senha forte (mínimo 8 caracteres, letras, números e caracteres especiais), e-mail único.
- Senha armazenada como hash PBKDF2 (100.000 iterações + salt).

### Autenticação
- Login via e-mail e senha.
- Geração de JWT contendo `UserId`, `Email` e `Role`.

### Autorização (papéis)
- **User**: acessa a plataforma, lista jogos, adquire jogos e visualiza a própria biblioteca.
- **Admin**: cria/atualiza/remove (soft delete) jogos, gerencia usuários, cria promoções e pode ver a biblioteca de qualquer usuário.

### Jogos
- CRUD completo restrito a Admin.
- Listagem e detalhe acessíveis a usuários autenticados.
- Validação: título obrigatório, preço não negativo.
- Soft delete via `IsActive = false`.

### Biblioteca do usuário
- `POST /api/library/acquire/{gameId}` — usuário autenticado.
- `GET /api/library/my-games` — usuário autenticado.
- `GET /api/library/user/{userId}` — Admin.
- Regras: jogo existente, jogo ativo, sem duplicidade.

### Promoções
- CRUD restrito a Admin (com `GET` aberto a usuários autenticados).
- Validações: desconto entre 1 e 100, `EndDate` maior que `StartDate`.

### Documentação e qualidade
- Swagger configurado com autenticação JWT.
- Middleware global de exceções mapeando exceções de domínio para os HTTP status corretos.
- Logs estruturados via Serilog.
- Documentação DDD com Event Storming dos quatro fluxos principais.
- Testes unitários cobrindo as regras críticas.

---

## Resumo dos requisitos opcionais implementados

### MongoDB — Auditoria
Implementado em `MongoAuditLogger`. Eventos registrados:

- `UserRegistered`
- `UserLoggedIn`
- `GameCreated`
- `GameUpdated`
- `GameDeleted`
- `GameAcquired`
- `PromotionCreated`

A escrita de auditoria é resiliente: falhas no MongoDB não afetam o caso de uso principal.

### Dapper — Leituras otimizadas
Implementado em `GameQueryService`:

- `ListActiveGamesAsync` — utilizado por `GET /api/games`.
- `ListUserLibraryAsync` — utilizado por `GET /api/library/my-games`.

Mantém o caminho de leitura enxuto enquanto o EF Core continua responsável pela escrita.

---

## Observações finais

O projeto está pronto para ser executado localmente conforme o `README.md`, com:

```bash
docker compose up -d
dotnet run --project src/FiapCloudGames.Api
```

A demonstração das funcionalidades é realizada via Swagger, conforme o fluxo sugerido no `README.md`.
