using SB.GestionPagos.Dominio.Excepciones;

namespace SB.GestionPagos.Dominio.ObjetosDeValor;

/// <summary>
/// Resultado del cálculo del pago semanal: el desglose y el total que se deriva de él.
/// </summary>
/// <remarks>
/// El RF-06 pide un reporte que detalle los cálculos según el tipo de contrato, no solo el
/// total. Al hacer que <see cref="Total"/> se derive de <see cref="Lineas"/> en lugar de
/// almacenarse aparte, el total y el desglose no pueden contradecirse.
/// </remarks>
public sealed class ResultadoPago
{
    public ResultadoPago(params LineaCalculo[] lineas)
    {
        if (lineas is null || lineas.Length == 0)
        {
            throw new ExcepcionValorRequerido(nameof(Lineas));
        }

        // Se COPIA el arreglo y luego se envuelve. Array.AsReadOnly por sí solo no copia
        // nada: devuelve una vista sobre el MISMO arreglo, de modo que quien conserve la
        // referencia original podría seguir alterando el desglose de un resultado ya
        // construido. Con la sintaxis params el arreglo lo crea el compilador y nadie más lo
        // referencia, pero la garantía no puede depender de cómo se invoque el constructor.
        Lineas = Array.AsReadOnly(lineas.ToArray());
    }

    public IReadOnlyList<LineaCalculo> Lineas { get; }

    public decimal Total => Lineas.Sum(linea => linea.Monto);
}
