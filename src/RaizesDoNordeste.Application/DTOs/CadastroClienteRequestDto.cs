namespace RaizesDoNordeste.Application.DTOs;

public record CadastroClienteRequestDto(
    string Nome,
    string Email,
    string Senha,
    string? Cpf,
    string? Telefone,
    bool ConsentimentoFidelidade
);
