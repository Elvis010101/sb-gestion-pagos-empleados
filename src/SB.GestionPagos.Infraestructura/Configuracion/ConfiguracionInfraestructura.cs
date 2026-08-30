using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SB.GestionPagos.Aplicacion.Seguridad;
using SB.GestionPagos.Dominio.Repositorios;
using SB.GestionPagos.Infraestructura.DatosPlanos;
using SB.GestionPagos.Infraestructura.Persistencia;
using SB.GestionPagos.Infraestructura.Persistencia.Repositorios;
using SB.GestionPagos.Infraestructura.Seguridad;

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

        AgregarAlmacenamientoEnArchivoPlano(servicios, configuracion);
        AgregarSeguridad(servicios, configuracion);

        return servicios;
    }

    /// <summary>
    /// Registra los dos servicios técnicos de seguridad: el hash de contraseñas y la emisión
    /// de tokens.
    /// </summary>
    /// <remarks>
    /// Ambos son <c>Singleton</c> porque no guardan estado por petición: uno aplica un
    /// algoritmo sobre el texto que recibe y el otro firma con una clave que no cambia
    /// mientras el proceso vive. Construir uno nuevo en cada petición sería trabajo tirado.
    ///
    /// Aquí se ve el cierre del Principio de Inversión de Dependencias: la capa Servicios
    /// declaró que necesitaba "algo que hashee" y "algo que emita tokens" mediante dos
    /// interfaces de Aplicación, y es esta línea —y solo esta— la que decide que ese algo
    /// sea BCrypt y HMAC-SHA256.
    /// </remarks>
    private static void AgregarSeguridad(IServiceCollection servicios, IConfiguration configuracion)
    {
        // Se lee y se comprueba AL ARRANCAR. Si falta la clave de firma o quedó el marcador
        // del repositorio, la aplicación no levanta.
        servicios.AddSingleton(OpcionesJwt.LeerDe(configuracion));

        servicios.AddSingleton<IServicioHash, ServicioHashBCrypt>();
        servicios.AddSingleton<IGeneradorTokenJwt, GeneradorTokenJwt>();
    }

    /// <summary>
    /// Registra el catálogo de entidades gubernamentales, respaldado por un archivo de texto
    /// plano en lugar de SQL Server.
    /// </summary>
    /// <remarks>
    /// Estas dos líneas son toda la diferencia entre los dos almacenes del sistema. La capa
    /// Servicios recibe <see cref="IEntidadGubernamentalRepositorio"/> exactamente igual que
    /// recibe <see cref="IEmpleadoRepositorio"/>, y no tiene forma de distinguir que detrás de
    /// una hay un motor relacional y detrás de la otra un archivo de 188 líneas. Migrar este
    /// catálogo a SQL Server el día de mañana es cambiar el tipo de la derecha.
    /// </remarks>
    private static void AgregarAlmacenamientoEnArchivoPlano(
        IServiceCollection servicios,
        IConfiguration configuracion)
    {
        // La ruta se lee de la configuración, con la ruta de salida del build por omisión.
        // Misma norma que la cadena de conexión: en código va el nombre de la clave, no el valor.
        servicios.AddSingleton(new OpcionesArchivoEntidadesGubernamentales(
            configuracion[OpcionesArchivoEntidadesGubernamentales.CLAVE_CONFIGURACION]));

        // Singleton, y no Scoped como los repositorios de EF Core. No es una inconsistencia:
        // es lo que la implementación exige. El semáforo que serializa las escrituras y el
        // caché del archivo solo sirven si TODAS las peticiones comparten la misma instancia.
        // Registrado como Scoped, cada petición traería su propio semáforo, cada semáforo
        // estaría libre, y dos altas simultáneas escribirían el archivo a la vez.
        //
        // Ser Singleton es seguro aquí precisamente porque la clase está diseñada para ello:
        // su estado mutable está protegido por el semáforo. El repositorio de SQL Server no
        // podría serlo, porque envuelve un DbContext, que no es seguro para uso concurrente.
        servicios.AddSingleton<IEntidadGubernamentalRepositorio, EntidadGubernamentalRepositorioArchivo>();
    }
}
