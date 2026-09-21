using RaizesDoNordeste.Domain.Enums;

namespace RaizesDoNordeste.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }

    public int UnidadeId { get; set; }

    public CanalPedido CanalPedido { get; set; }

    public StatusPedido Status { get; set; }
        = StatusPedido.AguardandoPagamento;

    public decimal ValorTotal { get; set; }

    public DateTime CriadoEm { get; set; }
        = DateTime.UtcNow;

    public DateTime? AtualizadoEm { get; set; }

    public Cliente? Cliente { get; set; }

    public Unidade Unidade { get; set; } = null!;

    public ICollection<ItemPedido> Itens { get; set; }
        = new List<ItemPedido>();

    public ICollection<Pagamento> Pagamentos { get; set; }
        = new List<Pagamento>();

    public void RecalcularValorTotal()
    {
        ValorTotal = Itens.Sum(
            item => item.Quantidade * item.PrecoUnitario
        );
    }

    public void AtualizarStatus(StatusPedido novoStatus)
    {
        Status = novoStatus;
        AtualizadoEm = DateTime.UtcNow;
    }
}
