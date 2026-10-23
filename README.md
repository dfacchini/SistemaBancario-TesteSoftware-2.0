# Sistema Bancário

Aplicação web de gestão bancária com **clientes**, **contas correntes** e **transferências** entre
contas. É composta por:

- **API (back-end)** — ASP.NET Core *minimal API* em **.NET 10**, com Entity Framework Core 10 e
  banco **SQLite**;
- **Client (front-end)** — SPA em **Vue 3 + Vuetify 4 + Pinia + Vue Router**, construída com
  **Vite** e TypeScript.

Os requisitos funcionais (RF-01 a RF-18) estão descritos em
[`requisitos-funcionais.md`](requisitos-funcionais.md) e são referenciados nos comentários do código.

## Funcionalidades

| Área           | Operações                                                                                              |
|----------------|--------------------------------------------------------------------------------------------------------|
| Clientes       | listar, consultar por ID, cadastrar (idade entre 18 e 65 anos), ativar, desativar, remover, limpar base |
| Contas         | listar (com titular), consultar por ID, cadastrar com saldo inicial, ativar, desativar, remover          |
| Transferências | transferir valor entre duas contas ativas, com validação de saldo e controle de concorrência             |

Ao cadastrar um cliente sem informar uma conta, uma conta corrente nova é aberta para ele. Cada conta
pertence a no máximo um cliente, e uma conta vinculada a um cliente não pode ser removida.

Na primeira execução o banco é criado com dados pré-cadastrados (RF-18):

| Tipo    | ID | Dados                                                     |
|---------|----|-----------------------------------------------------------|
| Conta   | 1  | saldo R$ 1.000,00, ativa                                  |
| Conta   | 2  | saldo R$ 1.000,00, ativa                                  |
| Cliente | 1  | Maria Silva, 31 anos, mariasilva@gmail.com, conta 1       |
| Cliente | 2  | Felipe Augusto, 34 anos, felipeaugusto@gmail.com, conta 2 |

## Estrutura do projeto

```text
SistemaBancario/
├── SistemaBancario.slnx              # solução (.NET)
├── SistemaBancario.csproj            # projeto da API (fica na raiz, junto do Program.cs)
├── Program.cs                        # registro de serviços, middlewares e rotas
├── Endpoints/                        # um arquivo por grupo de rotas
│   ├── ClientesEndpoints.cs          #   /api/clientes
│   ├── ContasEndpoints.cs            #   /api/contas
│   ├── TransferenciasEndpoints.cs    #   /api/transferencias
│   └── Contratos.cs                  #   records de requisição/resposta (DTOs)
├── Migrations/                       # migrations do EF Core (SQLite)
├── Properties/launchSettings.json    # perfis de execução (http / https)
├── appsettings*.json                 # logging e connection string "Banco"
├── SistemaBancario.http              # exemplos de requisições (REST Client do VS / VS Code)
├── dotnet-tools.json                 # ferramenta local dotnet-ef
├── requisitos-funcionais.md          # requisitos RF-01..RF-18
│
├── SistemaBancario.Dominio/          # biblioteca de domínio (independente do provedor de banco)
│   ├── Entidades/                    #   Cliente, ContaCorrente (regras nos construtores/métodos)
│   ├── Excecoes/                     #   IdadeNaoPermitidaException
│   └── Persistencia/
│       ├── BancoDbContext.cs         #   DbContext do EF Core
│       └── Configuracoes/            #   mapeamentos Fluent API + dados iniciais (HasData)
│
└── sistemabancario.client/           # front-end Vue
    ├── vite.config.mts               # porta 3000 e proxy de /api para a API
    └── src/
        ├── api/                      #   cliente HTTP e tipos (espelham Endpoints/Contratos.cs)
        ├── pages/                    #   Painel, Clientes, Contas, Transferências
        ├── components/               #   diálogos de formulário, consulta por ID, chips etc.
        ├── stores/                   #   stores Pinia (banco, notificações)
        ├── router/                   #   rotas registradas manualmente
        ├── plugins/                  #   Vuetify, Pinia, Router
        ├── styles/                   #   Tailwind + configurações do Vuetify
        └── utils/                    #   regras de formulário e formatação em BRL
```

### Back-end

- `Program.cs` registra o OpenAPI, o `BancoDbContext` com SQLite e aplica as migrations pendentes ao
  iniciar (`Database.Migrate()`), criando o arquivo `sistemabancario.db` na raiz do projeto.
- As entidades de domínio têm *setters* privados e validam suas regras em construtores e métodos; o
  mapeamento é feito apenas por classes `IEntityTypeConfiguration<T>` (sem *data annotations*).
- As migrations ficam no projeto da API, para que o domínio não dependa do SQLite.
- Os erros são retornados como **ProblemDetails**; o front-end exibe o campo `detail` ao usuário.

### Front-end

- As chamadas usam URLs relativas (`/api/...`), encaminhadas pelo **proxy do Vite** para
  `http://localhost:5285`. Por isso a API não precisa de política de CORS em desenvolvimento.
- O estado fica em stores Pinia: `banco.ts` mantém clientes e contas e recarrega as duas listas após
  cada alteração; `notificacoes.ts` alimenta as mensagens (snackbar) da aplicação.
- Rotas: Painel `/`, `/clientes`, `/contas` e `/transferencias`.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20.19+ ou 22.12+ (exigido pelo Vite 8) e npm

## Como executar

A API e o front-end rodam em processos separados. Use dois terminais.

### 1. Back-end (API)

Na raiz do repositório:

```bash
dotnet restore
dotnet run
```

A API sobe em **http://localhost:5285** (perfil `http`). Na primeira execução o banco SQLite é criado
com os dados iniciais.

Outras opções:

```bash
dotnet run --launch-profile https   # https://localhost:7091 + http://localhost:5285
dotnet watch                        # recarrega automaticamente ao alterar o código
```

Em ambiente de desenvolvimento o documento OpenAPI fica em
<http://localhost:5285/openapi/v1.json>. Para testar os endpoints manualmente, abra
`SistemaBancario.http` no Visual Studio ou no VS Code (extensão REST Client).

### 2. Front-end (client)

Com a API já em execução, em outro terminal:

```bash
cd sistemabancario.client
npm install
npm run dev
```

Acesse **http://localhost:3000** (se a porta estiver ocupada, o Vite usa a 3001 ou seguinte).

> O front-end depende da API escutando na porta **5285**. Se a API não estiver rodando, as telas
> exibirão erro ao carregar os dados.

### Outros comandos do front-end

```bash
npm run build       # checagem de tipos (vue-tsc) + build de produção em dist/
npm run preview     # serve o build gerado
npm run type-check  # apenas checagem de tipos
npm run lint        # ESLint (npm run lint:fix corrige estilo e ordem de imports)
```

## Banco de dados e migrations

O banco é um arquivo SQLite (`sistemabancario.db`) configurado em `appsettings.json`
(`ConnectionStrings:Banco`). Para recriá-lo do zero, pare a API e apague os arquivos
`sistemabancario.db*`; ele será recriado na próxima execução.

Ao alterar entidades, mapeamentos ou dados iniciais, gere uma nova migration com a ferramenta
`dotnet-ef` local (versão 10):

```bash
dotnet tool restore
dotnet dotnet-ef migrations add NomeDaMigration
```

> Use `dotnet dotnet-ef` (ferramenta local) em vez de um `dotnet ef` global de versão anterior, que
> não consegue carregar o EF Core 10.

## Endpoints da API

| Método   | Rota                                 | Descrição                                       |
|----------|--------------------------------------|-------------------------------------------------|
| `GET`    | `/api/clientes`                      | Lista os clientes (RF-09)                       |
| `GET`    | `/api/clientes/{id}`                 | Consulta cliente por ID (RF-01, RF-04)          |
| `POST`   | `/api/clientes`                      | Cadastra cliente (RF-05, RF-06)                 |
| `POST`   | `/api/clientes/{id}/ativar`          | Ativa cliente (RF-02)                           |
| `POST`   | `/api/clientes/{id}/desativar`       | Desativa cliente (RF-03)                        |
| `DELETE` | `/api/clientes/{id}`                 | Remove cliente; a conta permanece (RF-07)       |
| `DELETE` | `/api/clientes`                      | Remove todos os clientes (RF-08)                |
| `GET`    | `/api/contas`                        | Lista as contas com o titular (RF-14)           |
| `GET`    | `/api/contas/{id}`                   | Consulta conta por ID (RF-10, RF-12)            |
| `POST`   | `/api/contas`                        | Cadastra conta com saldo inicial (RF-11)        |
| `POST`   | `/api/contas/{id}/ativar`            | Ativa conta                                     |
| `POST`   | `/api/contas/{id}/desativar`         | Desativa conta                                  |
| `DELETE` | `/api/contas/{id}`                   | Remove conta sem titular (RF-13)                |
| `POST`   | `/api/transferencias`                | Transfere valor entre contas (RF-15)            |

Exemplo de transferência:

```http
POST http://localhost:5285/api/transferencias
Content-Type: application/json

{
  "contaOrigemId": 1,
  "contaDestinoId": 2,
  "valor": 150.00
}
```
