using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// Columnas propias del empleado asalariado por comisión.
/// </summary>
/// <remarks>
/// Comparte <c>VentasBrutas</c> y <c>TarifaComision</c> con
/// <see cref="EmpleadoPorComisionConfiguracion"/>. Las precisiones tienen que coincidir
/// exactamente entre ambas configuraciones: si difirieran, EF rechaza el modelo al
/// construirlo, y esa validación es justamente la red que impide que las dos definiciones
/// se separen sin que nadie lo note.
/// </remarks>
internal sealed class EmpleadoAsalariadoPorComisionConfiguracion
    : IEntityTypeConfiguration<EmpleadoAsalariadoPorComision>
{
    public void Configure(EntityTypeBuilder<EmpleadoAsalariadoPorComision> constructorDeEntidad)
    {
        constructorDeEntidad
            .Property(empleado => empleado.VentasBrutas)
            .HasColumnName(EsquemaBaseDeDatos.COLUMNA_VENTAS_BRUTAS)
            .HasPrecision(EsquemaBaseDeDatos.PRECISION_MONETARIA, EsquemaBaseDeDatos.ESCALA_MONETARIA);

        constructorDeEntidad
            .Property(empleado => empleado.TarifaComision)
            .HasColumnName(EsquemaBaseDeDatos.COLUMNA_TARIFA_COMISION)
            .HasPrecision(
                EsquemaBaseDeDatos.PRECISION_TARIFA_COMISION,
                EsquemaBaseDeDatos.ESCALA_TARIFA_COMISION);

        // Esta sí es exclusiva del tipo: no la comparte con nadie.
        constructorDeEntidad
            .Property(empleado => empleado.SalarioBase)
            .HasPrecision(EsquemaBaseDeDatos.PRECISION_MONETARIA, EsquemaBaseDeDatos.ESCALA_MONETARIA);
    }
}
