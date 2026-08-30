using Microsoft.Extensions.DependencyInjection;
using SB.GestionPagos.Aplicacion.Autenticacion;
using SB.GestionPagos.Aplicacion.Empleados;
using SB.GestionPagos.Aplicacion.Empleados.Dtos;
using SB.GestionPagos.Aplicacion.EntidadesGubernamentales;
using SB.GestionPagos.Aplicacion.Reportes;
using SB.GestionPagos.Servicios.Autenticacion;
using SB.GestionPagos.Servicios.Empleados;
using SB.GestionPagos.Servicios.EntidadesGubernamentales;
using SB.GestionPagos.Servicios.Reportes;

namespace SB.GestionPagos.Servicios.Configuracion;

/// <summary>
/// Registro en el contenedor de dependencias de las implementaciones de esta capa.
/// </summary>
/// <remarks>
/// Este archivo es la única costura donde una interfaz de Aplicación se encuentra con su
/// implementación concreta. El proyecto Api inyecta <c>IEmpleadoServicio</c> y no llega a
/// nombrar nunca a <c>EmpleadoServicio</c>: por eso los controladores pueden probarse contra
/// dobles y por eso cambiar una implementación no obliga a tocar el host.
///
/// Aquí también se ve el precio real de agregar un tipo de empleado: una línea más.
/// </remarks>
public static class ConfiguracionServicios
{
    /// <summary>
    /// Registra los servicios de aplicación con alcance por petición.
    /// </summary>
    /// <remarks>
    /// <c>Scoped</c> y no <c>Singleton</c> porque dependen de repositorios que, en el caso de
    /// empleados, envuelven un <c>DbContext</c> de EF Core: ese objeto no es seguro para uso
    /// concurrente y su vida tiene que terminar con la petición.
    /// </remarks>
    public static IServiceCollection AgregarServicios(this IServiceCollection servicios)
    {
        servicios.AddScoped<IEmpleadoServicio, EmpleadoServicio>();

        servicios.AddScoped<
            IEmpleadoServicio<CrearEmpleadoAsalariadoDto, ActualizarEmpleadoAsalariadoDto>,
            EmpleadoAsalariadoServicio>();

        servicios.AddScoped<
            IEmpleadoServicio<CrearEmpleadoPorHorasDto, ActualizarEmpleadoPorHorasDto>,
            EmpleadoPorHorasServicio>();

        servicios.AddScoped<
            IEmpleadoServicio<CrearEmpleadoPorComisionDto, ActualizarEmpleadoPorComisionDto>,
            EmpleadoPorComisionServicio>();

        servicios.AddScoped<
            IEmpleadoServicio<CrearEmpleadoAsalariadoPorComisionDto, ActualizarEmpleadoAsalariadoPorComisionDto>,
            EmpleadoAsalariadoPorComisionServicio>();

        servicios.AddScoped<IEntidadGubernamentalServicio, EntidadGubernamentalServicio>();
        servicios.AddScoped<IReporteServicio, ReporteServicio>();
        servicios.AddScoped<IAutenticacionServicio, AutenticacionServicio>();

        return servicios;
    }
}
