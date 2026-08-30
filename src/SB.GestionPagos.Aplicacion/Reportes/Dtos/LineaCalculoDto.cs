namespace SB.GestionPagos.Aplicacion.Reportes.Dtos;

/// <summary>
/// Un renglón del desglose del pago semanal, tal como se publica.
/// </summary>
/// <param name="Concepto">Nombre del concepto, por ejemplo "Horas extra".</param>
/// <param name="Monto">Lo que ese concepto aporta al pago semanal.</param>
/// <remarks>
/// Es el espejo de <c>LineaCalculo</c> del Dominio. Se duplica en vez de exponer el objeto de
/// valor directamente para que el contrato publicado no quede amarrado a un tipo del Dominio:
/// aquel puede ganar comportamiento o cambiar de forma sin romper a ningún cliente.
/// </remarks>
public sealed record LineaCalculoDto(string Concepto, decimal Monto);
