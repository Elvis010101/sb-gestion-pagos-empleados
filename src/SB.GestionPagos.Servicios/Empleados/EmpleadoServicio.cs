using Microsoft.Extensions.Logging;
using SB.GestionPagos.Aplicacion.Comunes;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Dominio.Enumeraciones;
using SB.GestionPagos.Dominio.Repositorios;
using SB.GestionPagos.Servicios.Mapeos;

namespace SB.GestionPagos.Servicios.Empleados;

/// <summary>
/// Operaciones de empleados comunes a los cuatro tipos: consulta, obtención y baja lógica.
/// </summary>
/// <remarks>
/// El servicio orquesta y no calcula. Traduce el filtro de la Api a los criterios que entiende
/// el repositorio, le pide los datos, deja que cada empleado calcule su propio pago y arma la
/// respuesta. No hay una sola operación aritmética de negocio en esta clase.
/// </remarks>
public sealed class EmpleadoServicio : IEmpleadoServicio
{
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly ILogger<EmpleadoServicio> _registrador;

    public EmpleadoServicio(IEmpleadoRepositorio empleadoRepositorio, ILogger<EmpleadoServicio> registrador)
    {
        _empleadoRepositorio = empleadoRepositorio;
        _registrador = registrador;
    }

    /// <summary>
    /// Devuelve la página de empleados que cumplen el filtro (RF-03).
    /// </summary>
    /// <remarks>
    /// Los criterios y el tramo de página se arman aquí y se entregan al repositorio, que es
    /// quien los traduce a la consulta. El servicio nunca recibe la colección completa para
    /// recortarla: si lo hiciera, el RNF-04 sería imposible de cumplir por diseño.
    /// </remarks>
    public async Task<Resultado<PaginaDto<EmpleadoDto>>> BuscarAsync(
        FiltroEmpleados filtro,
        CancellationToken cancelacion)
    {
        FiltroBusquedaEmpleado criterios = new(filtro.Nombre, filtro.Departamento, filtro.Estado);

        // Construir Paginacion revalida la página y el tamaño. FluentValidation ya los revisó
        // en la frontera HTTP; esta es la red de seguridad para cualquier otro llamador.
        Paginacion paginacion = new(filtro.Pagina, filtro.TamanoPagina);

        PaginaDeRegistros<Empleado> pagina =
            await _empleadoRepositorio.BuscarPaginaAsync(criterios, paginacion, cancelacion);

        List<EmpleadoDto> empleados = new(pagina.Elementos.Count);
        foreach (Empleado empleado in pagina.Elementos)
        {
            empleados.Add(MapeadorEmpleado.AEmpleadoDto(empleado));
        }

        PaginaDto<EmpleadoDto> respuesta = new(
            empleados,
            pagina.TotalRegistros,
            paginacion.Pagina,
            paginacion.TamanoPagina);

        return Resultado<PaginaDto<EmpleadoDto>>.Exitoso(respuesta);
    }

    public async Task<Resultado<EmpleadoDto>> ObtenerPorIdAsync(int identificador, CancellationToken cancelacion)
    {
        Empleado? empleado = await _empleadoRepositorio.ObtenerPorIdAsync(identificador, cancelacion);

        if (empleado is null)
        {
            return Resultado<EmpleadoDto>.NoEncontrado(MensajesEmpleado.NoEncontrado(identificador));
        }

        return Resultado<EmpleadoDto>.Exitoso(MapeadorEmpleado.AEmpleadoDto(empleado));
    }

    /// <summary>
    /// Da de baja al empleado marcándolo como inactivo. No borra la fila.
    /// </summary>
    /// <remarks>
    /// Es idempotente: dar de baja a alguien que ya está inactivo deja el sistema en el
    /// estado pedido, así que se informa éxito sin volver a escribir. Un doble clic en la
    /// interfaz no debe producir un error.
    /// </remarks>
    public async Task<Resultado> EliminarAsync(int identificador, CancellationToken cancelacion)
    {
        Empleado? empleado = await _empleadoRepositorio.ObtenerPorIdAsync(identificador, cancelacion);

        if (empleado is null)
        {
            return Resultado.NoEncontrado(MensajesEmpleado.NoEncontrado(identificador));
        }

        if (empleado.Estado == EstadoEmpleado.Inactivo)
        {
            _registrador.LogInformation(
                "Baja solicitada sobre un empleado que ya estaba inactivo. Identificador: {IdentificadorEmpleado}.",
                identificador);

            return Resultado.Exitoso();
        }

        empleado.CambiarEstado(EstadoEmpleado.Inactivo);
        await _empleadoRepositorio.ActualizarAsync(empleado, cancelacion);

        _registrador.LogInformation(
            "Empleado dado de baja. Identificador: {IdentificadorEmpleado}. Tipo de contrato: {TipoContrato}.",
            empleado.Id,
            empleado.TipoContrato);

        return Resultado.Exitoso();
    }
}
