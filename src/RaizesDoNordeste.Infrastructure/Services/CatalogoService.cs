using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.DTOs;
using RaizesDoNordeste.Application.Interfaces;
using RaizesDoNordeste.Infrastructure.Persistence;

namespace RaizesDoNordeste.Infrastructure.Services;

public class CatalogoService : ICatalogoService
{
    private readonly AppDbContext _dbContext;

    public CatalogoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UnidadeDto>>
        ListarUnidadesAsync()
    {
        return await _dbContext.Unidades
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new UnidadeDto(
                x.Id,
                x.Nome,
                x.Cidade,
                x.Estado,
                x.Ativa))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProdutoDto>>
        ListarProdutosAsync()
    {
        return await _dbContext.Produtos
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new ProdutoDto(
                x.Id,
                x.Nome,
                x.Descricao,
                x.Preco,
                x.Ativo))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CardapioItemDto>?>
        ObterCardapioAsync(int unidadeId)
    {
        var unidadeExiste = await _dbContext.Unidades
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == unidadeId &&
                x.Ativa);

        if (!unidadeExiste)
            return null;

        return await _dbContext.Estoques
            .AsNoTracking()
            .Where(x =>
                x.UnidadeId == unidadeId &&
                x.Produto.Ativo &&
                x.Quantidade > 0)
            .OrderBy(x => x.Produto.Nome)
            .Select(x => new CardapioItemDto(
                x.ProdutoId,
                x.Produto.Nome,
                x.Produto.Descricao,
                x.Produto.Preco,
                x.Quantidade))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<EstoqueDto>>
        ListarEstoquesAsync(int? unidadeId = null)
    {
        var query = _dbContext.Estoques
            .AsNoTracking()
            .AsQueryable();

        if (unidadeId.HasValue)
        {
            query = query.Where(
                x => x.UnidadeId == unidadeId.Value);
        }

        return await query
            .OrderBy(x => x.Unidade.Nome)
            .ThenBy(x => x.Produto.Nome)
            .Select(x => new EstoqueDto(
                x.Id,
                x.UnidadeId,
                x.Unidade.Nome,
                x.ProdutoId,
                x.Produto.Nome,
                x.Quantidade))
            .ToListAsync();
    }
}
