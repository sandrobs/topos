using Microsoft.AspNetCore.Identity;

namespace CrmIctm.Api.Dominio;

public sealed class Usuario : IdentityUser<Guid>
{
    public string Nome { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public Guid? IgrejaId { get; set; }
    public Igreja? Igreja { get; set; }
    public bool Ativo { get; set; } = true;
    public bool DeveTrocarSenha { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
}
