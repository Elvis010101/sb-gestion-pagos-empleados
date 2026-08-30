using SB.GestionPagos.Aplicacion.Seguridad;
using AlgoritmoBCrypt = BCrypt.Net.BCrypt;

namespace SB.GestionPagos.Infraestructura.Seguridad;

/// <summary>
/// Implementación de <see cref="IServicioHash"/> con BCrypt.
/// </summary>
/// <remarks>
/// BCrypt y no SHA-256 a secas. SHA-256 está diseñado para ser RÁPIDO —es lo que se quiere
/// de un hash de integridad— y esa velocidad es precisamente el problema cuando lo que se
/// protege es una contraseña: una tarjeta gráfica de consumo calcula del orden de miles de
/// millones de SHA-256 por segundo, así que probar el diccionario entero contra una base de
/// hashes filtrada es cuestión de minutos.
///
/// BCrypt es deliberadamente lento y su lentitud es CONFIGURABLE mediante el factor de
/// trabajo. Además incorpora la sal dentro del propio resultado, de modo que dos usuarios
/// con la misma contraseña no comparten hash y una tabla precalculada (rainbow table) no
/// sirve de nada.
/// </remarks>
internal sealed class ServicioHashBCrypt : IServicioHash
{
    /// <summary>
    /// Factor de trabajo: el costo es 2^12 = 4.096 iteraciones internas.
    /// </summary>
    /// <remarks>
    /// Es el parámetro que decide cuánto tarda un intento. Doce deja el cálculo en el orden
    /// de las décimas de segundo en hardware actual: imperceptible para quien inicia sesión
    /// una vez, y demoledor para quien quiere probar millones de combinaciones.
    ///
    /// El valor va aquí y no en configuración porque el hash almacenado LLEVA ESCRITO dentro
    /// el factor con que se creó (el prefijo <c>$2a$12$</c>). Subirlo mañana a 13 no invalida
    /// los hashes existentes: los antiguos se siguen verificando con 12 y los nuevos se
    /// generan con 13.
    /// </remarks>
    private const int FACTOR_DE_TRABAJO = 12;

    public string GenerarHash(string contrasenaEnClaro)
        => AlgoritmoBCrypt.HashPassword(contrasenaEnClaro, FACTOR_DE_TRABAJO);

    /// <summary>
    /// Compara una contraseña contra el hash almacenado.
    /// </summary>
    /// <remarks>
    /// No se "deshashea" nada: se vuelve a aplicar el algoritmo a la contraseña recibida
    /// usando la sal y el factor de trabajo que el propio hash guardado trae dentro, y se
    /// comparan los dos resultados. Esa comparación la hace la biblioteca en tiempo
    /// constante, para que la duración de la respuesta no revele cuántos caracteres del
    /// hash coincidían.
    ///
    /// Un hash con formato inválido —una fila corrupta, un dato migrado a mano— se trata
    /// como credencial incorrecta y no como error del servidor: un 500 en el login le
    /// confirmaría al atacante que ese usuario existe y que su registro es anómalo.
    /// </remarks>
    public bool Verificar(string contrasenaEnClaro, string hashAlmacenado)
    {
        try
        {
            return AlgoritmoBCrypt.Verify(contrasenaEnClaro, hashAlmacenado);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
