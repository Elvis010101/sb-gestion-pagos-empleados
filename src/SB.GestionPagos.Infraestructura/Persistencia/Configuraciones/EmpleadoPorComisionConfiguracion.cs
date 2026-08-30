using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// Columnas propias del empleado por comisión.
/// </summary>
/// <remarks>
/// Las dos columnas se nombran explícitamente para COMPARTIRLAS con
/// <see cref="EmpleadoAsalariadoPorComisionConfiguracion"/>: son el mismo dato de negocio
/// con el mismo tipo, y en TPH dos subtipos hermanos pueden ocupar la misma columna.
/// Sin nombrarlas, EF crearía una segunda pareja llamada
/// <c>EmpleadoAsalariadoPorComision_VentasBrutas</c> y <c>..._TarifaComision</c>.
/// </remarks>
internal sealed class EmpleadoPorComisionConfiguracion : IEntityTypeConfiguration<EmpleadoPorComision>
{
    public void Configure(EntityTypeBuilder<EmpleadoPorComision> constructorDeEntidad)
    {
        constructorDeEntidad
            .Property(empleado => empleado.VentasBrutas)
            .HasColumnName(EsquemaBaseDeDatos.COLUMNA_VENTAS_BRUTAS)
            .HasPrecision(EsquemaBaseDeDatos.PRECISION_MONETARIA, EsquemaBaseDeDatos.ESCALA_MONETARIA);

        // Fracción, no dinero: cuatro decimales. Con la escala monetaria, una comisión del
        // 7,5 % se guardaría como 8 % y el pago semanal quedaría mal para siempre.
        constructorDeEntidad
            .Property(empleado => empleado.TarifaComision)
            .HasColumnName(EsquemaBaseDeDatos.COLUMNA_TARIFA_COMISION)
            .HasPrecision(
                EsquemaBaseDeDatos.PRECISION_TARIFA_COMISION,
                EsquemaBaseDeDatos.ESCALA_TARIFA_COMISION);
    }
}
