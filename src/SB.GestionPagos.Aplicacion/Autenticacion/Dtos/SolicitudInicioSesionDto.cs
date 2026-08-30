namespace SB.GestionPagos.Aplicacion.Autenticacion.Dtos;

/// <summary>
/// Credenciales enviadas al iniciar sesión.
/// </summary>
/// <remarks>
/// Es el ejemplo más claro de por qué un DTO no es la entidad: la entidad <c>Usuario</c>
/// guarda un HASH y jamás una contraseña en claro, mientras que este contrato transporta la
/// contraseña tal como la escribió la persona. Son dos conceptos distintos que solo se tocan
/// dentro del servicio de autenticación, y ni este tipo ni sus valores deben aparecer nunca
/// en el log (RNF-08 con el matiz de seguridad del A6).
/// </remarks>
public sealed record SolicitudInicioSesionDto(string NombreUsuario, string Contrasena);
