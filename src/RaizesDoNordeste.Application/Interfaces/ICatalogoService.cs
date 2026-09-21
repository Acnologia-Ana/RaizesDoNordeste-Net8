using RaizesDoNordeste.Application.DTOs;

namespace RaizesDoNordeste.Application.Interfaces;

public interface ICatalogoService
{
    Task<IReadOnlyList<UnidadeDto>> ListarUnidadesAsync();

    Task<IReadOnlyList<ProdutoDto>> ListarProdutosAsync();

    Task<IReadOnlyList<CardapioItemDto>?> ObterCardapioAsync(
        int unidadeId);

    Task<IReadOnlyList<EstoqueDto>> ListarEstoquesAsync(
        int? unidadeId = null);
}
