using Microsoft.EntityFrameworkCore;
using SB.GestionPagos.Dominio.Entidades;
using SB.GestionPagos.Infraestructura.Persistencia.Semilla;

namespace SB.GestionPagos.Infraestructura.Persistencia;

/// <summary>
/// Sesión de trabajo contra la base de datos de empleados y usuarios.
/// </summary>
/// <remarks>
/// Un <c>DbContext</c> es dos cosas a la vez: el modelo (qué clase corresponde a qué tabla)
/// y una unidad de trabajo con seguimiento de cambios. Por eso se registra con alcance
/// <c>Scoped</c> y no como singleton: no es seguro para uso concurrente y su vida debe
/// terminar con la petición HTTP.
///
/// Vive en Infraestructura y ninguna capa interior lo nombra. Los servicios ven
/// <c>IEmpleadoRepositorio</c>, declarado en el Dominio; que detrás haya EF Core, Dapper o
/// un archivo es información que no les llega.
/// </remarks>
public sealed class GestionPagosDbContext : DbContext
{
    public GestionPagosDbContext(DbContextOptions<GestionPagosDbContext> opciones)
        : base(opciones)
    {
    }

    /// <summary>
    /// Punto de entrada a la jerarquía completa de empleados.
    /// </summary>
    /// <remarks>
    /// Hay un solo <c>DbSet</c> para los cuatro tipos porque hay una sola tabla (TPH).
    /// Consultar un tipo concreto se hace con <c>OfType&lt;EmpleadoPorHoras&gt;()</c>, que
    /// EF traduce a un filtro sobre la columna discriminadora.
    ///
    /// Se expone como propiedad calculada sobre <c>Set&lt;T&gt;()</c> y no como propiedad
    /// automática porque así no hace falta un <c>= null!</c> para callar al análisis de
    /// nulabilidad: el valor no lo asigna EF por reflexión, lo devuelve el propio contexto.
    /// </remarks>
    public DbSet<Empleado> Empleados => Set<Empleado>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder constructorDeModelo)
    {
        base.OnModelCreating(constructorDeModelo);

        // El mapeo NO se escribe aquí. Cada entidad tiene su propia clase de configuración
        // y este método solo las recoge del ensamblado. La alternativa —encadenar todas las
        // llamadas dentro de OnModelCreating— produce un método de cientos de líneas donde
        // cualquier cambio de una entidad obliga a leer el mapeo de todas las demás.
        constructorDeModelo.ApplyConfigurationsFromAssembly(typeof(GestionPagosDbContext).Assembly);

        // La semilla va aparte del mapeo a propósito: son dos preguntas distintas. El mapeo
        // dice CÓMO se guarda un empleado; la semilla dice QUÉ empleados existen al arrancar.
        DatosSemilla.Aplicar(constructorDeModelo);
    }
}
