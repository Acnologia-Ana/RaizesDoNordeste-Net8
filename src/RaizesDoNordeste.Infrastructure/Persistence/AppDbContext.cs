using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Domain.Entities;

namespace RaizesDoNordeste.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Estoque> Estoques => Set<Estoque>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<Fidelidade> Fidelidades => Set<Fidelidade>();
    public DbSet<MovimentacaoFidelidade> MovimentacoesFidelidade => Set<MovimentacaoFidelidade>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarUsuario(modelBuilder);
        ConfigurarCliente(modelBuilder);
        ConfigurarUnidade(modelBuilder);
        ConfigurarProduto(modelBuilder);
        ConfigurarEstoque(modelBuilder);
        ConfigurarPedido(modelBuilder);
        ConfigurarItemPedido(modelBuilder);
        ConfigurarPagamento(modelBuilder);
        ConfigurarFidelidade(modelBuilder);
        ConfigurarMovimentacaoFidelidade(modelBuilder);
        ConfigurarAuditoria(modelBuilder);
    }

    private static void ConfigurarUsuario(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Usuario>();

        entity.ToTable("Usuarios");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(180);

        entity.HasIndex(x => x.Email)
            .IsUnique();

        entity.Property(x => x.SenhaHash)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(30);

        entity.HasOne(x => x.Cliente)
            .WithOne(x => x.Usuario)
            .HasForeignKey<Cliente>(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarCliente(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Cliente>();

        entity.ToTable("Clientes");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Cpf)
            .HasMaxLength(14);

        entity.Property(x => x.Telefone)
            .HasMaxLength(20);

        entity.HasIndex(x => x.Cpf)
            .IsUnique();

        entity.HasMany(x => x.Pedidos)
            .WithOne(x => x.Cliente)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(x => x.Fidelidade)
            .WithOne(x => x.Cliente)
            .HasForeignKey<Fidelidade>(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarUnidade(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Unidade>();

        entity.ToTable("Unidades");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(x => x.Cidade)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.Estado)
            .IsRequired()
            .HasMaxLength(2);

        entity.HasMany(x => x.Pedidos)
            .WithOne(x => x.Unidade)
            .HasForeignKey(x => x.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarProduto(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Produto>();

        entity.ToTable("Produtos");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(x => x.Descricao)
            .HasMaxLength(500);

        entity.Property(x => x.Preco)
            .HasPrecision(10, 2);
    }

    private static void ConfigurarEstoque(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Estoque>();

        entity.ToTable("Estoques");

        entity.HasKey(x => x.Id);

        entity.HasIndex(x => new
        {
            x.UnidadeId,
            x.ProdutoId
        })
        .IsUnique();

        entity.HasOne(x => x.Unidade)
            .WithMany(x => x.Estoques)
            .HasForeignKey(x => x.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Produto)
            .WithMany(x => x.Estoques)
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarPedido(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Pedido>();

        entity.ToTable("Pedidos");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.CanalPedido)
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        entity.Property(x => x.ValorTotal)
            .HasPrecision(10, 2);

        entity.HasMany(x => x.Itens)
            .WithOne(x => x.Pedido)
            .HasForeignKey(x => x.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(x => x.Pagamentos)
            .WithOne(x => x.Pedido)
            .HasForeignKey(x => x.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarItemPedido(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ItemPedido>();

        entity.ToTable("ItensPedido");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.PrecoUnitario)
            .HasPrecision(10, 2);

        entity.Ignore(x => x.Subtotal);

        entity.HasOne(x => x.Produto)
            .WithMany(x => x.ItensPedido)
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarPagamento(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Pagamento>();

        entity.ToTable("Pagamentos");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Valor)
            .HasPrecision(10, 2);

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.Property(x => x.CodigoTransacao)
            .HasMaxLength(100);

        entity.Property(x => x.MensagemRetorno)
            .HasMaxLength(500);
    }

    private static void ConfigurarFidelidade(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Fidelidade>();

        entity.ToTable("Fidelidades");

        entity.HasKey(x => x.Id);

        entity.HasIndex(x => x.ClienteId)
            .IsUnique();

        entity.HasMany(x => x.Movimentacoes)
            .WithOne(x => x.Fidelidade)
            .HasForeignKey(x => x.FidelidadeId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarMovimentacaoFidelidade(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MovimentacaoFidelidade>();

        entity.ToTable("MovimentacoesFidelidade");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.Property(x => x.Descricao)
            .HasMaxLength(300);
    }

    private static void ConfigurarAuditoria(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Auditoria>();

        entity.ToTable("Auditorias");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Acao)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.Entidade)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.EntidadeId)
            .HasMaxLength(100);

        entity.HasOne(x => x.Usuario)
            .WithMany(x => x.Auditorias)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
