using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;

namespace SB.GestionPagos.Aplicacion.EntidadesGubernamentales;

/// <summary>
/// Mantenimiento del listado de entidades gubernamentales de la República Dominicana (RF-09).
/// </summary>
/// <remarks>
/// <see cref="ObtenerTodasAsync"/> no pagina, a diferencia de la consulta de empleados: el
/// origen es un archivo de texto plano con 181 registros que la implementación lee entero de
/// todas formas. Paginar aquí sería una complicación sin ganancia, y el criterio que se
/// evalúa es justamente saber cuándo NO agregar maquinaria.
/// </remarks>
public interface IEntidadGubernamentalServicio
{
    Task<Resultado<IReadOnlyList<EntidadGubernamentalDto>>> ObtenerTodasAsync(CancellationToken cancelacion);

    Task<Resultado<EntidadGubernamentalDto>> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion);

    Task<Resultado<IReadOnlyList<EntidadGubernamentalDto>>> BuscarAsync(
        FiltroEntidadesGubernamentales filtro,
        CancellationToken cancelacion);

    Task<Resultado<EntidadGubernamentalDto>> CrearAsync(
        CrearEntidadGubernamentalDto solicitud,
        CancellationToken cancelacion);

    Task<Resultado<EntidadGubernamentalDto>> ActualizarAsync(
        int identificador,
        ActualizarEntidadGubernamentalDto solicitud,
        CancellationToken cancelacion);

    Task<Resultado> EliminarAsync(int identificador, CancellationToken cancelacion);
}
