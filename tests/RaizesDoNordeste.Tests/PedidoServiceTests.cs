using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.DTOs;
using RaizesDoNordeste.Domain.Entities;
using RaizesDoNordeste.Domain.Enums;
using RaizesDoNordeste.Infrastructure.Persistence;
using RaizesDoNordeste.Infrastructure.Services;
using Xunit;

namespace RaizesDoNordeste.Tests;

public class PedidoServiceTests
{
    [Fact]
    public async Task CriarPedidoValidoRetorna201()
    {
        await using var a = await Ambiente.CriarAsync();

        var resultado = await a.Service.CriarAsync(
            a.PedidoPadrao(), a.UsuarioId, "Admin");

        Assert.Equal(201, resultado.Codigo);
        Assert.NotNull(resultado.Dados);
        Assert.Equal("AguardandoPagamento", resultado.Dados.Status);
        Assert.Equal(52.30m, resultado.Dados.ValorTotal);
        Assert.Equal(2, resultado.Dados.Itens.Count);
    }

    [Fact]
    public async Task ConsultarPedidoExistenteRetorna200()
    {
        await using var a = await Ambiente.CriarAsync();
        var id = await a.CriarPedidoAsync();

        var resultado = await a.Service.ObterAsync(
            id, a.UsuarioId, "Admin");

        Assert.Equal(200, resultado.Codigo);
        Assert.Equal(id, resultado.Dados!.Id);
    }

    [Fact]
    public async Task ListarPedidosPermiteFiltrarCanalEStatus()
    {
        await using var a = await Ambiente.CriarAsync();

        await a.CriarPedidoAsync();

        var resultado = await a.Service.ListarAsync(
            "Web", "AguardandoPagamento",
            a.UsuarioId, "Admin");

        Assert.Equal(200, resultado.Codigo);
        Assert.Single(resultado.Dados!);
        Assert.Equal("Web", resultado.Dados![0].CanalPedido);
    }

    [Fact]
    public async Task PagamentoRecusadoPreservaEstoque()
    {
        await using var a = await Ambiente.CriarAsync();
        var id = await a.CriarPedidoAsync();

        var pagamento = await a.Service.PagarAsync(
            id, new PagamentoMockDto(false),
            a.UsuarioId, "Admin");

        Assert.Equal(200, pagamento.Codigo);
        Assert.Equal("PagamentoRecusado", pagamento.Dados!.Status);
        Assert.Equal(30, await a.EstoqueAsync(a.Produto1Id));
        Assert.Equal(40, await a.EstoqueAsync(a.Produto2Id));
    }

    [Fact]
    public async Task PagamentoAprovadoBaixaEstoque()
    {
        await using var a = await Ambiente.CriarAsync();
        var id = await a.CriarPedidoAsync();

        var pagamento = await a.Service.PagarAsync(
            id, new PagamentoMockDto(true),
            a.UsuarioId, "Admin");

        Assert.Equal(200, pagamento.Codigo);
        Assert.Equal("Confirmado", pagamento.Dados!.Status);
        Assert.Equal(28, await a.EstoqueAsync(a.Produto1Id));
        Assert.Equal(39, await a.EstoqueAsync(a.Produto2Id));
    }

    [Fact]
    public async Task PagamentoRecusadoPermiteNovaTentativa()
    {
        await using var a = await Ambiente.CriarAsync();
        var id = await a.CriarPedidoAsync();

        var recusa = await a.Service.PagarAsync(
            id, new PagamentoMockDto(false),
            a.UsuarioId, "Admin");

        Assert.Equal(200, recusa.Codigo);

        var aprovacao = await a.Service.PagarAsync(
            id, new PagamentoMockDto(true),
            a.UsuarioId, "Admin");

        Assert.Equal(200, aprovacao.Codigo);
        Assert.Equal("Confirmado", aprovacao.Dados!.Status);
        Assert.Equal(2, aprovacao.Dados.Pagamentos.Count);
    }

    [Fact]
    public async Task PedidoPodeAvancarAteEntregue()
    {
        await using var a = await Ambiente.CriarAsync();
        var id = await a.CriarPedidoAsync();

        var pagamento = await a.Service.PagarAsync(
            id, new PagamentoMockDto(true),
            a.UsuarioId, "Admin");

        Assert.Equal(200, pagamento.Codigo);

        foreach (var status in new[]
        {
            "EmPreparo", "Pronto", "Entregue"
        })
        {
            var resultado = await a.Service.AlterarStatusAsync(
                id, new AlterarStatusDto(status), a.UsuarioId);

            Assert.Equal(200, resultado.Codigo);
            Assert.Equal(status, resultado.Dados!.Status);
        }
    }

    [Fact]
    public async Task CanalInvalidoRetorna400()
    {
        await using var a = await Ambiente.CriarAsync();

        var pedido = new CriarPedidoDto(
            a.UnidadeId,
            "CanalInexistente",
            new List<NovoItemPedidoDto>
            {
                new(a.Produto1Id, 1)
            });

        var resultado = await a.Service.CriarAsync(
            pedido, a.UsuarioId, "Admin");

        Assert.Equal(400, resultado.Codigo);
    }

    [Fact]
    public async Task PedidoSemItensRetorna400()
    {
        await using var a = await Ambiente.CriarAsync();

        var pedido = new CriarPedidoDto(
            a.UnidadeId,
            "Web",
            new List<NovoItemPedidoDto>());

        var resultado = await a.Service.CriarAsync(
            pedido, a.UsuarioId, "Admin");

        Assert.Equal(400, resultado.Codigo);
    }

    [Fact]
    public async Task QuantidadeZeroRetorna400()
    {
        await using var a = await Ambiente.CriarAsync();

        var pedido = new CriarPedidoDto(
            a.UnidadeId,
            "Web",
            new List<NovoItemPedidoDto>
            {
                new(a.Produto1Id, 0)
            });

        var resultado = await a.Service.CriarAsync(
            pedido, a.UsuarioId, "Admin");

        Assert.Equal(400, resultado.Codigo);
    }

    [Fact]
    public async Task UnidadeInexistenteRetorna404()
    {
        await using var a = await Ambiente.CriarAsync();

        var pedido = new CriarPedidoDto(
            999999,
            "Web",
            new List<NovoItemPedidoDto>
            {
                new(a.Produto1Id, 1)
            });

        var resultado = await a.Service.CriarAsync(
            pedido, a.UsuarioId, "Admin");

        Assert.Equal(404, resultado.Codigo);
    }

    [Fact]
    public async Task EstoqueInsuficienteRetorna409()
    {
        await using var a = await Ambiente.CriarAsync();

        var pedido = new CriarPedidoDto(
            a.UnidadeId,
            "Web",
            new List<NovoItemPedidoDto>
            {
                new(a.Produto1Id, 100)
            });

        var resultado = await a.Service.CriarAsync(
            pedido, a.UsuarioId, "Admin");

        Assert.Equal(409, resultado.Codigo);
    }

    [Fact]
    public async Task PedidoInexistenteRetorna404()
    {
        await using var a = await Ambiente.CriarAsync();

        var resultado = await a.Service.ObterAsync(
            999999, a.UsuarioId, "Admin");

        Assert.Equal(404, resultado.Codigo);
    }

    [Fact]
    public async Task StatusAntesDoPagamentoRetorna409()
    {
        await using var a = await Ambiente.CriarAsync();
        var id = await a.CriarPedidoAsync();

        var resultado = await a.Service.AlterarStatusAsync(
            id, new AlterarStatusDto("EmPreparo"),
            a.UsuarioId);

        Assert.Equal(409, resultado.Codigo);
    }

    [Fact]
    public async Task PagamentoDuplicadoRetorna409()
    {
        await using var a = await Ambiente.CriarAsync();
        var id = await a.CriarPedidoAsync();

        var primeiro = await a.Service.PagarAsync(
            id, new PagamentoMockDto(true),
            a.UsuarioId, "Admin");

        Assert.Equal(200, primeiro.Codigo);

        var segundo = await a.Service.PagarAsync(
            id, new PagamentoMockDto(true),
            a.UsuarioId, "Admin");

        Assert.Equal(409, segundo.Codigo);
        Assert.Equal(28, await a.EstoqueAsync(a.Produto1Id));
    }

    [Fact]
    public async Task FiltroDeStatusInvalidoRetorna400()
    {
        await using var a = await Ambiente.CriarAsync();

        var resultado = await a.Service.ListarAsync(
            null, "StatusInventado",
            a.UsuarioId, "Admin");

        Assert.Equal(400, resultado.Codigo);
    }

    private sealed class Ambiente : IAsyncDisposable
    {
        private readonly SqliteConnection _conexao;

        public AppDbContext Db { get; }
        public PedidoService Service { get; }

        public int UsuarioId { get; }
        public int UnidadeId { get; }
        public int Produto1Id { get; }
        public int Produto2Id { get; }

        private Ambiente(
            SqliteConnection conexao,
            AppDbContext db,
            int usuarioId,
            int unidadeId,
            int produto1Id,
            int produto2Id)
        {
            _conexao = conexao;
            Db = db;
            Service = new PedidoService(db);
            UsuarioId = usuarioId;
            UnidadeId = unidadeId;
            Produto1Id = produto1Id;
            Produto2Id = produto2Id;
        }

        public static async Task<Ambiente> CriarAsync()
        {
            var conexao = new SqliteConnection(
                "Data Source=:memory:");

            await conexao.OpenAsync();

            var opcoes =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(conexao)
                    .Options;

            var db = new AppDbContext(opcoes);

            await db.Database.EnsureCreatedAsync();

            var usuario = new Usuario
            {
                Nome = "Admin Testes",
                Email = "admin-testes@raizes.local",
                SenhaHash = "HashSomenteParaTestes",
                Role = RoleUsuario.Admin,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            var unidade = new Unidade
            {
                Nome = "Recife Testes",
                Cidade = "Recife",
                Estado = "PE",
                Ativa = true
            };

            var produto1 = new Produto
            {
                Nome = "Tapioca",
                Preco = 18.90m,
                Ativo = true
            };

            var produto2 = new Produto
            {
                Nome = "Cuscuz",
                Preco = 14.50m,
                Ativo = true
            };

            db.Usuarios.Add(usuario);
            db.Unidades.Add(unidade);
            db.Produtos.AddRange(produto1, produto2);

            await db.SaveChangesAsync();

            db.Estoques.AddRange(
                new Estoque
                {
                    UnidadeId = unidade.Id,
                    ProdutoId = produto1.Id,
                    Quantidade = 30
                },
                new Estoque
                {
                    UnidadeId = unidade.Id,
                    ProdutoId = produto2.Id,
                    Quantidade = 40
                });

            await db.SaveChangesAsync();

            return new Ambiente(
                conexao,
                db,
                usuario.Id,
                unidade.Id,
                produto1.Id,
                produto2.Id);
        }

        public CriarPedidoDto PedidoPadrao() =>
            new(
                UnidadeId,
                "Web",
                new List<NovoItemPedidoDto>
                {
                    new(Produto1Id, 2),
                    new(Produto2Id, 1)
                });

        public async Task<int> CriarPedidoAsync()
        {
            var resultado = await Service.CriarAsync(
                PedidoPadrao(), UsuarioId, "Admin");

            Assert.Equal(201, resultado.Codigo);
            Assert.NotNull(resultado.Dados);

            return resultado.Dados.Id;
        }

        public async Task<int> EstoqueAsync(int produtoId)
        {
            var estoque = await Db.Estoques
                .AsNoTracking()
                .SingleAsync(x =>
                    x.UnidadeId == UnidadeId &&
                    x.ProdutoId == produtoId);

            return estoque.Quantidade;
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _conexao.DisposeAsync();
        }
    }
}
