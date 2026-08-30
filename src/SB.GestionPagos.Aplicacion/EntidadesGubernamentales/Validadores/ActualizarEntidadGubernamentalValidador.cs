using FluentValidation;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;

namespace SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Validadores;

/// <summary>Valida la forma del DTO de edición de una entidad gubernamental.</summary>
public sealed class ActualizarEntidadGubernamentalValidador : AbstractValidator<ActualizarEntidadGubernamentalDto>
{
    public ActualizarEntidadGubernamentalValidador()
    {
        RuleFor(solicitud => solicitud.Nombre).TextoObligatorio(LongitudMaxima.NOMBRE_ENTIDAD_GUBERNAMENTAL);
        RuleFor(solicitud => solicitud.Categoria).TextoObligatorio(LongitudMaxima.CATEGORIA);
        RuleFor(solicitud => solicitud.PoderDelEstado).TextoObligatorio(LongitudMaxima.PODER_DEL_ESTADO);
        RuleFor(solicitud => solicitud.Sector).TextoObligatorio(LongitudMaxima.SECTOR);
    }
}
