using System.Security.Claims;
using CrmIctm.Api.Dominio;
using CrmIctm.Api.Infraestrutura;
using Microsoft.EntityFrameworkCore;

namespace CrmIctm.Api.Servicos;

public sealed class UsuarioAtual(BancoContexto banco, IHttpContextAccessor contextoHttp)
{
    public async Task<Usuario?> ObterAsync(
        CancellationToken cancellationToken,
        bool permitirTrocaSenhaPendente = false)
    {
        var idTexto = contextoHttp.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idTexto, out var id))
        {
            return null;
        }

        return await banco.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id &&
                     x.Ativo &&
                     (x.IgrejaId == null || x.Igreja!.Ativa) &&
                     (permitirTrocaSenhaPendente || !x.DeveTrocarSenha),
                cancellationToken);
    }
}
