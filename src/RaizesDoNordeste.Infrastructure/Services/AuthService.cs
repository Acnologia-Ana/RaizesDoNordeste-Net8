using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Application.DTOs;
using RaizesDoNordeste.Application.Interfaces;
using RaizesDoNordeste.Domain.Entities;
using RaizesDoNordeste.Domain.Enums;
using RaizesDoNordeste.Infrastructure.Persistence;

namespace RaizesDoNordeste.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;

    public AuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UsuarioAutenticadoDto?>
        ValidarCredenciaisAsync(
            LoginRequestDto request)
    {
        var email =
            request.Email.Trim().ToLowerInvariant();

        var usuario =
            await _dbContext.Usuarios
                .SingleOrDefaultAsync(x =>
                    x.Email == email &&
                    x.Ativo);

        if (usuario is null)
            return null;

        var hasher =
            new PasswordHasher<Usuario>();

        var resultado =
            hasher.VerifyHashedPassword(
                usuario,
                usuario.SenhaHash,
                request.Senha);

        if (resultado ==
            PasswordVerificationResult.Failed)
        {
            return null;
        }

        return new UsuarioAutenticadoDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role.ToString());
    }

    public async Task<UsuarioAutenticadoDto?>
        CadastrarClienteAsync(
            CadastroClienteRequestDto request)
    {
        var email =
            request.Email.Trim().ToLowerInvariant();

        var existeEmail =
            await _dbContext.Usuarios
                .AnyAsync(x => x.Email == email);

        if (existeEmail)
            return null;

        if (!string.IsNullOrWhiteSpace(request.Cpf))
        {
            var existeCpf =
                await _dbContext.Clientes
                    .AnyAsync(x =>
                        x.Cpf == request.Cpf);

            if (existeCpf)
                return null;
        }

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = email,
            Role = RoleUsuario.Cliente,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var hasher =
            new PasswordHasher<Usuario>();

        usuario.SenhaHash =
            hasher.HashPassword(
                usuario,
                request.Senha);

        var cliente = new Cliente
        {
            Usuario = usuario,
            Cpf = string.IsNullOrWhiteSpace(request.Cpf)
                ? null
                : request.Cpf.Trim(),

            Telefone =
                string.IsNullOrWhiteSpace(request.Telefone)
                    ? null
                    : request.Telefone.Trim(),

            ConsentimentoFidelidade =
                request.ConsentimentoFidelidade,

            DataConsentimento =
                request.ConsentimentoFidelidade
                    ? DateTime.UtcNow
                    : null
        };

        if (request.ConsentimentoFidelidade)
        {
            cliente.Fidelidade =
                new Fidelidade
                {
                    SaldoPontos = 0
                };
        }

        _dbContext.Clientes.Add(cliente);

        await _dbContext.SaveChangesAsync();

        return new UsuarioAutenticadoDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role.ToString());
    }
}
