using FluentValidation;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;

namespace SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Validadores;

/// <summary>
/// Valida los criterios de búsqueda del catálogo que llegan en la cadena de consulta.
/// </summary>
/// <remarks>
/// Ambos criterios son opcionales: no enviarlos significa "no filtrar por esto", y una
/// búsqueda sin filtros es la lista completa, que es una petición válida. Las cotas de
/// longitud son defensivas: sin ellas, un nombre de varios megabytes obligaría a recorrer
/// las 181 entidades comparando contra esa cadena.
/// </remarks>
public sealed class FiltroEntidadesGubernamentalesValidador : AbstractValidator<FiltroEntidadesGubernamentales>
{
    public FiltroEntidadesGubernamentalesValidador()
    {
        RuleFor(filtro => filtro.Nombre).TextoOpcional(LongitudMaxima.NOMBRE_ENTIDAD_GUBERNAMENTAL);
        RuleFor(filtro => filtro.Sector).TextoOpcional(LongitudMaxima.SECTOR);
    }
}
