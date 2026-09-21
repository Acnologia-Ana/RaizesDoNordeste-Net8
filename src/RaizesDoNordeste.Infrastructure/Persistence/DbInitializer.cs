using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Domain.Entities;

namespace RaizesDoNordeste.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(
        AppDbContext dbContext)
    {
        if (await dbContext.Unidades.AnyAsync())
            return;

        var recife = new Unidade
        {
            Nome = "Recife Centro",
            Cidade = "Recife",
            Estado = "PE",
            Ativa = true
        };

        var olinda = new Unidade
        {
            Nome = "Olinda",
            Cidade = "Olinda",
            Estado = "PE",
            Ativa = true
        };

        var produtos = new[]
        {
            new Produto
            {
                Nome = "Tapioca de Carne de Sol",
                Descricao =
                    "Tapioca recheada com carne de sol.",
                Preco = 18.90m,
                Ativo = true
            },

            new Produto
            {
                Nome = "Cuscuz com Queijo Coalho",
                Descricao =
                    "Cuscuz nordestino com queijo coalho.",
                Preco = 14.50m,
                Ativo = true
            },

            new Produto
            {
                Nome = "Bolo de Macaxeira",
                Descricao =
                    "Fatia de bolo tradicional de macaxeira.",
                Preco = 9.90m,
                Ativo = true
            },

            new Produto
            {
                Nome = "Suco de Caja",
                Descricao =
                    "Suco regional de caja.",
                Preco = 8.50m,
                Ativo = true
            },

            new Produto
            {
                Nome = "Cafe Nordestino",
                Descricao =
                    "Cafe coado tradicional.",
                Preco = 6.00m,
                Ativo = true
            }
        };

        dbContext.Unidades.AddRange(
            recife,
            olinda);

        dbContext.Produtos.AddRange(produtos);

        await dbContext.SaveChangesAsync();

        var estoques = new[]
        {
            new Estoque
            {
                UnidadeId = recife.Id,
                ProdutoId = produtos[0].Id,
                Quantidade = 30
            },

            new Estoque
            {
                UnidadeId = recife.Id,
                ProdutoId = produtos[1].Id,
                Quantidade = 40
            },

            new Estoque
            {
                UnidadeId = recife.Id,
                ProdutoId = produtos[2].Id,
                Quantidade = 18
            },

            new Estoque
            {
                UnidadeId = recife.Id,
                ProdutoId = produtos[3].Id,
                Quantidade = 25
            },

            new Estoque
            {
                UnidadeId = recife.Id,
                ProdutoId = produtos[4].Id,
                Quantidade = 50
            },

            new Estoque
            {
                UnidadeId = olinda.Id,
                ProdutoId = produtos[0].Id,
                Quantidade = 20
            },

            new Estoque
            {
                UnidadeId = olinda.Id,
                ProdutoId = produtos[1].Id,
                Quantidade = 15
            },

            new Estoque
            {
                UnidadeId = olinda.Id,
                ProdutoId = produtos[2].Id,
                Quantidade = 10
            },

            new Estoque
            {
                UnidadeId = olinda.Id,
                ProdutoId = produtos[4].Id,
                Quantidade = 30
            }
        };

        dbContext.Estoques.AddRange(estoques);

        await dbContext.SaveChangesAsync();
    }
}
