namespace CrmIctm.Api.Infraestrutura;

public interface IRelogio
{
    DateTimeOffset Agora { get; }
}

public sealed class RelogioSistema : IRelogio
{
    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}

