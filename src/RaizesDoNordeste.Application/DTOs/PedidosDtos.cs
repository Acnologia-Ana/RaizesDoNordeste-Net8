namespace RaizesDoNordeste.Application.DTOs;

public record NovoItemPedidoDto(int ProdutoId, int Quantidade);

public record CriarPedidoDto(
    int UnidadeId,
    string CanalPedido,
    List<NovoItemPedidoDto> Itens);

public record PagamentoMockDto(bool Aprovar);

public record AlterarStatusDto(string Status);

public record ItemPedidoView(
    int ProdutoId,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);

public record PagamentoView(
    int Id,
    string Status,
    decimal Valor,
    string? CodigoTransacao,
    DateTime CriadoEm);

public record PedidoView(
    int Id,
    int? ClienteId,
    int UnidadeId,
    string CanalPedido,
    string Status,
    decimal ValorTotal,
    DateTime CriadoEm,
    List<ItemPedidoView> Itens,
    List<PagamentoView> Pagamentos);

public record OperacaoResultado<T>(
    T? Dados,
    int Codigo,
    string? Erro);
