using System.Globalization;
using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Repositorios;
using SB.GestionPagos.Servicios.Mapeos;

namespace SB.GestionPagos.Servicios.EntidadesGubernamentales;

/// <summary>
/// Mantenimiento del listado de entidades gubernamentales (RF-09).
/// </summary>
/// <remarks>
/// El servicio es idéntico al que se escribiría contra SQL Server, y sin embargo detrás hay
/// un archivo de texto plano. Esa indiferencia es la prueba de que la Regla de Dependencia
/// funciona: el servicio habla con <see cref="IEntidadGubernamentalRepositorio"/>, declarada
/// en el Dominio, y no tiene forma de enterarse de dónde se guardan los datos.
///
/// A diferencia de los empleados, aquí la baja SÍ es física: esto es un catálogo, no un
/// historial. Una entidad gubernamental que se elimina del listado no deja pagos detrás que
/// haya que poder rastrear.
/// </remarks>
public sealed class EntidadGubernamentalServicio : IEntidadGubernamentalServicio
{
    private readonly IEntidadGubernamentalRepositorio _entidadGubernamentalRepositorio;
    private readonly ILogger<EntidadGubernamentalServicio> _registrador;

    public EntidadGubernamentalServicio(
        IEntidadGubernamentalRepositorio entidadGubernamentalRepositorio,
        ILogger<EntidadGubernamentalServicio> registrador)
    {
        _entidadGubernamentalRepositorio = entidadGubernamentalRepositorio;
        _registrador = registrador;
    }

    public async Task<Resultado<IReadOnlyList<EntidadGubernamentalDto>>> ObtenerTodasAsync(
        CancellationToken cancelacion)
    {
        IReadOnlyList<EntidadGubernamental> entidades =
            await _entidadGubernamentalRepositorio.ObtenerTodasAsync(cancelacion);

        return Resultado<IReadOnlyList<EntidadGubernamentalDto>>.Exitoso(ADtos(entidades));
    }

    public async Task<Resultado<IReadOnlyList<EntidadGubernamentalDto>>> BuscarAsync(
        FiltroEntidadesGubernamentales filtro,
        CancellationToken cancelacion)
    {
        // El DTO que llega de la Api y el filtro que entiende el Dominio son dos tipos
        // distintos aunque hoy tengan los mismos campos. El primero es el contrato con el
        // mundo exterior; el segundo, el contrato con el repositorio. Traducirlos aquí es lo
        // que permite renombrar un parámetro de la cadena de consulta sin tocar el Dominio.
        FiltroBusquedaEntidadGubernamental filtroDominio = new(filtro.Nombre, filtro.Sector);

        IReadOnlyList<EntidadGubernamental> entidades =
            await _entidadGubernamentalRepositorio.BuscarAsync(filtroDominio, cancelacion);

        _registrador.LogInformation(
            "Búsqueda de entidades gubernamentales. Nombre: {NombreBuscado}. Sector: {SectorBuscado}. "
            + "Coincidencias: {TotalCoincidencias}.",
            filtro.Nombre,
            filtro.Sector,
            entidades.Count);

        return Resultado<IReadOnlyList<EntidadGubernamentalDto>>.Exitoso(ADtos(entidades));
    }

    public async Task<Resultado<EntidadGubernamentalDto>> ObtenerPorIdAsync(
        int identificador,
        CancellationToken cancelacion)
    {
        EntidadGubernamental? entidad =
            await _entidadGubernamentalRepositorio.ObtenerPorIdAsync(identificador, cancelacion);

        if (entidad is null)
        {
            return Resultado<EntidadGubernamentalDto>.NoEncontrado(NoEncontrada(identificador));
        }

        return Resultado<EntidadGubernamentalDto>.Exitoso(MapeadorEntidadGubernamental.ADto(entidad));
    }

    public async Task<Resultado<EntidadGubernamentalDto>> CrearAsync(
        CrearEntidadGubernamentalDto solicitud,
        CancellationToken cancelacion)
    {
        EntidadGubernamental entidad = new(
            solicitud.Nombre,
            solicitud.Categoria,
            solicitud.PoderDelEstado,
            solicitud.Sector);

        // El identificador lo asigna el repositorio: aquí no hay motor de base de datos que
        // lo genere, porque el almacén es un archivo.
        await _entidadGubernamentalRepositorio.AgregarAsync(entidad, cancelacion);

        _registrador.LogInformation(
            "Entidad gubernamental creada. Identificador: {IdentificadorEntidad}. Nombre: {NombreEntidad}.",
            entidad.Id,
            entidad.Nombre);

        return Resultado<EntidadGubernamentalDto>.Exitoso(MapeadorEntidadGubernamental.ADto(entidad));
    }

    public async Task<Resultado<EntidadGubernamentalDto>> ActualizarAsync(
        int identificador,
        ActualizarEntidadGubernamentalDto solicitud,
        CancellationToken cancelacion)
    {
        EntidadGubernamental? entidad =
            await _entidadGubernamentalRepositorio.ObtenerPorIdAsync(identificador, cancelacion);

        if (entidad is null)
        {
            return Resultado<EntidadGubernamentalDto>.NoEncontrado(NoEncontrada(identificador));
        }

        entidad.Actualizar(
            solicitud.Nombre,
            solicitud.Categoria,
            solicitud.PoderDelEstado,
            solicitud.Sector);

        await _entidadGubernamentalRepositorio.ActualizarAsync(entidad, cancelacion);

        _registrador.LogInformation(
            "Entidad gubernamental actualizada. Identificador: {IdentificadorEntidad}. Nombre: {NombreEntidad}.",
            entidad.Id,
            entidad.Nombre);

        return Resultado<EntidadGubernamentalDto>.Exitoso(MapeadorEntidadGubernamental.ADto(entidad));
    }

    public async Task<Resultado> EliminarAsync(int identificador, CancellationToken cancelacion)
    {
        EntidadGubernamental? entidad =
            await _entidadGubernamentalRepositorio.ObtenerPorIdAsync(identificador, cancelacion);

        if (entidad is null)
        {
            return Resultado.NoEncontrado(NoEncontrada(identificador));
        }

        await _entidadGubernamentalRepositorio.EliminarAsync(entidad, cancelacion);

        _registrador.LogInformation(
            "Entidad gubernamental eliminada. Identificador: {IdentificadorEntidad}. Nombre: {NombreEntidad}.",
            entidad.Id,
            entidad.Nombre);

        return Resultado.Exitoso();
    }

    private static IReadOnlyList<EntidadGubernamentalDto> ADtos(IReadOnlyList<EntidadGubernamental> entidades)
    {
        List<EntidadGubernamentalDto> respuesta = new(entidades.Count);
        foreach (EntidadGubernamental entidad in entidades)
        {
            respuesta.Add(MapeadorEntidadGubernamental.ADto(entidad));
        }

        return respuesta;
    }

    private static string NoEncontrada(int identificador)
        => string.Format(
            CultureInfo.InvariantCulture,
            "No existe una entidad gubernamental con el identificador {0}.",
            identificador);
}
