namespace SB.GestionPagos.Api.Seguridad;

/// <summary>
/// Límites de frecuencia aplicados a los endpoints que los necesitan.
/// </summary>
/// <remarks>
/// Hay dos capas, y las dos actúan sobre la misma petición de inicio de sesión:
///
/// 1. Un límite GENERAL por dirección de origen, que cubre todos los endpoints y protege al
///    servidor de que un solo cliente —un bucle mal escrito o un ataque— consuma toda su
///    capacidad.
/// 2. Un límite ESTRICTO sobre el inicio de sesión, que es el único endpoint invocable sin
///    credenciales y por tanto el único al que se le puede disparar sin coste. Aquí lo que se
///    frena no es la saturación, es la prueba de contraseñas por fuerza bruta.
///
/// Un límite general lo bastante estricto para frenar fuerza bruta rompería el uso normal de
/// la aplicación; uno lo bastante amplio para no estorbar dejaría pasar cientos de intentos
/// de contraseña por minuto. Por eso son dos y no uno.
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
    /// Peticiones que un mismo origen puede disparar de golpe contra toda la API.
    /// </summary>
    /// <remarks>
    /// Es la capacidad del cubo, no el ritmo. Existe porque una sola pantalla del frontend
    /// puede lanzar varias peticiones simultáneas al cargar, y un límite sin margen para
    /// ráfagas rechazaría el uso normal de la aplicación.
    /// </remarks>
    public const int PETICIONES_EN_RAFAGA_POR_ORIGEN = 100;

    /// <summary>Fichas que se reponen al cubo en cada periodo.</summary>
    public const int PETICIONES_REPUESTAS_POR_PERIODO = 50;

    /// <summary>
    /// Cada cuánto se reponen las fichas, en segundos.
    /// </summary>
    /// <remarks>
    /// Con los valores actuales el ritmo sostenido es de 300 peticiones por minuto y por
    /// origen, con margen para ráfagas de 100. Muy por encima de lo que produce una persona
    /// usando la aplicación, y muy por debajo de lo que hace falta para saturar el servidor.
    /// </remarks>
    public const int SEGUNDOS_DE_REPOSICION = 10;

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
