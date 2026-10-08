# Diagramas técnicos da Raízes do Nordeste

Estes diagramas documentam o **modelo conceitual e o fluxo funcional** implementado. Para documentação física do banco, conferir `AppDbContext` e as migrations.

## 1. Arquitetura em camadas

```mermaid
flowchart TD
    API["API: Controllers, JWT, Swagger"]
    APP["Application: DTOs e interfaces"]
    INFRA["Infrastructure: serviços e EF Core"]
    DOMAIN["Domain: entidades e enums"]
    DB[(SQLite)]
    API --> APP
    API --> INFRA
    INFRA --> APP
    APP --> DOMAIN
    INFRA --> DOMAIN
    INFRA --> DB
```

## 2. Diagrama conceitual de entidades e relacionamentos

```mermaid
erDiagram
    USUARIO ||--o| CLIENTE : possui
    USUARIO ||--o{ AUDITORIA : realiza
    CLIENTE |o--o{ PEDIDO : realiza
    CLIENTE ||--o| FIDELIDADE : possui
    FIDELIDADE ||--o{ MOVIMENTACAO_FIDELIDADE : registra
    UNIDADE ||--o{ PEDIDO : recebe
    UNIDADE ||--o{ ESTOQUE : controla
    PRODUTO ||--o{ ESTOQUE : consta
    PEDIDO ||--o{ ITEM_PEDIDO : contem
    PRODUTO ||--o{ ITEM_PEDIDO : integra
    PEDIDO ||--o{ PAGAMENTO : recebe

    USUARIO {
      int Id PK
      string Nome
      string Email
      string SenhaHash
      string Role
    }
    CLIENTE {
      int Id PK
      int UsuarioId FK
      bool ConsentimentoFidelidade
    }
    UNIDADE {
      int Id PK
      string Nome
      string Cidade
      string Estado
    }
    PRODUTO {
      int Id PK
      string Nome
      decimal Preco
    }
    ESTOQUE {
      int Id PK
      int UnidadeId FK
      int ProdutoId FK
      int Quantidade
    }
    PEDIDO {
      int Id PK
      int ClienteId FK
      int UnidadeId FK
      string CanalPedido
      string Status
      decimal ValorTotal
    }
    ITEM_PEDIDO {
      int Id PK
      int PedidoId FK
      int ProdutoId FK
      int Quantidade
      decimal PrecoUnitario
    }
    PAGAMENTO {
      int Id PK
      int PedidoId FK
      decimal Valor
      string Status
      string CodigoTransacao
    }
    FIDELIDADE {
      int Id PK
      int ClienteId FK
      int SaldoPontos
    }
    MOVIMENTACAO_FIDELIDADE {
      int Id PK
      int FidelidadeId FK
    }
    AUDITORIA {
      int Id PK
      int UsuarioId FK
      string Acao
      string Entidade
      string EntidadeId
    }
```

## 3. Diagrama simplificado de classes de domínio

```mermaid
classDiagram
    class Usuario {
        +int Id
        +string Nome
        +string Email
        +string SenhaHash
        +RoleUsuario Role
    }
    class Cliente {
        +int Id
        +int UsuarioId
        +bool ConsentimentoFidelidade
    }
    class Unidade {
        +int Id
        +string Nome
    }
    class Produto {
        +int Id
        +string Nome
        +decimal Preco
    }
    class Estoque {
        +int Quantidade
        +Baixar()
    }
    class Pedido {
        +int Id
        +CanalPedido CanalPedido
        +StatusPedido Status
        +decimal ValorTotal
        +RecalcularValorTotal()
        +AtualizarStatus()
    }
    class ItemPedido {
        +int ProdutoId
        +int Quantidade
        +decimal PrecoUnitario
        +decimal Subtotal
    }
    class Pagamento {
        +decimal Valor
        +StatusPagamento Status
    }
    Usuario --> Cliente
    Cliente --> Pedido
    Unidade "1" --> "*" Pedido
    Unidade "1" --> "*" Estoque
    Produto "1" --> "*" Estoque
    Pedido "1" --> "*" ItemPedido
    Produto "1" --> "*" ItemPedido
    Pedido "1" --> "*" Pagamento
```

## 4. Fluxo de pedido e pagamento mock

```mermaid
flowchart TD
    A["Receber pedido"] --> B{"Itens, canal e unidade válidos?"}
    B -- Não --> C["Erro 400 ou 404"]
    B -- Sim --> D{"Estoque disponível?"}
    D -- Não --> E["Erro 409"]
    D -- Sim --> F["AguardandoPagamento"]
    F --> G{"Mock aprovou?"}
    G -- Não --> H["PagamentoRecusado"]
    H --> G
    G -- Sim --> I{"Baixa condicional possível?"}
    I -- Não --> E
    I -- Sim --> J["Confirmado"]
    J --> K["EmPreparo"]
    K --> L["Pronto"]
    L --> M["Entregue"]
```

## 5. Atores e casos de uso

O diagrama UML de casos de uso está disponível em `casos-de-uso.puml`. Pode ser renderizado com PlantUML para o relatório acadêmico.
