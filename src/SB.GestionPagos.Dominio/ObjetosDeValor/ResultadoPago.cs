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

        // Se copia a una vista de solo lectura para que quien haya construido el arreglo
        // no pueda seguir modificando el desglose después de crear el resultado.
        Lineas = Array.AsReadOnly(lineas);
    }

    public IReadOnlyList<LineaCalculo> Lineas { get; }

    public decimal Total => Lineas.Sum(linea => linea.Monto);
}
