using CrmIctm.Api.Dominio;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrmIctm.Api.Infraestrutura;

public sealed class BancoContexto(DbContextOptions<BancoContexto> options)
    : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Igreja> Igrejas => Set<Igreja>();
    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();
    public DbSet<ObservacaoAtendimento> ObservacoesAtendimento => Set<ObservacaoAtendimento>();
    public DbSet<HistoricoAtendimento> HistoricosAtendimento => Set<HistoricoAtendimento>();
    public DbSet<RegistroAuditoriaAdministrativa> RegistrosAuditoriaAdministrativa =>
        Set<RegistroAuditoriaAdministrativa>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("crm");
        ConfigurarIgreja(builder);
        ConfigurarUsuario(builder);
        ConfigurarAtendimento(builder);
        ConfigurarObservacao(builder);
        ConfigurarHistorico(builder);
        ConfigurarAuditoriaAdministrativa(builder);
    }

    private static void ConfigurarIgreja(ModelBuilder builder)
    {
        var igreja = builder.Entity<Igreja>();
        igreja.ToTable("igrejas");
        igreja.HasKey(x => x.Id);
        igreja.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        igreja.Property(x => x.Logradouro).HasMaxLength(150).IsRequired();
        igreja.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        igreja.Property(x => x.Bairro).HasMaxLength(100).IsRequired();
        igreja.Property(x => x.Complemento).HasMaxLength(100);
        igreja.Property(x => x.Cep).HasMaxLength(8).IsFixedLength().IsRequired();
        igreja.Property(x => x.Cidade).HasMaxLength(100).IsRequired();
        igreja.Property(x => x.Estado).HasMaxLength(2).IsFixedLength().IsRequired();
        igreja.Property(x => x.IdentificadorPublico).HasMaxLength(50).IsRequired();
        igreja.HasIndex(x => x.IdentificadorPublico).IsUnique();
    }

    private static void ConfigurarUsuario(ModelBuilder builder)
    {
        var usuario = builder.Entity<Usuario>();
        usuario.ToTable("usuarios");
        usuario.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        usuario.Property(x => x.Perfil).HasConversion<string>().HasMaxLength(30).IsRequired();
        usuario.HasOne(x => x.Igreja)
            .WithMany()
            .HasForeignKey(x => x.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IdentityRole<Guid>>().ToTable("perfis_identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("usuarios_perfis_identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("usuarios_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("usuarios_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("perfis_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("usuarios_tokens");
    }

    private static void ConfigurarAtendimento(ModelBuilder builder)
    {
        var atendimento = builder.Entity<Atendimento>();
        atendimento.ToTable("atendimentos");
        atendimento.HasKey(x => x.Id);
        atendimento.Property(x => x.NomeVisitante).HasMaxLength(150).IsRequired();
        atendimento.Property(x => x.Whatsapp).HasMaxLength(14).IsRequired();
        atendimento.Property(x => x.Etapa).HasConversion<string>().HasMaxLength(30).IsRequired();
        atendimento.Property(x => x.Resultado).HasConversion<string>().HasMaxLength(30);
        atendimento.Property(x => x.AvisoPrivacidadeVersao).HasMaxLength(50).IsRequired();
        atendimento.Property(x => x.LogradouroVisitante).HasMaxLength(150);
        atendimento.Property(x => x.NumeroResidencia).HasMaxLength(20);
        atendimento.Property(x => x.BairroVisitante).HasMaxLength(100);
        atendimento.Property(x => x.ComplementoEndereco).HasMaxLength(100);
        atendimento.Property(x => x.SituacaoCongregacional)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(SituacaoCongregacional.NaoInformado)
            .IsRequired();
        atendimento.Property(x => x.NomeIgrejaCongrega).HasMaxLength(150);
        atendimento.HasIndex(x => new { x.IgrejaId, x.Etapa });
        atendimento.HasIndex(x => new { x.IgrejaId, x.Whatsapp });
        atendimento.HasOne(x => x.Igreja)
            .WithMany()
            .HasForeignKey(x => x.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);
        atendimento.HasOne(x => x.Responsavel)
            .WithMany()
            .HasForeignKey(x => x.ResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarObservacao(ModelBuilder builder)
    {
        var observacao = builder.Entity<ObservacaoAtendimento>();
        observacao.ToTable("observacoes_atendimento");
        observacao.HasKey(x => x.Id);
        observacao.Property(x => x.Texto).HasMaxLength(2_000).IsRequired();
        observacao.HasOne(x => x.Atendimento)
            .WithMany()
            .HasForeignKey(x => x.AtendimentoId)
            .OnDelete(DeleteBehavior.Cascade);
        observacao.HasOne(x => x.Autor)
            .WithMany()
            .HasForeignKey(x => x.AutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarHistorico(ModelBuilder builder)
    {
        var historico = builder.Entity<HistoricoAtendimento>();
        historico.ToTable("historicos_atendimento");
        historico.HasKey(x => x.Id);
        historico.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
        historico.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
        historico.HasOne(x => x.Atendimento)
            .WithMany()
            .HasForeignKey(x => x.AtendimentoId)
            .OnDelete(DeleteBehavior.Cascade);
        historico.HasOne(x => x.Autor)
            .WithMany()
            .HasForeignKey(x => x.AutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarAuditoriaAdministrativa(ModelBuilder builder)
    {
        var registro = builder.Entity<RegistroAuditoriaAdministrativa>();
        registro.ToTable("registros_auditoria_administrativa");
        registro.HasKey(x => x.Id);
        registro.Property(x => x.TipoEntidade).HasMaxLength(20).IsRequired();
        registro.Property(x => x.Acao).HasMaxLength(40).IsRequired();
        registro.Property(x => x.Descricao).HasMaxLength(500).IsRequired();
        registro.HasIndex(x => new { x.TipoEntidade, x.EntidadeId, x.CriadoEm });
        registro.HasOne(x => x.Autor)
            .WithMany()
            .HasForeignKey(x => x.AutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
