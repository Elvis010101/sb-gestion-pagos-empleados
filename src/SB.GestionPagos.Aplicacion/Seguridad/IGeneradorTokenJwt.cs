using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Aplicacion.Seguridad;

/// <summary>
/// Emite el token de acceso de un usuario ya autenticado.
/// </summary>
/// <remarks>
/// Se declara aquí y se implementa en Infraestructura: es el Principio de Inversión de
/// Dependencias aplicado a un servicio técnico. La Aplicación dice QUÉ necesita —"un token
/// para este usuario"— y no sabe nada de claves de firma, algoritmos ni bibliotecas.
///
/// Es síncrona a propósito: firmar un JWT es cálculo en memoria, sin entrada ni salida.
/// Devolver <c>Task</c> "por si acaso" obligaría a todos los llamadores a usar
/// <c>await</c> sobre una operación que nunca espera.
/// </remarks>
public interface IGeneradorTokenJwt
{
    TokenGenerado Generar(Usuario usuario);
}
