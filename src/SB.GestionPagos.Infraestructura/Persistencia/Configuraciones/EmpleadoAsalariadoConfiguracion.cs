using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.GestionPagos.Dominio.Entidades;

namespace SB.GestionPagos.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// Columnas propias del empleado asalariado.
/// </summary>
/// <remarks>
/// No repite tabla, clave ni índices: los hereda de <see cref="EmpleadoConfiguracion"/>,
/// porque en TPH el subtipo vive en la misma tabla que su base.
/// </remarks>
internal sealed class EmpleadoAsalariadoConfiguracion : IEntityTypeConfiguration<EmpleadoAsalariado>
{
    public void Configure(EntityTypeBuilder<EmpleadoAsalariado> constructorDeEntidad)
    {
        // Importe monetario: `decimal` en C# y `decimal(18,2)` en SQL Server. Sin esta
        // precisión explícita, EF advierte y el proveedor cae en `decimal(18,2)` por omisión
        // de todos modos; declararla deja el contrato escrito en vez de heredado de un
        // comportamiento por omisión que podría cambiar de versión.
        constructorDeEntidad
            .Property(empleado => empleado.SalarioSemanal)
            .HasPrecision(EsquemaBaseDeDatos.PRECISION_MONETARIA, EsquemaBaseDeDatos.ESCALA_MONETARIA);
    }
}
