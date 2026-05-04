# DDD & Event Storming — FIAP Cloud Games

Este documento descreve a linguagem ubíqua, os bounded contexts e os fluxos de Event Storming do MVP do FCG.

---

## 1. Linguagem ubíqua

| Termo            | Significado                                                              |
|------------------|--------------------------------------------------------------------------|
| **User**         | Pessoa cadastrada na plataforma com papel `User`.                        |
| **Admin**        | Administrador da plataforma com privilégios estendidos.                  |
| **Game**         | Item de jogo digital disponível no catálogo.                             |
| **Library**      | Conjunto de jogos que um usuário específico já adquiriu.                 |
| **Acquisition**  | Ato de um usuário adicionar um jogo à sua biblioteca.                    |
| **Promotion**    | Desconto percentual com vigência limitada, definido por um Admin.        |
| **Audit Log**    | Registro imutável de uma ação relevante de negócio (MongoDB).            |

---

## 2. Bounded Contexts

O MVP é um monolito, mas o código-fonte é organizado em torno dos seguintes contextos lógicos:

1. **Identidade e Acesso** — cadastro, autenticação, emissão de JWT, papéis.
2. **Catálogo de Jogos** — CRUD de jogos, soft delete, listagem.
3. **Biblioteca do Usuário** — aquisições, listagem da biblioteca do usuário.
4. **Promoções** — CRUD de promoções com regras de validade.
5. **Auditoria** — contexto transversal que registra eventos relevantes no MongoDB.

```
┌──────────────────────┐     ┌─────────────────────┐     ┌────────────────────┐
│ Identidade e Acesso  │     │  Catálogo de Jogos  │     │     Promoções      │
│  - Agregado User     │     │  - Agregado Game    │     │ - Agregado Promo.  │
└─────────┬────────────┘     └─────────┬───────────┘     └─────────┬──────────┘
          │                            │                           │
          ▼                            ▼                           │
┌──────────────────────────────────────────────┐                   │
│            Biblioteca do Usuário             │◀──────────────────┘
│  - UserGame (aquisição por usuário)          │
└──────────────────────┬───────────────────────┘
                       │
                       ▼
              ┌────────────────────┐
              │     Auditoria      │  (MongoDB — somente escrita)
              └────────────────────┘
```

---

## 3. Event Storming — Fluxo de Cadastro de Usuário

**Comando:** `Register User`

**Eventos:**
- User Registration Requested
- User Email Validated
- User Password Validated
- User Registered
- Audit Log Registered

**Regras de negócio:**
- E-mail deve ter formato válido.
- Senha deve ser segura (≥ 8 caracteres, letras, números e caracteres especiais).
- E-mail deve ser único.

**Agregado:** `User`

```
[Comando: Register User]
        │
        ▼
[Evento: User Registration Requested]
        │
        ├── valida nome / e-mail / senha ──▶ [Evento: User Email Validated]
        │                                  ▶ [Evento: User Password Validated]
        ▼
[Evento: User Registered]
        │
        ▼
[Evento: Audit Log Registered]  (MongoDB: action = "UserRegistered")
```

---

## 4. Event Storming — Fluxo de Criação de Jogo

**Comando:** `Create Game`

**Eventos:**
- Game Creation Requested
- Admin Permission Validated
- Game Created
- Audit Log Registered

**Regras de negócio:**
- Apenas Admin pode criar jogos.
- Título do jogo é obrigatório.
- Preço do jogo deve ser válido (≥ 0).

**Agregado:** `Game`

```
[Comando: Create Game]
        │
        ▼
[Evento: Game Creation Requested]
        │
        ├── verifica papel (Admin) ──▶ [Evento: Admin Permission Validated]
        │
        ▼
[Evento: Game Created]
        │
        ▼
[Evento: Audit Log Registered]  (MongoDB: action = "GameCreated")
```

---

## 5. Event Storming — Fluxo de Aquisição de Jogo

**Comando:** `Acquire Game`

**Eventos:**
- Game Acquisition Requested
- Game Availability Validated
- User Library Checked
- Game Acquired
- Audit Log Registered

**Regras de negócio:**
- Usuário deve estar autenticado.
- Jogo deve existir.
- Jogo deve estar ativo.
- Usuário não pode adquirir o mesmo jogo duas vezes.

**Agregado:** `User Library`

```
[Comando: Acquire Game]
        │
        ▼
[Evento: Game Acquisition Requested]
        │
        ├── carrega jogo ──▶ [Evento: Game Availability Validated]
        │
        ├── carrega biblioteca do usuário ──▶ [Evento: User Library Checked]
        │
        ▼
[Evento: Game Acquired]
        │
        ▼
[Evento: Audit Log Registered]  (MongoDB: action = "GameAcquired")
```

---

## 6. Event Storming — Fluxo de Criação de Promoção

**Comando:** `Create Promotion`

**Eventos:**
- Promotion Creation Requested
- Admin Permission Validated
- Promotion Created
- Audit Log Registered

**Regras de negócio:**
- Apenas Admin pode criar promoções.
- Percentual de desconto deve estar entre 1 e 100.
- Data final deve ser maior que a data inicial.

**Agregado:** `Promotion`

```
[Comando: Create Promotion]
        │
        ▼
[Evento: Promotion Creation Requested]
        │
        ├── verifica papel (Admin) ──▶ [Evento: Admin Permission Validated]
        │
        ▼
[Evento: Promotion Created]
        │
        ▼
[Evento: Audit Log Registered]  (MongoDB: action = "PromotionCreated")
```

---

## Mapeamento dos eventos no código

| Evento                          | Onde acontece                                                   |
|---------------------------------|------------------------------------------------------------------|
| User Registered                 | `AuthService.RegisterAsync`                                     |
| User Logged In                  | `AuthService.LoginAsync`                                        |
| Game Created/Updated/Deleted    | `GameService.CreateAsync` / `UpdateAsync` / `DeleteAsync`       |
| Game Acquired                   | `LibraryService.AcquireAsync`                                   |
| Promotion Created               | `PromotionService.CreateAsync`                                  |
| Audit Log Registered            | `MongoAuditLogger.LogAsync` (chamado pelos serviços acima)      |
