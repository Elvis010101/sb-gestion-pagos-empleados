namespace SB.GestionPagos.Aplicacion.Validaciones;

/// <summary>
/// Longitud máxima admitida para cada campo de texto de la Api.
/// </summary>
/// <remarks>
/// Están centralizadas y nombradas —en vez de escritas dentro de cada validador— por la
/// norma de nomenclatura de SB que prohíbe los números mágicos, y porque en el Bloque 4
/// la configuración de EF Core leerá estas mismas constantes para dimensionar las columnas.
/// Un solo número por campo evita el escenario clásico: el validador acepta 200 caracteres
/// y la columna admite 100, así que el error aparece recién al guardar.
/// </remarks>
public static class LongitudMaxima
{
    public const int PRIMER_NOMBRE = 100;

    public const int APELLIDO_PATERNO = 100;

    /// <summary>
    /// El documento de la prueba no fija el formato del número de seguro social, así que
    /// no se impone ningún patrón: solo una cota de longitud.
    /// </summary>
    public const int NUMERO_SEGURO_SOCIAL = 20;

    public const int DEPARTAMENTO = 100;

    public const int NOMBRE_ENTIDAD_GUBERNAMENTAL = 200;

    public const int CATEGORIA = 100;

    public const int PODER_DEL_ESTADO = 100;

    public const int SECTOR = 150;

    public const int NOMBRE_USUARIO = 50;

    /// <summary>
    /// Cota de la contraseña recibida en el inicio de sesión. No es una regla de robustez
    /// —eso corresponde al alta de usuarios— sino un límite defensivo: sin él, una petición
    /// con una contraseña de varios megabytes obligaría al algoritmo de hash a procesarla.
    /// </summary>
    public const int CONTRASENA = 128;
}
