namespace SB.GestionPagos.Api.Seguridad;

/// <summary>
/// Límites de frecuencia aplicados a los endpoints que los necesitan.
/// </summary>
/// <remarks>
/// Solo el inicio de sesión tiene límite en este bloque, y es a propósito: es el único
/// endpoint que se puede invocar sin credenciales, y por lo tanto el único al que se le
/// puede disparar sin coste. El resto del sistema exige un token válido antes de hacer
/// cualquier trabajo.
/// </remarks>
public static class PoliticasLimiteDePeticiones
{
    /// <summary>Política que protege el endpoint de inicio de sesión.</summary>
    public const string INICIO_SESION = "InicioSesion";

    /// <summary>
    /// Intentos de inicio de sesión permitidos por ventana y por origen.
    /// </summary>
    /// <remarks>
    /// Cinco cubre de sobra a una persona que se equivoca al teclear y ahoga a un programa
    /// que prueba contraseñas: con este límite, un diccionario de cien mil palabras
    /// necesitaría casi catorce días desde una misma dirección.
    /// </remarks>
    public const int INTENTOS_PERMITIDOS_POR_VENTANA = 5;

    /// <summary>Duración de la ventana, en minutos.</summary>
    public const int MINUTOS_DE_VENTANA = 1;

    /// <summary>
    /// Partición usada cuando no se puede determinar la dirección del cliente.
    /// </summary>
    /// <remarks>
    /// Todos los casos sin dirección comparten un único cubo. Es lo conservador: la
    /// alternativa —dejarlos pasar sin límite— convertiría "ocultar la IP" en la forma de
    /// saltarse la protección.
    /// </remarks>
    public const string ORIGEN_DESCONOCIDO = "origen-desconocido";
}
