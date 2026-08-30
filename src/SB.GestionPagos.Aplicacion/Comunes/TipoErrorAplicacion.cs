namespace SB.GestionPagos.Aplicacion.Comunes;

/// <summary>
/// Clasificación del fallo de un caso de uso.
/// </summary>
/// <remarks>
/// No es un catálogo de mensajes: es la información mínima que la capa Api necesita para
/// elegir un código de estado HTTP sin conocer el caso de uso. Cada miembro existe porque
/// se traduce a un código distinto; si dos miembros se tradujeran al mismo, sobraría uno.
/// </remarks>
public enum TipoErrorAplicacion
{
    /// <summary>La operación salió bien. Es el valor de todo resultado exitoso.</summary>
    Ninguno = 0,

    /// <summary>El recurso solicitado no existe. La Api lo traduce a 404.</summary>
    NoEncontrado = 1,

    /// <summary>
    /// El estado actual de los datos impide la operación: por ejemplo, dar de alta un
    /// empleado con un número de seguro social que ya está registrado. Se traduce a 409.
    /// </summary>
    Conflicto = 2,

    /// <summary>
    /// Una regla de negocio rechaza la operación aunque el dato de entrada esté bien
    /// formado. Se traduce a 400.
    /// </summary>
    ReglaDeNegocio = 3,

    /// <summary>
    /// El usuario o la contraseña no corresponden. Se traduce a 401.
    /// </summary>
    CredencialesInvalidas = 4
}
