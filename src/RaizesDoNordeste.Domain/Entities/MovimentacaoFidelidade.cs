using RaizesDoNordeste.Domain.Enums;

namespace RaizesDoNordeste.Domain.Entities;

public class MovimentacaoFidelidade
{
    public int Id { get; set; }

    public int FidelidadeId { get; set; }

    public TipoMovimentacaoFidelidade Tipo { get; set; }

    public int Pontos { get; set; }

    public string? Descricao { get; set; }

    public DateTime CriadoEm { get; set; }
        = DateTime.UtcNow;

    public Fidelidade Fidelidade { get; set; } = null!;
}
