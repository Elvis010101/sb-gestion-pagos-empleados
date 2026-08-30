using FluentValidation;

namespace SB.GestionPagos.Aplicacion.Validaciones;

/// <summary>
/// Reglas de validación reutilizables, escritas como extensiones de FluentValidation.
/// </summary>
/// <remarks>
/// Los doce validadores de esta capa comparten las mismas cuatro o cinco reglas. Extraerlas
/// aquí evita repetirlas —y sobre todo evita que se repitan MAL: que un validador exija el
/// nombre y otro lo deje pasar vacío. Los mensajes se escriben en español porque llegan tal
/// cual a la interfaz.
/// </remarks>
public static class ReglasDeValidacion
{
    private const decimal MONTO_MINIMO = 0m;

    /// <summary>
    /// Exige que el texto venga, no sea solo espacios y no exceda su longitud máxima.
    /// </summary>
    public static IRuleBuilderOptions<T, string> TextoObligatorio<T>(
        this IRuleBuilder<T, string> constructorDeRegla,
        int longitudMaxima)
        => constructorDeRegla
            .NotEmpty()
            .WithMessage("El campo '{PropertyName}' es obligatorio.")
            .MaximumLength(longitudMaxima)
            .WithMessage("El campo '{PropertyName}' no puede exceder {MaxLength} caracteres.");

    /// <summary>
    /// Aplica la cota de longitud solo si el texto vino. Es la regla de los campos opcionales,
    /// como los criterios de búsqueda: no enviarlos significa "no filtrar", no "dato inválido".
    /// </summary>
    public static IRuleBuilderOptions<T, string?> TextoOpcional<T>(
        this IRuleBuilder<T, string?> constructorDeRegla,
        int longitudMaxima)
        => constructorDeRegla
            .MaximumLength(longitudMaxima)
            .WithMessage("El campo '{PropertyName}' no puede exceder {MaxLength} caracteres.");

    /// <summary>Rechaza importes negativos. Espeja la guarda <c>NoNegativo</c> del Dominio.</summary>
    public static IRuleBuilderOptions<T, decimal> MontoNoNegativo<T>(
        this IRuleBuilder<T, decimal> constructorDeRegla)
        => constructorDeRegla
            .GreaterThanOrEqualTo(MONTO_MINIMO)
            .WithMessage("El campo '{PropertyName}' no puede ser negativo.");

    /// <summary>
    /// Rechaza los valores fuera del rango indicado.
    /// </summary>
    /// <remarks>
    /// Recibe los límites como parámetros a propósito: quien la usa pasa las constantes del
    /// Dominio (por ejemplo <c>EmpleadoPorHoras.HORAS_MAXIMAS_SEMANALES</c>), de modo que el
    /// validador del DTO y la invariante de la entidad no pueden desincronizarse.
    /// </remarks>
    public static IRuleBuilderOptions<T, decimal> EnRangoInclusivo<T>(
        this IRuleBuilder<T, decimal> constructorDeRegla,
        decimal valorMinimo,
        decimal valorMaximo)
        => constructorDeRegla
            .InclusiveBetween(valorMinimo, valorMaximo)
            .WithMessage("El campo '{PropertyName}' debe estar entre {From} y {To}.");

    /// <summary>Igual que la anterior, para los criterios enteros de la paginación.</summary>
    public static IRuleBuilderOptions<T, int> EnRangoInclusivo<T>(
        this IRuleBuilder<T, int> constructorDeRegla,
        int valorMinimo,
        int valorMaximo)
        => constructorDeRegla
            .InclusiveBetween(valorMinimo, valorMaximo)
            .WithMessage("El campo '{PropertyName}' debe estar entre {From} y {To}.");

    /// <summary>
    /// Exige que el valor recibido corresponda a un miembro declarado de la enumeración.
    /// </summary>
    /// <remarks>
    /// Sin esta regla, un cliente puede enviar <c>estado = 99</c> y el modelo lo aceptaría:
    /// en .NET un enum es un entero con nombres, y cualquier entero cabe en él.
    /// </remarks>
    public static IRuleBuilderOptions<T, TEnumeracion> ValorDeEnumeracionDefinido<T, TEnumeracion>(
        this IRuleBuilder<T, TEnumeracion> constructorDeRegla)
        => constructorDeRegla
            .IsInEnum()
            .WithMessage("El campo '{PropertyName}' no admite el valor recibido.");
}
