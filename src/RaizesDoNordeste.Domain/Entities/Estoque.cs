namespace RaizesDoNordeste.Domain.Entities;

public class Estoque
{
    public int Id { get; set; }

    public int UnidadeId { get; set; }

    public int ProdutoId { get; set; }

    public int Quantidade { get; set; }

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public Unidade Unidade { get; set; } = null!;

    public Produto Produto { get; set; } = null!;

    public bool PossuiQuantidade(int quantidadeSolicitada)
    {
        return quantidadeSolicitada > 0 &&
               Quantidade >= quantidadeSolicitada;
    }

    public void Baixar(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException(
                "A quantidade deve ser maior que zero."
            );

        if (Quantidade < quantidade)
            throw new InvalidOperationException(
                "Estoque insuficiente."
            );

        Quantidade -= quantidade;
        AtualizadoEm = DateTime.UtcNow;
    }
}
