namespace RaizesDoNordeste.Domain.Entities;

public class Fidelidade
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int SaldoPontos { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public ICollection<MovimentacaoFidelidade> Movimentacoes { get; set; }
        = new List<MovimentacaoFidelidade>();

    public void AdicionarPontos(int pontos)
    {
        if (pontos <= 0)
            throw new ArgumentException(
                "A quantidade de pontos deve ser maior que zero."
            );

        SaldoPontos += pontos;
    }

    public void ResgatarPontos(int pontos)
    {
        if (pontos <= 0)
            throw new ArgumentException(
                "A quantidade de pontos deve ser maior que zero."
            );

        if (SaldoPontos < pontos)
            throw new InvalidOperationException(
                "Saldo de pontos insuficiente."
            );

        SaldoPontos -= pontos;
    }
}
