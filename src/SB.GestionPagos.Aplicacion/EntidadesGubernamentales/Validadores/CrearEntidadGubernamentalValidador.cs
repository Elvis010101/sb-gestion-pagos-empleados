using FluentValidation;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;

namespace SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Validadores;

/// <summary>Valida la forma del DTO de alta de una entidad gubernamental.</summary>
public sealed class CrearEntidadGubernamentalValidador : AbstractValidator<CrearEntidadGubernamentalDto>
{
    public CrearEntidadGubernamentalValidador()
    {
        RuleFor(solicitud => solicitud.Nombre).TextoObligatorio(LongitudMaxima.NOMBRE_ENTIDAD_GUBERNAMENTAL);
        RuleFor(solicitud => solicitud.Categoria).TextoObligatorio(LongitudMaxima.CATEGORIA);
        RuleFor(solicitud => solicitud.PoderDelEstado).TextoObligatorio(LongitudMaxima.PODER_DEL_ESTADO);
        RuleFor(solicitud => solicitud.Sector).TextoObligatorio(LongitudMaxima.SECTOR);
    }
}
