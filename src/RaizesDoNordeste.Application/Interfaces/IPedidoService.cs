using RaizesDoNordeste.Application.DTOs;

namespace RaizesDoNordeste.Application.Interfaces;

public interface IPedidoService
{
    Task<OperacaoResultado<PedidoView>> CriarAsync(
        CriarPedidoDto request, int usuarioId, string role);

    Task<OperacaoResultado<PedidoView>> ObterAsync(
        int id, int usuarioId, string role);

    Task<OperacaoResultado<IReadOnlyList<PedidoView>>> ListarAsync(
        string? canal, string? status, int usuarioId, string role);

    Task<OperacaoResultado<PedidoView>> PagarAsync(
        int id, PagamentoMockDto request, int usuarioId, string role);

    Task<OperacaoResultado<PedidoView>> AlterarStatusAsync(
        int id, AlterarStatusDto request, int usuarioId);
}
