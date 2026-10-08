# Raízes do Nordeste

API REST de uma rede fictícia de alimentação nordestina, desenvolvida como projeto acadêmico da trilha Back-End. O foco do protótipo é integrar catálogo, estoque, autenticação, pedidos multicanais e pagamento simulado em uma aplicação reproduzível.

## Tecnologias e estrutura

- C# e ASP.NET Core 8, com documentação Swagger/OpenAPI.
- Entity Framework Core 8 com SQLite e migrations.
- Autenticação JWT Bearer e hash de senha com `PasswordHasher<Usuario>`.
- xUnit e SQLite em memória para testes automatizados.
- Git para versionamento.

```text
src/
  RaizesDoNordeste.Domain/          Entidades e enumerações
  RaizesDoNordeste.Application/     DTOs e interfaces
  RaizesDoNordeste.Infrastructure/  Persistência, serviços e seed
  RaizesDoNordeste.Api/             Controllers, JWT e Swagger
tests/
  RaizesDoNordeste.Tests/          Testes xUnit
docs/                              Diagramas técnicos e casos de uso
postman/                           Coleção de requisições HTTP
```

## Funcionalidades implementadas

- Listagem de unidades e produtos, cardápio por unidade e consulta de estoque.
- Cadastro de clientes com consentimento opcional para fidelidade.
- Login com JWT e controle de acesso por perfil (`Cliente`, `Atendente`, `Cozinha`, `Gerente`, `Admin`).
- Registro de pedidos com origem `App`, `Totem`, `Balcao`, `Pickup` e `Web`.
- Consulta de pedidos e filtros por canal e status.
- Pagamento simulado aprovado ou recusado, com histórico de tentativas.
- Baixa condicional de estoque após aprovação, dentro de transação de banco.
- Evolução controlada: `Confirmado -> EmPreparo -> Pronto -> Entregue`.
- Registros de auditoria para criação, pagamento e atualização de status.

**Escopo:** pagamento mock não movimenta dinheiro. A entidade de fidelidade existe, mas operações completas de crédito/resgate de pontos não fazem parte do fluxo demonstrado.

## Como executar localmente

**Pré-requisitos:** .NET SDK 8 ou 9, PowerShell e, opcionalmente, Postman.

Na raiz do repositório:

```powershell
dotnet restore
dotnet build
```

A API usa User Secrets para a chave JWT e a senha inicial do administrador. Em uma instalação nova, configure-os antes de iniciar:

```powershell
$Api = 'src\RaizesDoNordeste.Api\RaizesDoNordeste.Api.csproj'
$Bytes = New-Object byte[] 64
$Rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$Rng.GetBytes($Bytes)
$Rng.Dispose()
$Chave = [Convert]::ToBase64String($Bytes)
dotnet user-secrets set 'Jwt:Key' $Chave --project $Api
dotnet user-secrets set 'AdminSeed:Email' 'admin@raizesdonordeste.local' --project $Api
$Cred = Get-Credential -UserName 'admin@raizesdonordeste.local' -Message 'Defina uma senha inicial para o Admin'
if ($null -eq $Cred) { throw 'Operacao cancelada.' }
dotnet user-secrets set 'AdminSeed:Password' $Cred.GetNetworkCredential().Password --project $Api
```

Para rodar a API:

```powershell
dotnet run --project 'src\RaizesDoNordeste.Api'
```

Com o perfil local padrão, acesse `http://localhost:5022/swagger`. A porta pode variar conforme `launchSettings.json`.

O projeto aplica migrations na inicialização e insere unidades, produtos e estoques de exemplo se ainda não existirem. Também cria o Admin inicial se o e-mail ainda não constar no banco. **O seed não altera a senha de um Admin já existente.** A conexão SQLite usa `Data Source=raizes.db` no diretório de execução da API.

## Endpoints

| Método | Rota | Autorização | Finalidade |
|---|---|---|---|
| GET | `/api/unidades` | Pública | Listar unidades |
| GET | `/api/unidades/{unidadeId}/cardapio` | Pública | Cardápio com itens disponíveis |
| GET | `/api/produtos` | Pública | Listar produtos |
| GET | `/api/estoques?unidadeId=1` | Gerente/Admin | Consultar estoque |
| POST | `/api/auth/registrar` | Pública | Cadastrar cliente |
| POST | `/api/auth/login` | Pública | Obter JWT |
| GET | `/api/auth/me` | Autenticada | Consultar identidade |
| POST | `/api/pedidos` | Cliente/Atendente/Gerente/Admin | Criar pedido |
| GET | `/api/pedidos` | Autenticada | Listar e filtrar pedidos |
| GET | `/api/pedidos/{id}` | Autenticada | Consultar pedido |
| POST | `/api/pedidos/{id}/pagamento/mock` | Cliente/Atendente/Gerente/Admin | Aprovar/recusar mock |
| PATCH | `/api/pedidos/{id}/status` | Cozinha/Atendente/Gerente/Admin | Evoluir status |

O cliente só acessa seus próprios pedidos; perfis internos têm acesso conforme autorização dos endpoints. No Swagger, faça login, copie `accessToken`, clique em **Authorize** e cole somente o token, sem escrever `Bearer` manualmente.

### Exemplo de criação de pedido

`POST /api/pedidos` com Bearer token:

```json
{
  "unidadeId": 1,
  "canalPedido": "Web",
  "itens": [
    {"produtoId": 1, "quantidade": 2},
    {"produtoId": 2, "quantidade": 1}
  ]
}
```

Com os preços iniciais, o total é R$ 52,30. O pedido começa em `AguardandoPagamento`.

Pagamento recusado: `POST /api/pedidos/{id}/pagamento/mock` com `{"aprovar":false}`. O estoque permanece inalterado e a tentativa fica registrada. Para aprovar uma nova tentativa: `{"aprovar":true}`. O pedido passa a `Confirmado` e o estoque é reduzido. O pagamento duplicado após confirmação é bloqueado.

### Fluxo de estados

`AguardandoPagamento -> PagamentoRecusado -> Confirmado -> EmPreparo -> Pronto -> Entregue`

Há também cancelamento permitido enquanto o pedido aguarda pagamento ou está recusado. Transições não autorizadas geram erro `409`.

## Testes e evidências

```powershell
dotnet test --logger 'console;verbosity=normal'
```

Na última execução registrada durante o desenvolvimento, **17 testes passaram**, dos quais 16 verificam o serviço de pedidos e 1 é o teste inicial de scaffold. Os testes de pedidos utilizam SQLite em memória e não afetam o banco real.

Também foi realizado um teste HTTP completo com `POST /api/pedidos`, pagamento recusado, nova tentativa aprovada, validação da baixa de estoque, bloqueio de pagamento duplicado e conclusão em `Entregue`.

A coleção `postman/RaizesDoNordeste.postman_collection.json` contém 18 requisições. Importe no Postman, defina `adminSenha` **como variável secreta em um ambiente local**, execute com a API rodando e nunca exporte credenciais. A coleção usa `pedidoId` e `token` capturados durante a execução.

## Diagramas

- `docs/diagramas.md`: arquitetura, modelo conceitual de entidades, classes e fluxo de pedidos em Mermaid.
- `docs/casos-de-uso.puml`: diagrama de casos de uso em PlantUML.

## Privacidade, segurança e limitações

O sistema é um **protótipo acadêmico**, não um sistema pronto para produção. São necessários HTTPS, política robusta de senhas, revisão de acesso e de LGPD, gestão de segredos fora da máquina do desenvolvedor, testes concorrentes, observabilidade e validações adicionais antes de uma implantação real.

Não publique banco `.db`, tokens JWT, senhas, informações pessoais reais nem exports do Postman com segredos. Os diagramas são conceituais e devem ser conferidos com o modelo EF Core/migrations antes de serem apresentados como DER físico.
