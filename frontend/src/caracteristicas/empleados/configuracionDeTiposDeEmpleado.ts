import { z } from 'zod';

import { actualizarEmpleadoDeTipo, crearEmpleadoDeTipo } from './empleadosApi';
import {
  EstadoEmpleado,
  type ActualizarEmpleadoAsalariadoDto,
  type ActualizarEmpleadoAsalariadoPorComisionDto,
  type ActualizarEmpleadoPorComisionDto,
  type ActualizarEmpleadoPorHorasDto,
  type CrearEmpleadoAsalariadoDto,
  type CrearEmpleadoAsalariadoPorComisionDto,
  type CrearEmpleadoPorComisionDto,
  type CrearEmpleadoPorHorasDto,
  type EmpleadoDto,
  type TipoEmpleado,
} from './tipos';

/**
 * Longitudes máximas de los campos de texto. Son las mismas de `LongitudMaxima` en la capa
 * Aplicación del backend.
 *
 * Están replicadas y no deducidas porque el navegador no puede leer las constantes de C#. La
 * duplicación es consciente y acotada: si un día divergen, el servidor sigue rechazando lo
 * que no cumple —la validación del cliente nunca es la que manda—, y el usuario vería el
 * error un viaje de red más tarde en lugar de al instante.
 */
const LONGITUD_MAXIMA = {
  PRIMER_NOMBRE: 100,
  APELLIDO_PATERNO: 100,
  NUMERO_SEGURO_SOCIAL: 20,
  DEPARTAMENTO: 100,
} as const;

/** Límites que impone el Dominio. Espejo de las constantes de las entidades. */
const MONTO_MINIMO = 0;
const HORAS_MINIMAS_SEMANALES = 0;
const HORAS_MAXIMAS_SEMANALES = 168;
const TARIFA_COMISION_MINIMA = 0;
const TARIFA_COMISION_MAXIMA = 1;

/** Incremento del control numérico: dos decimales para el dinero, cuatro para la fracción. */
const PASO_MONETARIO = '0.01';
const PASO_FRACCION = '0.0001';

/**
 * Un campo propio de un tipo de contrato, descrito como DATO.
 *
 * Que sea un dato y no código es lo que permite que el formulario dibuje los campos
 * recorriendo una lista, en lugar de preguntar de qué tipo es el empleado.
 */
export interface DefinicionDeCampoDeContrato {
  /** Nombre de la propiedad en el DTO del backend. */
  nombre: string;

  etiqueta: string;

  /** Aclaración bajo el campo, para lo que el rótulo no alcanza a explicar. */
  ayuda?: string;

  paso: string;
  valorMinimo: number;
  valorMaximo?: number;
}

/**
 * Valores del formulario.
 *
 * Los campos del contrato viven agrupados bajo `camposDeContrato` y como TEXTO, no como
 * número. Son texto porque un `<input>` siempre entrega texto, y forzar la conversión antes
 * de validar convierte un campo vacío en `NaN`, que produce el mensaje inútil "se esperaba un
 * número, se recibió NaN" en lugar de "el salario semanal es obligatorio". La conversión se
 * hace una sola vez, al construir la solicitud.
 */
export interface ValoresFormularioEmpleado {
  primerNombre: string;
  apellidoPaterno: string;
  numeroSeguroSocial: string;
  departamento: string;
  estado: EstadoEmpleado;
  camposDeContrato: Record<string, string>;
}

/**
 * Todo lo que el sistema necesita saber sobre un tipo de contrato, en un solo objeto.
 *
 * Aquí está la respuesta a "cómo se hace un formulario dinámico sin un `if` gigante": cada
 * tipo declara qué campos tiene, cómo se convierten en la solicitud de su endpoint y cómo se
 * leen de vuelta desde un empleado existente. El formulario no conoce ninguno de los cuatro
 * tipos; conoce esta interfaz.
 *
 * Es la misma idea que sostiene el backend. Allá, `Empleado` declara `CalcularPagoSemanal()`
 * y cada subclase la implementa, de modo que el servicio de reportes nunca pregunta de qué
 * tipo es nadie. Aquí, cada configuración implementa `crear`, `actualizar` y
 * `extraerValoresDeContrato`. En los dos casos, agregar un quinto tipo es agregar una pieza
 * nueva y no editar las existentes: eso es el principio abierto/cerrado.
 */
export interface ConfiguracionTipoEmpleado {
  clave: TipoEmpleado;

  /** Debe coincidir LITERALMENTE con el `TipoContrato` que publica el Dominio. */
  etiqueta: string;

  descripcion: string;

  campos: readonly DefinicionDeCampoDeContrato[];

  crear: (valores: ValoresFormularioEmpleado) => Promise<EmpleadoDto>;

  actualizar: (identificador: number, valores: ValoresFormularioEmpleado) => Promise<EmpleadoDto>;

  /** Convierte un empleado ya guardado en los valores de texto del formulario de edición. */
  extraerValoresDeContrato: (empleado: EmpleadoDto) => Record<string, string>;
}

/**
 * Segmento de la URL de cada tipo, tal como lo declara el `[Route]` de su controlador.
 *
 * `satisfies` comprueba que estén los cuatro tipos y ni uno más, sin ensanchar el tipo de los
 * valores a `string`: si mañana se agrega una clave a `TIPOS_EMPLEADO` y se olvida aquí, esto
 * deja de compilar.
 */
const SEGMENTOS_DE_RUTA = {
  Asalariado: 'asalariados',
  PorHoras: 'por-horas',
  PorComision: 'por-comision',
  AsalariadoPorComision: 'asalariados-por-comision',
} as const satisfies Record<TipoEmpleado, string>;

/** Extrae la parte común a los cuatro DTOs de alta. */
function extraerDatosPersonales(valores: ValoresFormularioEmpleado) {
  return {
    primerNombre: valores.primerNombre,
    apellidoPaterno: valores.apellidoPaterno,
    numeroSeguroSocial: valores.numeroSeguroSocial,
    departamento: valores.departamento,
  };
}

/** Igual que la anterior, más el estado, que solo se edita. */
function extraerDatosDeEdicion(valores: ValoresFormularioEmpleado) {
  return { ...extraerDatosPersonales(valores), estado: valores.estado };
}

/** Convierte a número el campo de contrato indicado. El esquema ya garantizó que es válido. */
function aNumero(valores: ValoresFormularioEmpleado, nombreDelCampo: string): number {
  return Number(valores.camposDeContrato[nombreDelCampo]);
}

/**
 * Convierte a texto un decimal que puede venir nulo.
 *
 * Un campo que no corresponde al tipo llega como `null`, y `String(null)` daría la cadena
 * "null" dentro del control: se vería literalmente esa palabra en el formulario.
 */
function aTexto(valor: number | null): string {
  return valor === null ? '' : String(valor);
}

/**
 * El registro. Es el ÚNICO lugar del frontend donde se enumeran los tipos de empleado.
 */
export const REGISTRO_TIPOS_EMPLEADO: Record<TipoEmpleado, ConfiguracionTipoEmpleado> = {
  Asalariado: {
    clave: 'Asalariado',
    etiqueta: 'Empleado Asalariado',
    descripcion: 'Cobra un salario fijo cada semana, sin importar las horas trabajadas.',
    campos: [
      {
        nombre: 'salarioSemanal',
        etiqueta: 'Salario semanal',
        paso: PASO_MONETARIO,
        valorMinimo: MONTO_MINIMO,
      },
    ],
    crear: (valores) =>
      crearEmpleadoDeTipo<CrearEmpleadoAsalariadoDto>(SEGMENTOS_DE_RUTA.Asalariado, {
        ...extraerDatosPersonales(valores),
        salarioSemanal: aNumero(valores, 'salarioSemanal'),
      }),
    actualizar: (identificador, valores) =>
      actualizarEmpleadoDeTipo<ActualizarEmpleadoAsalariadoDto>(
        SEGMENTOS_DE_RUTA.Asalariado,
        identificador,
        {
          ...extraerDatosDeEdicion(valores),
          salarioSemanal: aNumero(valores, 'salarioSemanal'),
        },
      ),
    extraerValoresDeContrato: (empleado) => ({
      salarioSemanal: aTexto(empleado.salarioSemanal),
    }),
  },

  PorHoras: {
    clave: 'PorHoras',
    etiqueta: 'Empleado por Horas',
    descripcion:
      'Cobra por hora. A partir de 40 horas semanales, las horas extra se pagan a 1.5 veces la tarifa.',
    campos: [
      {
        nombre: 'sueldoPorHora',
        etiqueta: 'Sueldo por hora',
        paso: PASO_MONETARIO,
        valorMinimo: MONTO_MINIMO,
      },
      {
        nombre: 'horasTrabajadas',
        etiqueta: 'Horas trabajadas en la semana',
        ayuda: 'Entre 0 y 168 horas, que son las que tiene una semana.',
        paso: PASO_MONETARIO,
        valorMinimo: HORAS_MINIMAS_SEMANALES,
        valorMaximo: HORAS_MAXIMAS_SEMANALES,
      },
    ],
    crear: (valores) =>
      crearEmpleadoDeTipo<CrearEmpleadoPorHorasDto>(SEGMENTOS_DE_RUTA.PorHoras, {
        ...extraerDatosPersonales(valores),
        sueldoPorHora: aNumero(valores, 'sueldoPorHora'),
        horasTrabajadas: aNumero(valores, 'horasTrabajadas'),
      }),
    actualizar: (identificador, valores) =>
      actualizarEmpleadoDeTipo<ActualizarEmpleadoPorHorasDto>(
        SEGMENTOS_DE_RUTA.PorHoras,
        identificador,
        {
          ...extraerDatosDeEdicion(valores),
          sueldoPorHora: aNumero(valores, 'sueldoPorHora'),
          horasTrabajadas: aNumero(valores, 'horasTrabajadas'),
        },
      ),
    extraerValoresDeContrato: (empleado) => ({
      sueldoPorHora: aTexto(empleado.sueldoPorHora),
      horasTrabajadas: aTexto(empleado.horasTrabajadas),
    }),
  },

  PorComision: {
    clave: 'PorComision',
    etiqueta: 'Empleado por Comisión',
    descripcion: 'Cobra un porcentaje de lo que vende en la semana.',
    campos: [
      {
        nombre: 'ventasBrutas',
        etiqueta: 'Ventas brutas de la semana',
        paso: PASO_MONETARIO,
        valorMinimo: MONTO_MINIMO,
      },
      {
        nombre: 'tarifaComision',
        etiqueta: 'Tarifa de comisión',
        ayuda: 'Se expresa como fracción: 0.1 equivale al 10 %. Debe estar entre 0 y 1.',
        paso: PASO_FRACCION,
        valorMinimo: TARIFA_COMISION_MINIMA,
        valorMaximo: TARIFA_COMISION_MAXIMA,
      },
    ],
    crear: (valores) =>
      crearEmpleadoDeTipo<CrearEmpleadoPorComisionDto>(SEGMENTOS_DE_RUTA.PorComision, {
        ...extraerDatosPersonales(valores),
        ventasBrutas: aNumero(valores, 'ventasBrutas'),
        tarifaComision: aNumero(valores, 'tarifaComision'),
      }),
    actualizar: (identificador, valores) =>
      actualizarEmpleadoDeTipo<ActualizarEmpleadoPorComisionDto>(
        SEGMENTOS_DE_RUTA.PorComision,
        identificador,
        {
          ...extraerDatosDeEdicion(valores),
          ventasBrutas: aNumero(valores, 'ventasBrutas'),
          tarifaComision: aNumero(valores, 'tarifaComision'),
        },
      ),
    extraerValoresDeContrato: (empleado) => ({
      ventasBrutas: aTexto(empleado.ventasBrutas),
      tarifaComision: aTexto(empleado.tarifaComision),
    }),
  },

  AsalariadoPorComision: {
    clave: 'AsalariadoPorComision',
    etiqueta: 'Empleado Asalariado por Comisión',
    descripcion:
      'Cobra un salario base más comisión sobre sus ventas. El salario base recibe además una bonificación del 10 %, que aplica el Dominio.',
    campos: [
      {
        nombre: 'ventasBrutas',
        etiqueta: 'Ventas brutas de la semana',
        paso: PASO_MONETARIO,
        valorMinimo: MONTO_MINIMO,
      },
      {
        nombre: 'tarifaComision',
        etiqueta: 'Tarifa de comisión',
        ayuda: 'Se expresa como fracción: 0.1 equivale al 10 %. Debe estar entre 0 y 1.',
        paso: PASO_FRACCION,
        valorMinimo: TARIFA_COMISION_MINIMA,
        valorMaximo: TARIFA_COMISION_MAXIMA,
      },
      {
        nombre: 'salarioBase',
        etiqueta: 'Salario base',
        ayuda: 'La bonificación del 10 % no se captura: la calcula el servidor.',
        paso: PASO_MONETARIO,
        valorMinimo: MONTO_MINIMO,
      },
    ],
    crear: (valores) =>
      crearEmpleadoDeTipo<CrearEmpleadoAsalariadoPorComisionDto>(
        SEGMENTOS_DE_RUTA.AsalariadoPorComision,
        {
          ...extraerDatosPersonales(valores),
          ventasBrutas: aNumero(valores, 'ventasBrutas'),
          tarifaComision: aNumero(valores, 'tarifaComision'),
          salarioBase: aNumero(valores, 'salarioBase'),
        },
      ),
    actualizar: (identificador, valores) =>
      actualizarEmpleadoDeTipo<ActualizarEmpleadoAsalariadoPorComisionDto>(
        SEGMENTOS_DE_RUTA.AsalariadoPorComision,
        identificador,
        {
          ...extraerDatosDeEdicion(valores),
          ventasBrutas: aNumero(valores, 'ventasBrutas'),
          tarifaComision: aNumero(valores, 'tarifaComision'),
          salarioBase: aNumero(valores, 'salarioBase'),
        },
      ),
    extraerValoresDeContrato: (empleado) => ({
      ventasBrutas: aTexto(empleado.ventasBrutas),
      tarifaComision: aTexto(empleado.tarifaComision),
      salarioBase: aTexto(empleado.salarioBase),
    }),
  },
};

/** Patrón de un número decimal con punto. Rechaza "12,5", "1e3" y el texto vacío. */
const PATRON_NUMERO_DECIMAL = /^-?\d+(\.\d+)?$/;

function construirEsquemaDeCampoDeContrato(
  campo: DefinicionDeCampoDeContrato,
): z.ZodType<string, string> {
  let esquema = z
    .string()
    .trim()
    .min(1, `El campo '${campo.etiqueta}' es obligatorio.`)
    .refine(
      (valor) => PATRON_NUMERO_DECIMAL.test(valor),
      `El campo '${campo.etiqueta}' debe ser un número. Use punto para los decimales.`,
    )
    .refine(
      (valor) => Number(valor) >= campo.valorMinimo,
      `El campo '${campo.etiqueta}' no puede ser menor que ${campo.valorMinimo}.`,
    );

  if (campo.valorMaximo !== undefined) {
    const valorMaximo = campo.valorMaximo;

    esquema = esquema.refine(
      (valor) => Number(valor) <= valorMaximo,
      `El campo '${campo.etiqueta}' no puede ser mayor que ${valorMaximo}.`,
    );
  }

  return esquema;
}

/**
 * Arma el esquema de validación del tipo indicado a partir de sus campos declarados.
 *
 * La validación también se genera desde el registro, no solo el dibujado. Si el esquema se
 * escribiera aparte, agregar un campo obligaría a acordarse de tocar dos sitios, y el olvido
 * clásico —campo dibujado pero no validado— pasa desapercibido hasta que el servidor lo
 * rechaza.
 *
 * Los mensajes replican los del backend en español para que un mismo error se lea igual lo
 * detecte quien lo detecte.
 */
export function construirEsquemaDeEmpleado(
  configuracion: ConfiguracionTipoEmpleado,
  // El tipo se anota con salida Y entrada. En Zod 4, `ZodType<T>` deja la entrada como
  // `unknown`, y con eso `zodResolver` no puede comprobar que lo que el formulario entrega
  // encaja con lo que el esquema espera: el compilador rechaza el resolver entero.
): z.ZodType<ValoresFormularioEmpleado, ValoresFormularioEmpleado> {
  const formaDeLosCamposDeContrato: Record<string, z.ZodType<string, string>> = {};

  for (const campo of configuracion.campos) {
    formaDeLosCamposDeContrato[campo.nombre] = construirEsquemaDeCampoDeContrato(campo);
  }

  return z.object({
    primerNombre: esquemaDeTextoObligatorio('Primer nombre', LONGITUD_MAXIMA.PRIMER_NOMBRE),
    apellidoPaterno: esquemaDeTextoObligatorio(
      'Apellido paterno',
      LONGITUD_MAXIMA.APELLIDO_PATERNO,
    ),
    numeroSeguroSocial: esquemaDeTextoObligatorio(
      'Número de seguro social',
      LONGITUD_MAXIMA.NUMERO_SEGURO_SOCIAL,
    ),
    departamento: esquemaDeTextoObligatorio('Departamento', LONGITUD_MAXIMA.DEPARTAMENTO),
    estado: z.union([z.literal(EstadoEmpleado.Activo), z.literal(EstadoEmpleado.Inactivo)]),
    camposDeContrato: z.object(formaDeLosCamposDeContrato),
  });
}

function esquemaDeTextoObligatorio(
  etiqueta: string,
  longitudMaxima: number,
): z.ZodType<string, string> {
  return z
    .string()
    .trim()
    .min(1, `El campo '${etiqueta}' es obligatorio.`)
    .max(longitudMaxima, `El campo '${etiqueta}' no puede exceder ${longitudMaxima} caracteres.`);
}

/**
 * Valores en blanco para el formulario de alta.
 *
 * Cada campo arranca como cadena vacía y no como `undefined`: un `<input>` cuyo valor pasa de
 * indefinido a definido salta de no controlado a controlado, y React lo avisa por consola
 * porque a partir de ese momento pierde el rastro de quién manda sobre el campo.
 */
export function construirValoresIniciales(
  configuracion: ConfiguracionTipoEmpleado,
): ValoresFormularioEmpleado {
  const camposDeContrato: Record<string, string> = {};

  for (const campo of configuracion.campos) {
    camposDeContrato[campo.nombre] = '';
  }

  return {
    primerNombre: '',
    apellidoPaterno: '',
    numeroSeguroSocial: '',
    departamento: '',
    estado: EstadoEmpleado.Activo,
    camposDeContrato,
  };
}

/** Valores de partida del formulario de edición, a partir del empleado ya guardado. */
export function construirValoresDesdeEmpleado(
  configuracion: ConfiguracionTipoEmpleado,
  empleado: EmpleadoDto,
): ValoresFormularioEmpleado {
  return {
    primerNombre: empleado.primerNombre,
    apellidoPaterno: empleado.apellidoPaterno,
    numeroSeguroSocial: empleado.numeroSeguroSocial,
    departamento: empleado.departamento,
    estado: empleado.estado,
    camposDeContrato: configuracion.extraerValoresDeContrato(empleado),
  };
}
