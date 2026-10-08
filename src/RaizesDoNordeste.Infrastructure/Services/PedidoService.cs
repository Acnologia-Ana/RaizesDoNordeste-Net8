using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.DTOs;
using RaizesDoNordeste.Application.Interfaces;
using RaizesDoNordeste.Domain.Entities;
using RaizesDoNordeste.Domain.Enums;
using RaizesDoNordeste.Infrastructure.Persistence;

namespace RaizesDoNordeste.Infrastructure.Services;

public class PedidoService : IPedidoService
{
    private readonly AppDbContext _db;

    public PedidoService(AppDbContext db)
    {
        _db = db;
    }

    private static OperacaoResultado<T> Falha<T>(
        int codigo, string mensagem) =>
        new(default, codigo, mensagem);

    private static PedidoView Mapear(Pedido p) =>
        new(
            p.Id,
            p.ClienteId,
            p.UnidadeId,
            p.CanalPedido.ToString(),
            p.Status.ToString(),
            p.ValorTotal,
            p.CriadoEm,
            p.Itens.Select(i => new ItemPedidoView(
                i.ProdutoId,
                i.Quantidade,
                i.PrecoUnitario,
                i.Subtotal)).ToList(),
            p.Pagamentos.OrderBy(x => x.Id)
                .Select(x => new PagamentoView(
                    x.Id,
                    x.Status.ToString(),
                    x.Valor,
                    x.CodigoTransacao,
                    x.CriadoEm)).ToList()
        );

    private IQueryable<Pedido> ConsultaCompleta() =>
        _db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .Include(p => p.Pagamentos)
            .AsSplitQuery();

    private void Auditar(
        int usuarioId, string acao, int pedidoId)
    {
        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = usuarioId,
            Acao = acao,
            Entidade = "Pedido",
            EntidadeId = pedidoId.ToString(),
            CriadoEm = DateTime.UtcNow
        });
    }

    public async Task<OperacaoResultado<PedidoView>> CriarAsync(
        CriarPedidoDto request, int usuarioId, string role)
    {
        if (request is null ||
            request.UnidadeId <= 0 ||
            request.Itens is null ||
            request.Itens.Count == 0 ||
            request.Itens.Count > 20)
            return Falha<PedidoView>(
                400, "Pedido deve conter de 1 a 20 itens.");

        if (!Enum.TryParse<CanalPedido>(
                request.CanalPedido, true, out var canal) ||
            !Enum.IsDefined(canal))
            return Falha<PedidoView>(
                400, "Canal de pedido invalido.");

        if (request.Itens.Any(x =>
            x.ProdutoId <= 0 ||
            x.Quantidade <= 0 ||
            x.Quantidade > 100))
            return Falha<PedidoView>(
                400, "Produto ou quantidade invalida.");

        var itens = request.Itens
            .GroupBy(x => x.ProdutoId)
            .Select(g => new
            {
                ProdutoId = g.Key,
                Quantidade = g.Sum(x => x.Quantidade)
            })
            .ToList();

        if (itens.Any(x => x.Quantidade > 100))
            return Falha<PedidoView>(
                400, "Quantidade maxima por produto: 100.");

        var unidade = await _db.Unidades
            .AnyAsync(x => x.Id == request.UnidadeId && x.Ativa);

        if (!unidade)
            return Falha<PedidoView>(
                404, "Unidade inexistente ou inativa.");

        int? clienteId = null;

        if (role == "Cliente")
        {
            var cliente = await _db.Clientes
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (cliente is null)
                return Falha<PedidoView>(
                    403, "Cadastro de cliente nao encontrado.");

            clienteId = cliente.Id;
        }

        var ids = itens.Select(x => x.ProdutoId).ToArray();

        var produtos = await _db.Produtos
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.Ativo)
            .ToListAsync();

        if (produtos.Count != itens.Count)
            return Falha<PedidoView>(
                400, "Produto inexistente ou inativo.");

        var estoques = await _db.Estoques
            .AsNoTracking()
            .Where(x => x.UnidadeId == request.UnidadeId &&
                        ids.Contains(x.ProdutoId))
            .ToListAsync();

        foreach (var item in itens)
        {
            var estoque = estoques.FirstOrDefault(
                x => x.ProdutoId == item.ProdutoId);

            if (estoque is null ||
                estoque.Quantidade < item.Quantidade)
                return Falha<PedidoView>(
                    409, $"Estoque insuficiente para produto {item.ProdutoId}.");
        }

        var pedido = new Pedido
        {
            UnidadeId = request.UnidadeId,
            ClienteId = clienteId,
            CanalPedido = canal,
            Status = StatusPedido.AguardandoPagamento,
            CriadoEm = DateTime.UtcNow
        };

        foreach (var item in itens)
        {
            var produto = produtos.Single(
                x => x.Id == item.ProdutoId);

            pedido.Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = item.Quantidade,
                PrecoUnitario = produto.Preco
            });
        }

        pedido.RecalcularValorTotal();

        if (pedido.ValorTotal <= 0)
            return Falha<PedidoView>(
                400, "Valor do pedido deve ser positivo.");

        _db.Pedidos.Add(pedido);

        await _db.SaveChangesAsync();

        Auditar(usuarioId, "PedidoCriado", pedido.Id);
        await _db.SaveChangesAsync();

        return new(Mapear(pedido), 201, null);
    }

    public async Task<OperacaoResultado<PedidoView>> ObterAsync(
        int id, int usuarioId, string role)
    {
        var pedido = await ConsultaCompleta()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pedido is null)
            return Falha<PedidoView>(404, "Pedido nao encontrado.");

        if (role == "Cliente" &&
            pedido.Cliente?.UsuarioId != usuarioId)
            return Falha<PedidoView>(403, "Acesso negado.");

        return new(Mapear(pedido), 200, null);
    }

    public async Task<OperacaoResultado<IReadOnlyList<PedidoView>>>
        ListarAsync(
            string? canal, string? status, int usuarioId, string role)
    {
        var query = ConsultaCompleta().AsNoTracking();

        if (role == "Cliente")
            query = query.Where(x =>
                x.Cliente != null &&
                x.Cliente.UsuarioId == usuarioId);

        if (!string.IsNullOrWhiteSpace(canal))
        {
            if (!Enum.TryParse<CanalPedido>(
                    canal, true, out var canalEnum) ||
                !Enum.IsDefined(canalEnum))
                return Falha<IReadOnlyList<PedidoView>>(
                    400, "Canal invalido.");

            query = query.Where(x => x.CanalPedido == canalEnum);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusPedido>(
                    status, true, out var statusEnum) ||
                !Enum.IsDefined(statusEnum))
                return Falha<IReadOnlyList<PedidoView>>(
                    400, "Status invalido.");

            query = query.Where(x => x.Status == statusEnum);
        }

        var pedidos = await query
            .OrderByDescending(x => x.CriadoEm)
            .Take(100)
            .ToListAsync();

        return new(
            pedidos.Select(Mapear).ToList(),
            200,
            null);
    }

    public async Task<OperacaoResultado<PedidoView>> PagarAsync(
        int id, PagamentoMockDto request,
        int usuarioId, string role)
    {
        await using var transacao =
            await _db.Database.BeginTransactionAsync();

        var pedido = await ConsultaCompleta()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pedido is null)
            return Falha<PedidoView>(404, "Pedido nao encontrado.");

        if (role == "Cliente" &&
            pedido.Cliente?.UsuarioId != usuarioId)
            return Falha<PedidoView>(403, "Acesso negado.");

        if (pedido.Status != StatusPedido.AguardandoPagamento &&
            pedido.Status != StatusPedido.PagamentoRecusado)
            return Falha<PedidoView>(
                409, "Pedido nao permite novo pagamento.");

        if (request.Aprovar)
        {
            foreach (var item in pedido.Itens)
            {
                // Baixa condicional impede estoque negativo.
                var alterados = await _db.Estoques
                    .Where(x =>
                        x.UnidadeId == pedido.UnidadeId &&
                        x.ProdutoId == item.ProdutoId &&
                        x.Quantidade >= item.Quantidade)
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(
                            x => x.Quantidade,
                            x => x.Quantidade - item.Quantidade)
                        .SetProperty(
                            x => x.AtualizadoEm,
                            DateTime.UtcNow));

                if (alterados != 1)
                {
                    await transacao.RollbackAsync();
                    return Falha<PedidoView>(
                        409, "Estoque insuficiente no pagamento.");
                }
            }
        }

        var pagamento = new Pagamento
        {
            PedidoId = pedido.Id,
            Valor = pedido.ValorTotal,
            Status = request.Aprovar
                ? StatusPagamento.Aprovado
                : StatusPagamento.Recusado,
            CodigoTransacao = Guid.NewGuid().ToString("N"),
            MensagemRetorno = request.Aprovar
                ? "Pagamento mock aprovado."
                : "Pagamento mock recusado.",
            CriadoEm = DateTime.UtcNow
        };

        pedido.Pagamentos.Add(pagamento);

        pedido.AtualizarStatus(request.Aprovar
            ? StatusPedido.Confirmado
            : StatusPedido.PagamentoRecusado);

        Auditar(
            usuarioId,
            request.Aprovar
                ? "PagamentoAprovado"
                : "PagamentoRecusado",
            pedido.Id);

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        return new(Mapear(pedido), 200, null);
    }

    public async Task<OperacaoResultado<PedidoView>>
        AlterarStatusAsync(
            int id, AlterarStatusDto request, int usuarioId)
    {
        if (!Enum.TryParse<StatusPedido>(
                request.Status, true, out var novo) ||
            !Enum.IsDefined(novo))
            return Falha<PedidoView>(400, "Status invalido.");

        var pedido = await ConsultaCompleta()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pedido is null)
            return Falha<PedidoView>(404, "Pedido nao encontrado.");

        var atual = pedido.Status;

        bool permitido =
            (atual == StatusPedido.Confirmado &&
                novo == StatusPedido.EmPreparo) ||
            (atual == StatusPedido.EmPreparo &&
                novo == StatusPedido.Pronto) ||
            (atual == StatusPedido.Pronto &&
                novo == StatusPedido.Entregue) ||
            ((atual == StatusPedido.AguardandoPagamento ||
              atual == StatusPedido.PagamentoRecusado) &&
                novo == StatusPedido.Cancelado);

        if (!permitido)
            return Falha<PedidoView>(
                409, "Transicao de status nao permitida.");

        pedido.AtualizarStatus(novo);
        Auditar(usuarioId, "StatusAlterado", pedido.Id);

        await _db.SaveChangesAsync();

        return new(Mapear(pedido), 200, null);
    }
}
