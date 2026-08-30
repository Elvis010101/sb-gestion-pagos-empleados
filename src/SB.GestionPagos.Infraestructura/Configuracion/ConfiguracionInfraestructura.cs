using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SB.GestionPagos.Dominio.Repositorios;
using SB.GestionPagos.Infraestructura.Persistencia;
using SB.GestionPagos.Infraestructura.Persistencia.Repositorios;

namespace SB.GestionPagos.Infraestructura.Configuracion;

/// <summary>
/// Registro en el contenedor de dependencias de lo que aporta la capa Infraestructura.
/// </summary>
/// <remarks>
/// Esta clase es la única costura donde una interfaz del Dominio se encuentra con su
/// implementación de base de datos. El proyecto Api llama a un método y no llega a nombrar
/// nunca ni al <c>DbContext</c> ni a los repositorios concretos: por eso ambos pueden ser
/// <c>internal</c>.
/// </remarks>
public static class ConfiguracionInfraestructura
{
    /// <summary>
    /// Clave bajo la que se busca la cadena de conexión en la sección
    /// <c>ConnectionStrings</c> del appsettings.
    /// </summary>
    /// <remarks>
    /// La norma de SB prohíbe las cadenas de conexión en código. Lo que vive aquí es el
    /// NOMBRE de la clave, no el valor: el servidor, la base y la contraseña llegan desde
    /// la configuración del host.
    /// </remarks>
    public const string NOMBRE_CADENA_CONEXION = "SbGestionPagos";

    private const string MENSAJE_CADENA_CONEXION_AUSENTE =
        "No se encontró la cadena de conexión '" + NOMBRE_CADENA_CONEXION +
        "' en la configuración. Revise la sección ConnectionStrings del appsettings.";

    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection servicios,
        IConfiguration configuracion)
    {
        string? cadenaConexion = configuracion.GetConnectionString(NOMBRE_CADENA_CONEXION);

        // Falla al arrancar y no en la primera consulta. Una aplicación que levanta bien y
        // revienta cuando el primer usuario entra es mucho más difícil de diagnosticar que
        // una que se niega a levantar diciendo exactamente qué le falta.
        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(MENSAJE_CADENA_CONEXION_AUSENTE);
        }

        // AddDbContext registra el contexto con alcance Scoped: una instancia por petición
        // HTTP, liberada al terminarla. No es un objeto seguro para uso concurrente, y su
        // rastreador de cambios acumula estado que no debe sobrevivir a la petición.
        servicios.AddDbContext<GestionPagosDbContext>(opciones =>
            opciones.UseSqlServer(cadenaConexion));

        // Los repositorios comparten el alcance del contexto que envuelven: si fueran
        // Singleton, el primero en resolverse se quedaría para siempre con el DbContext de
        // la primera petición.
        servicios.AddScoped<IEmpleadoRepositorio, EmpleadoRepositorioSql>();
        servicios.AddScoped<IUsuarioRepositorio, UsuarioRepositorioSql>();

        return servicios;
    }
}
