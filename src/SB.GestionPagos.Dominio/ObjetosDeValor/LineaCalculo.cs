namespace SB.GestionPagos.Dominio.ObjetosDeValor;

/// <summary>
/// Un renglón del desglose del pago semanal: un concepto y el monto que aporta al total.
/// </summary>
/// <remarks>
/// Es un <c>record</c> posicional porque es un objeto de valor: dos líneas con el mismo
/// concepto y el mismo monto son la misma línea. Esa igualdad estructural es la que hace
/// que las pruebas puedan comparar desgloses completos sin escribir comparadores a mano.
/// </remarks>
public sealed record LineaCalculo(string Concepto, decimal Monto);
