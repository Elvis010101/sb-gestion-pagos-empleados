namespace SB.GestionPagos.Infraestructura.DatosPlanos;

/// <summary>
/// Ubicación del archivo plano que respalda el catálogo de entidades gubernamentales.
/// </summary>
/// <remarks>
/// Existe por el mismo motivo por el que la cadena de conexión de SQL Server vive en el
/// appsettings y no en código: una ruta incrustada en una clase es un valor de despliegue
/// disfrazado de constante. Lo que sí vive aquí es el NOMBRE de la clave y la ruta relativa
/// por omisión, que coinciden con el destino al que el .csproj copia el archivo en el build.
/// </remarks>
internal sealed class OpcionesArchivoEntidadesGubernamentales
{
    /// <summary>Clave del appsettings que permite mover el archivo sin recompilar.</summary>
    internal const string CLAVE_CONFIGURACION = "AlmacenamientoArchivoPlano:RutaEntidadesGubernamentales";

    /// <summary>
    /// Ruta relativa al directorio de salida. Debe coincidir con el <c>Content</c> del
    /// .csproj que copia el archivo en el build; si divergen, la aplicación no arranca.
    /// </summary>
    internal const string RUTA_RELATIVA_PREDETERMINADA = "DatosPlanos/entidades-gubernamentales.txt";

    internal OpcionesArchivoEntidadesGubernamentales(string? rutaConfigurada)
    {
        string ruta = string.IsNullOrWhiteSpace(rutaConfigurada)
            ? RUTA_RELATIVA_PREDETERMINADA
            : rutaConfigurada.Trim();

        // Una ruta relativa se resuelve contra el directorio de salida del ensamblado y NO
        // contra el directorio de trabajo del proceso. El directorio de trabajo depende de
        // desde dónde se lanzó la aplicación —no es el mismo con `dotnet run`, desde el IDE
        // o desde un servicio de Windows—, y esa diferencia se manifiesta como un archivo
        // "que no existe" solo en algunos entornos.
        RutaArchivo = Path.IsPathRooted(ruta)
            ? Path.GetFullPath(ruta)
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ruta));
    }

    /// <summary>Ruta absoluta ya resuelta del archivo de datos.</summary>
    internal string RutaArchivo { get; }
}
