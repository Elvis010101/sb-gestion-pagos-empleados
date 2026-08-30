namespace SB.GestionPagos.Infraestructura.Persistencia;

/// <summary>
/// Nombres, dimensiones y precisiones del esquema relacional.
/// </summary>
/// <remarks>
/// Existe por la norma de nomenclatura de SB que prohíbe los números y las cadenas mágicas:
/// un <c>HasPrecision(18, 2)</c> suelto en cinco archivos es cinco oportunidades de escribir
/// uno distinto sin que nada avise.
///
/// Es <c>internal</c> porque describe un detalle de la implementación de persistencia. Ni el
/// Dominio ni la Aplicación deben poder nombrar una tabla.
///
/// Lo que NO está aquí son las longitudes de los campos de texto: esas viven en
/// <c>Aplicacion.Validaciones.LongitudMaxima</c> y se leen desde allí. Duplicarlas
/// produciría el clásico desajuste de un validador que acepta 200 caracteres contra una
/// columna que admite 100, con el error apareciendo recién al guardar.
/// </remarks>
internal static class EsquemaBaseDeDatos
{
    internal const string TABLA_EMPLEADOS = "Empleados";

    internal const string TABLA_USUARIOS = "Usuarios";

    // -----------------------------------------------------------------------
    // Herencia de tabla única (TPH)
    // -----------------------------------------------------------------------

    /// <summary>Columna que dice, en cada fila, de qué tipo de empleado se trata.</summary>
    internal const string COLUMNA_DISCRIMINADORA_TIPO_EMPLEADO = "TipoEmpleado";

    internal const int LONGITUD_COLUMNA_DISCRIMINADORA = 40;

    // Los valores del discriminador se declaran de forma explícita y NO se deja que EF use
    // el nombre de la clase. Si mañana alguien renombra `EmpleadoPorHoras`, el valor guardado
    // en las filas existentes seguiría diciendo "PorHoras" y la aplicación dejaría de poder
    // materializarlas. Un dato persistido no puede depender de un nombre de código.
    internal const string DISCRIMINADOR_ASALARIADO = "Asalariado";

    internal const string DISCRIMINADOR_POR_HORAS = "PorHoras";

    internal const string DISCRIMINADOR_POR_COMISION = "PorComision";

    internal const string DISCRIMINADOR_ASALARIADO_POR_COMISION = "AsalariadoPorComision";

    // -----------------------------------------------------------------------
    // Columnas compartidas entre subtipos hermanos
    // -----------------------------------------------------------------------

    // `EmpleadoPorComision` y `EmpleadoAsalariadoPorComision` declaran las mismas dos
    // propiedades. Por omisión, EF les daría una columna a cada uno
    // (`VentasBrutas` y `EmpleadoAsalariadoPorComision_VentasBrutas`), duplicando el dato.
    // Nombrarlas explícitamente hace que compartan una sola columna: mismo significado,
    // mismo tipo, misma precisión.
    internal const string COLUMNA_VENTAS_BRUTAS = "VentasBrutas";

    internal const string COLUMNA_TARIFA_COMISION = "TarifaComision";

    // -----------------------------------------------------------------------
    // Precisión numérica
    // -----------------------------------------------------------------------

    /// <summary>Dígitos totales de una columna monetaria: hasta 9.999.999.999.999.999,99.</summary>
    internal const int PRECISION_MONETARIA = 18;

    /// <summary>Centavos. El dinero de esta aplicación no admite fracciones más finas.</summary>
    internal const int ESCALA_MONETARIA = 2;

    /// <summary>
    /// La tarifa de comisión NO es dinero: es una fracción entre 0 y 1.
    /// </summary>
    /// <remarks>
    /// Con la escala monetaria (2 decimales), una comisión del 7,5 % —<c>0.075</c>— se
    /// guardaría como <c>0.08</c> y el pago semanal saldría mal para siempre. Cuatro
    /// decimales permiten expresar hasta la centésima de punto porcentual (<c>0.0001</c>).
    /// </remarks>
    internal const int PRECISION_TARIFA_COMISION = 5;

    internal const int ESCALA_TARIFA_COMISION = 4;

    /// <summary>
    /// Horas trabajadas: el Dominio las acota a 168 por semana, así que cuatro dígitos
    /// enteros sobran, y los dos decimales cubren las jornadas parciales (media hora).
    /// </summary>
    internal const int PRECISION_HORAS_TRABAJADAS = 6;

    internal const int ESCALA_HORAS_TRABAJADAS = 2;

    /// <summary>
    /// Espacio para el hash de la contraseña. BCrypt produce exactamente 60 caracteres;
    /// el margen extra permite migrar a otro algoritmo sin alterar el esquema.
    /// </summary>
    internal const int LONGITUD_HASH_CONTRASENA = 100;

    // -----------------------------------------------------------------------
    // Índices
    // -----------------------------------------------------------------------

    // Se nombran a mano en vez de aceptar el nombre autogenerado por EF, porque estos
    // nombres aparecen en el script SQL entregable y en los planes de ejecución: quien
    // diagnostique una consulta lenta tiene que poder leer a qué columna corresponde.
    internal const string INDICE_EMPLEADOS_NUMERO_SEGURO_SOCIAL = "IX_Empleados_NumeroSeguroSocial";

    internal const string INDICE_EMPLEADOS_DEPARTAMENTO = "IX_Empleados_Departamento";

    internal const string INDICE_EMPLEADOS_ESTADO = "IX_Empleados_Estado";

    internal const string INDICE_EMPLEADOS_APELLIDO_PATERNO = "IX_Empleados_ApellidoPaterno";

    internal const string INDICE_USUARIOS_NOMBRE_USUARIO = "IX_Usuarios_NombreUsuario";
}
