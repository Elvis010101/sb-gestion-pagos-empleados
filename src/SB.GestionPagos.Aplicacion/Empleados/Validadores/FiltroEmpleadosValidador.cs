using FluentValidation;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Aplicacion.Validaciones;
using SB.GestionPagos.Dominio.Repositorios;

namespace SB.GestionPagos.Aplicacion.Empleados.Validadores;

/// <summary>
/// Valida los criterios de búsqueda y el tramo de página que llegan en la cadena de consulta.
/// </summary>
/// <remarks>
/// Los límites de paginación se toman de <see cref="Paginacion"/>, en el Dominio, y no se
/// repiten aquí. El techo del tamaño de página es además una defensa concreta del RNF-04:
/// sin él, <c>?tamanoPagina=100000</c> convertiría la consulta paginada en una descarga
/// completa de la tabla.
/// </remarks>
public sealed class FiltroEmpleadosValidador : AbstractValidator<FiltroEmpleados>
{
    public FiltroEmpleadosValidador()
    {
        RuleFor(filtro => filtro.Nombre).TextoOpcional(LongitudMaxima.PRIMER_NOMBRE);
        RuleFor(filtro => filtro.Departamento).TextoOpcional(LongitudMaxima.DEPARTAMENTO);

        // Estado es anulable: no enviarlo significa "no filtrar por estado", y eso es válido.
        // La regla solo actúa cuando el cliente sí manda un valor.
        RuleFor(filtro => filtro.Estado)
            .ValorDeEnumeracionDefinido()
            .When(filtro => filtro.Estado.HasValue);

        RuleFor(filtro => filtro.Pagina).EnRangoInclusivo(
            Paginacion.PAGINA_MINIMA,
            Paginacion.PAGINA_MAXIMA);

        RuleFor(filtro => filtro.TamanoPagina).EnRangoInclusivo(
            Paginacion.TAMANO_PAGINA_MINIMO,
            Paginacion.TAMANO_PAGINA_MAXIMO);
    }
}
