# StackTeste — CRUD de Leads e Tasks

Implementação do desafio técnico: CRUD de **Leads** com **Tasks** associadas, usando **.NET 8 + EF Core** no backend e **Angular 19** no frontend.

---

## Stack

- **Backend:** .NET 8, ASP.NET Core Web API, Entity Framework Core, SQLite, AutoMapper, FluentValidation, xUnit
- **Frontend:** Angular 19 (standalone components), Angular Material, RxJS, Karma/Jasmine
- **Banco:** SQLite (arquivo local `stackteste.db`)

---

## Estrutura

```
StackTeste/
├── Backend/
│   ├── StackTeste.sln
│   ├── src/
│   │   ├── StackTeste.Api/             # Controllers, Program.cs, Middleware, Swagger
│   │   ├── StackTeste.Application/     # Services, DTOs, Validators (FluentValidation), AutoMapper
│   │   ├── StackTeste.Domain/          # Entities, Enums, Interfaces, Result<T>, PagedResult<T>
│   │   └── StackTeste.Infrastructure/  # DbContext, Repositories, Migrations
│   └── tests/
│       └── StackTeste.Application.Tests/  # Testes xUnit dos Services
└── Frontend/
    └── src/app/
        ├── core/
        │   ├── models/                 # Lead, TaskItem, enums
        │   └── services/               # LeadService, TaskService, NotificationService
        ├── pages/
        │   ├── leads/                  # Lista de Leads (tabela + filtros + paginação)
        │   └── lead-detail/            # Form de Lead + CRUD de Tasks (modal/página)
        └── shared/ui/                  # ConfirmDialog
```

---

## Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) e npm
- Ferramenta global do EF Core (caso ainda não tenha):

```bash
dotnet tool install --global dotnet-ef
```

---

## Como rodar — Backend

A connection string para configurar em `Backend/src/StackTeste.Api/appsettings.json` apontando para o arquivo SQLite local:
OBS: só está em um REDME.md pois se trata de um teste técnico

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=stackteste.db"
}
```

Na pasta `Backend/`:

```powershell
# 1. Restaurar dependências
dotnet restore

# 2. Aplicar as migrations e criar o banco SQLite
dotnet ef database update -s src/StackTeste.Api -p src/StackTeste.Infrastructure

# 3. Compilar
dotnet build

# 4. Subir a API
dotnet run --project src/StackTeste.Api/StackTeste.Api.csproj
```

A API fica disponível em:

- **Swagger:** http://localhost:5000/swagger
- **Base da API:** http://localhost:5000/api

### Rodar os testes do backend

Na pasta `Backend/`:

```powershell
dotnet test
```

> 23 testes (xUnit) cobrindo `LeadService` e `TaskService`.

---

## Como rodar — Frontend

A URL da API está configurada em `Frontend/src/environments/environment.ts`:

```ts
apiBase: "http://localhost:5000/api";
```

Na pasta `Frontend/`:

```powershell
# 1. Instalar dependências
npm install

# 2. Subir o app (com a API já rodando)
npm start
```

App disponível em **http://localhost:4200/**.

### Rodar os testes do frontend

Na pasta `Frontend/`:

```powershell
npm test
```

> 24 testes (Karma + Jasmine) sobre services e componentes.

---

## Endpoints

Base: `http://localhost:5000/api`

### Leads

| Método | Endpoint                                 | Descrição                            |
| ------ | ---------------------------------------- | ------------------------------------ |
| GET    | `/leads?search=&status=&page=&pageSize=` | Lista paginada com filtros           |
| GET    | `/leads/{id}`                            | Retorna o lead com suas tasks        |
| POST   | `/leads`                                 | Cria um lead                         |
| PUT    | `/leads/{id}`                            | Atualiza um lead                     |
| DELETE | `/leads/{id}`                            | Remove o lead (hard delete, cascata) |

### Tasks

| Método | Endpoint                         | Descrição           |
| ------ | -------------------------------- | ------------------- |
| GET    | `/leads/{leadId}/tasks`          | Lista tasks do lead |
| POST   | `/leads/{leadId}/tasks`          | Cria task no lead   |
| PUT    | `/leads/{leadId}/tasks/{taskId}` | Atualiza task       |
| DELETE | `/leads/{leadId}/tasks/{taskId}` | Remove task         |

Enums serializados como string (`"New"`, `"Qualified"`, `"Won"`, `"Lost"` para Lead; `"Todo"`, `"Doing"`, `"Done"` para Task).

---

## Funcionalidades implementadas

- CRUD completo de Leads e Tasks (Tasks aninhadas por Lead).
- Listagem de Leads com **busca por nome/email** (debounce no frontend) e **filtro por status**.
- **Paginação real no backend** (`page`, `pageSize`, com limite máximo de 100 itens por página).
- Validações com **FluentValidation** no backend (nome ≥ 3 caracteres, e-mail válido, status no enum) e mensagens espelhadas no frontend.
- **Mensagens claras de sucesso/erro** via Angular Material Snackbar; o frontend lê `ProblemDetails` e `ValidationProblemDetails` retornados pela API.
- **Swagger** habilitado.
- **CORS** configurado para o frontend em `http://localhost:4200`.
- Middleware global de tratamento de exceções no backend.
- `CreatedAt` / `UpdatedAt` aplicados automaticamente no `DbContext.SaveChanges`.
- Testes unitários cobrindo Services do backend e Services/componentes do frontend.

---

## Decisões de arquitetura

- **Camadas no backend** (`Api` → `Application` → `Domain` ← `Infrastructure`) com inversão de dependência via `Repositories` e `Services`.
- Padrão **Result<T>** para retornos de Service (sucesso/erro com mensagem), evitando exceções de fluxo.
- **AutoMapper** para mapear Entity ↔ DTO.
- **FluentValidation** desacoplada dos DTOs, com validação executada no Service antes da persistência.
- Frontend com **standalone components** do Angular 19 e lazy-loading da rota `/leads`.
- Componente `LeadDetailComponent` reaproveitado como **modal** (a partir da listagem) e como **página** (rota direta).
