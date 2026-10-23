# Requisitos Funcionais — Sistema Bancário

Requisitos funcionais extraídos diretamente do código-fonte da aplicação (`src/negocio/`), no estado
atual do repositório. Cada requisito indica onde está implementado e por qual canal fica acessível:

- **Menu** — exposto ao usuário pelo menu de console (`SistemaBancario.montaOpcoes`);
- **Negócio** — disponível apenas na camada de negócio (`GerenciadoraClientes` /
  `GerenciadoraContas`), sem opção correspondente no menu.

A coluna **HU** relaciona o requisito à história de usuário documentada em
`tarefa1-historias-de-usuario.md`.

---

## 1. Visão geral

| ID    | Requisito                                         | Canal   | HU    |
|-------|---------------------------------------------------|---------|-------|
| RF-01 | Consultar cliente por ID                          | Menu    | HU-01 |
| RF-02 | Ativar cliente                                    | Menu    | HU-02 |
| RF-03 | Desativar cliente                                 | Menu    | HU-03 |
| RF-04 | Verificar se um cliente está ativo                | Negócio | HU-04 |
| RF-05 | Cadastrar cliente                                 | Negócio | HU-05 |
| RF-06 | Validar idade do cliente                          | Negócio | HU-06 |
| RF-07 | Remover cliente                                   | Negócio | HU-07 |
| RF-08 | Limpar a base de clientes                         | Negócio | HU-08 |
| RF-09 | Listar clientes do banco                          | Negócio | —     |
| RF-10 | Consultar conta corrente por ID                   | Menu    | HU-09 |
| RF-11 | Cadastrar conta corrente                          | Negócio | HU-10 |
| RF-12 | Verificar se uma conta está ativa                 | Negócio | HU-11 |
| RF-13 | Remover conta corrente                            | Negócio | HU-12 |
| RF-14 | Listar contas do banco                            | Negócio | —     |
| RF-15 | Transferir valor entre contas                     | Negócio | HU-13 |
| RF-16 | Navegar pelo menu e encerrar o sistema            | Menu    | HU-14 |
| RF-17 | Tratar entrada numérica inválida                  | Menu    | HU-14 |
| RF-18 | Inicializar o sistema com dados pré-cadastrados   | Menu    | HU-15 |

---

## 2. Dados do domínio

**Cliente** (`Cliente.java`): `id` (int), `nome` (String), `idade` (int), `email` (String),
`ativo` (boolean) e `idContaCorrente` (int). O cliente referencia sua conta corrente **apenas pelo
ID**, não pelo objeto.

**Conta corrente** (`ContaCorrente.java`): `id` (int), `saldo` (double) e `ativa` (boolean).

---

## 3. Gestão de Clientes

### RF-01 — Consultar cliente por ID

- **Implementação:** `GerenciadoraClientes.pesquisaCliente(int)`; menu opção **1**.
- **Entrada:** ID do cliente.
- **Comportamento:**
  - Percorre a lista de clientes e retorna o primeiro cujo ID coincide com o informado.
  - Se não encontrar, retorna `null` (não lança exceção, inclusive com a base vazia).
  - O status do cliente não filtra a busca: clientes inativos também são encontrados.
- **Saída (menu):** os dados do cliente — Id, Nome, Email, Idade e Status (`Ativo`/`Inativo`) —
  entre linhas separadoras `=========================`; caso não exista, a mensagem
  `Cliente não encontrado!`.

### RF-02 — Ativar cliente

- **Implementação:** `SistemaBancario.montaOpcoes`, menu opção **3** (via `Cliente.setAtivo(true)`).
- **Entrada:** ID do cliente.
- **Comportamento:**
  - Se o cliente existir, seu status passa a ativo e é exibido `Cliente ativado com sucesso!`.
  - Se não existir, nenhum cliente é alterado e é exibido `Cliente não encontrado!`.
  - Apenas o cliente indicado é alterado.

### RF-03 — Desativar cliente

- **Implementação:** `SistemaBancario.montaOpcoes`, menu opção **4** (via `Cliente.setAtivo(false)`).
- **Entrada:** ID do cliente.
- **Comportamento:**
  - Se o cliente existir, seu status passa a inativo e é exibido `Cliente desativado com sucesso!`.
  - Se não existir, nenhum cliente é alterado e é exibido `Cliente não encontrado!`.

### RF-04 — Verificar se um cliente está ativo

- **Implementação:** `GerenciadoraClientes.clienteAtivo(int)`.
- **Entrada:** ID do cliente.
- **Saída:** `true` somente se o cliente existir **e** estiver ativo; `false` se estiver inativo
  ou se o ID não existir na base.

### RF-05 — Cadastrar cliente

- **Implementação:** `GerenciadoraClientes.adicionaCliente(Cliente)`.
- **Comportamento:** adiciona o cliente ao final da lista de clientes do banco.
- **Observação:** não há validação no cadastro — nem de ID duplicado, nem de idade (RF-06 é uma
  operação separada), nem de existência da conta referenciada por `idContaCorrente`.

### RF-06 — Validar idade do cliente

- **Implementação:** `GerenciadoraClientes.validaIdade(int)`.
- **Regra:** a idade permitida é de **18 a 65 anos, inclusive**.
- **Saída:**
  - Retorna `true` para idades dentro do intervalo.
  - Lança `IdadeNaoPermitidaException` (exceção verificada) com a mensagem
    `A idade do cliente precisa estar entre 18 e 65 anos.` para idades menores que 18 ou maiores
    que 65.
- **Valores-limite:** 17 (rejeita), 18 (aceita), 65 (aceita), 66 (rejeita).

### RF-07 — Remover cliente

- **Implementação:** `GerenciadoraClientes.removeCliente(int)`.
- **Entrada:** ID do cliente.
- **Comportamento:** remove o primeiro cliente com o ID informado e retorna `true`; se o ID não
  existir, a lista permanece inalterada e o retorno é `false`.
- **Observação:** a conta corrente associada ao cliente não é removida.

### RF-08 — Limpar a base de clientes

- **Implementação:** `GerenciadoraClientes.limpa()`.
- **Comportamento:** remove todos os clientes da lista. Como a lista é mantida por referência, a
  lista original passada ao construtor também é esvaziada.

### RF-09 — Listar clientes do banco

- **Implementação:** `GerenciadoraClientes.getClientesDoBanco()`.
- **Comportamento:** retorna a própria lista de clientes (sem cópia defensiva).

---

## 4. Gestão de Contas Correntes

### RF-10 — Consultar conta corrente por ID

- **Implementação:** `GerenciadoraContas.pesquisaConta(int)`; menu opção **2**.
- **Entrada:** ID da conta.
- **Comportamento:** retorna a conta com o ID informado ou `null` se não existir.
- **Saída (menu):** os dados da conta — Id, Saldo e Status (`Ativa`/`Inativa`) — entre linhas
  separadoras; caso não exista, a mensagem `Conta não encontrada!`.

### RF-11 — Cadastrar conta corrente

- **Implementação:** `GerenciadoraContas.adicionaConta(ContaCorrente)`.
- **Comportamento:** adiciona a conta ao final da lista de contas do banco, sem validações
  (ID duplicado, saldo inicial negativo etc.).

### RF-12 — Verificar se uma conta está ativa

- **Implementação:** `GerenciadoraContas.contaAtiva(int)`.
- **Saída:** `true` somente se a conta existir **e** estiver ativa; `false` caso contrário.

### RF-13 — Remover conta corrente

- **Implementação:** `GerenciadoraContas.removeConta(int)`.
- **Comportamento:** remove a primeira conta com o ID informado e retorna `true`; se o ID não
  existir, retorna `false`.
- **Observação:** clientes que referenciam a conta removida continuam com o mesmo
  `idContaCorrente`, ficando com referência órfã.

### RF-14 — Listar contas do banco

- **Implementação:** `GerenciadoraContas.getContasDoBanco()`.
- **Comportamento:** retorna a própria lista de contas (sem cópia defensiva).

---

## 5. Operações Financeiras

### RF-15 — Transferir valor entre contas

- **Implementação:** `GerenciadoraContas.transfereValor(int idContaOrigem, double valor, int idContaDestino)`.
- **Pré-condições para sucesso:**
  1. a conta de origem existe;
  2. a conta de destino existe;
  3. o saldo da conta de origem é **maior ou igual** ao valor (é permitido zerar o saldo).
- **Comportamento:**
  - Atendidas as pré-condições, debita o valor da origem, credita no destino e retorna `true`.
  - Caso contrário, nenhum saldo é alterado e o retorno é `false` (sem exceção, inclusive para IDs
    inexistentes).
- **Não validado pelo código:** valor zero ou negativo, situação ativa/inativa das contas e
  transferência de uma conta para ela mesma.

---

## 6. Operação do Sistema (console)

### RF-16 — Navegar pelo menu e encerrar o sistema

- **Implementação:** `SistemaBancario.montaOpcoes` / `printMenu`.
- **Comportamento:** exibe repetidamente o menu abaixo até que a opção 5 seja escolhida:

  ```
  1) Consultar por um cliente
  2) Consultar por uma conta corrente
  3) Ativar um cliente
  4) Desativar um cliente
  5) Sair
  ```

  - Opção **5** encerra o laço e exibe `################# Sistema encerrado #################`.
  - Qualquer outro número imprime linhas em branco e reapresenta o menu.

### RF-17 — Tratar entrada numérica inválida

- **Implementação:** `SistemaBancario.lerInteiro(Scanner)`.
- **Comportamento:** ao ler a opção do menu ou um ID, entradas não numéricas são descartadas e o
  sistema solicita `Digite um número: ` até receber um inteiro válido.

### RF-18 — Inicializar o sistema com dados pré-cadastrados

- **Implementação:** `SistemaBancario.inicializaSistemaBancario()`, chamado ao iniciar o menu.
- **Dados criados:**

  | Tipo    | ID | Dados                                                         | Situação |
  |---------|----|---------------------------------------------------------------|----------|
  | Conta   | 1  | saldo 0,00                                                    | Ativa    |
  | Conta   | 2  | saldo 0,00                                                    | Ativa    |
  | Cliente | 1  | Maria Silva, 31 anos, mariasilva@gmail.com, conta 1           | Ativo    |
  | Cliente | 2  | Felipe Augusto, 34 anos, felipeaugusto@gmail.com, conta 2     | Ativo    |

---

## 7. Divergências entre código e histórias de usuário

Pontos em que `tarefa1-historias-de-usuario.md` descreve comportamento que o código atual **não**
implementa — candidatos a falhas nos testes de aceitação:

- **HU-02 / HU-03 (idempotência):** as histórias esperam as mensagens `Cliente já ativo!` e
  `Cliente já inativo!`; o código sempre exibe a mensagem de sucesso, mesmo quando o status não muda.
- **Cadastro de cliente (RF-05):** a validação de idade (RF-06) não é aplicada automaticamente ao
  cadastrar; cabe ao chamador invocá-la antes.
- **Cadastro, remoção e transferência** não têm opção no menu de console — só são exercitáveis pela
  camada de negócio (testes unitários/BDD).
