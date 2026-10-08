using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaizesDoNordeste.Domain.Entities;
using RaizesDoNordeste.Domain.Enums;

namespace RaizesDoNordeste.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        IConfiguration configuration)
    {
        await SeedAdminAsync(
            dbContext,
            configuration);

        await SeedCatalogoAsync(
            dbContext);
    }

    private static async Task SeedAdminAsync(
        AppDbContext dbContext,
        IConfiguration configuration)
    {
        var email =
            configuration["AdminSeed:Email"]
            ?? "admin@raizesdonordeste.local";

        email =
            email.Trim().ToLowerInvariant();

        var password =
            configuration["AdminSeed:Password"]
            ?? throw new InvalidOperationException(
                "AdminSeed:Password nao configurada.");

        var existe =
            await dbContext.Usuarios
                .AnyAsync(x => x.Email == email);

        if (existe)
            return;

        var admin = new Usuario
        {
            Nome = "Administrador",
            Email = email,
            Role = RoleUsuario.Admin,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var hasher =
            new PasswordHasher<Usuario>();

        admin.SenhaHash =
            hasher.HashPassword(
                admin,
                password);

        dbContext.Usuarios.Add(admin);

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedCatalogoAsync(
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

        dbContext.Produtos.AddRange(
            produtos);

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

        dbContext.Estoques.AddRange(
            estoques);

        await dbContext.SaveChangesAsync();
    }
}
