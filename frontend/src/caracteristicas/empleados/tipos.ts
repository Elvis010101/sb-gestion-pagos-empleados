/**
 * Contratos del módulo de empleados (RF-01, RF-03, RF-04, RF-05).
 */

/**
 * Situación laboral del empleado, espejo del enum `EstadoEmpleado` del Dominio. Viaja como
 * número por el mismo motivo que `RolUsuario`.
 */
export const EstadoEmpleado = {
  Activo: 1,
  Inactivo: 2,
} as const;

export type EstadoEmpleado = (typeof EstadoEmpleado)[keyof typeof EstadoEmpleado];

export const ETIQUETAS_ESTADO_EMPLEADO: Record<EstadoEmpleado, string> = {
  [EstadoEmpleado.Activo]: 'Activo',
  [EstadoEmpleado.Inactivo]: 'Inactivo',
};

/**
 * Los cuatro tipos de contrato del sistema, identificados por una clave propia del frontend.
 *
 * La clave NO es el rótulo que devuelve el servidor ni el segmento de URL: es un
 * identificador estable del que cuelgan ambos. Si la institución decidiera mañana llamar
 * "Empleado Comisionista" a lo que hoy es "Empleado por Comisión", cambia un rótulo y no
 * media aplicación.
 */
export const TIPOS_EMPLEADO = [
  'Asalariado',
  'PorHoras',
  'PorComision',
  'AsalariadoPorComision',
] as const;

export type TipoEmpleado = (typeof TIPOS_EMPLEADO)[number];

/**
 * Traducción del rótulo que publica el Dominio en `EmpleadoDto.tipoContrato` a la clave
 * interna. Hace falta al editar: el listado entrega el rótulo, y el formulario necesita
 * saber qué campos dibujar.
 *
 * Los rótulos son literalmente los que devuelven las entidades del Dominio; cualquier
 * discrepancia de acento o mayúscula rompe la búsqueda, por eso están copiados y no
 * reconstruidos.
 */
export const CLAVE_POR_ETIQUETA_TIPO_CONTRATO: Record<string, TipoEmpleado> = {
  'Empleado Asalariado': 'Asalariado',
  'Empleado por Horas': 'PorHoras',
  'Empleado por Comisión': 'PorComision',
  'Empleado Asalariado por Comisión': 'AsalariadoPorComision',
};

/**
 * Representación de lectura de un empleado, común a los cuatro tipos.
 *
 * Los campos propios de cada contrato se declaran `number | null` y no opcionales con `?`:
 * `System.Text.Json` serializa los `decimal?` sin valor como `null` explícito, no los omite.
 * Declararlos con `?` describiría una respuesta que el servidor nunca envía, y el primer
 * `if (empleado.salarioSemanal === undefined)` fallaría en silencio.
 */
export interface EmpleadoDto {
  id: number;
  primerNombre: string;
  apellidoPaterno: string;
  numeroSeguroSocial: string;
  departamento: string;
  estado: EstadoEmpleado;

  /** Rótulo del tipo de contrato, provisto por el Dominio. */
  tipoContrato: string;

  /** Pago semanal ya calculado por el servidor (RF-04). Nunca se recalcula aquí. */
  pagoSemanalCalculado: number;

  /** Fecha en formato ISO 8601. */
  fechaCreacion: string;

  /** Solo en Empleado Asalariado. */
  salarioSemanal: number | null;

  /** Solo en Empleado por Horas. */
  sueldoPorHora: number | null;
  horasTrabajadas: number | null;

  /** En Empleado por Comisión y en Empleado Asalariado por Comisión. */
  ventasBrutas: number | null;
  tarifaComision: number | null;

  /** Solo en Empleado Asalariado por Comisión. */
  salarioBase: number | null;
}

/** Criterios de la consulta paginada de empleados (RF-03). */
export interface FiltroEmpleados {
  nombre?: string;
  departamento?: string;
  estado?: EstadoEmpleado;
  pagina: number;
  tamanoPagina: number;
}

/**
 * Parte común a los cuatro DTOs de alta. Es el espejo de la porción que en el backend
 * comparten los `Crear...Dto`; no lleva `estado` porque el Dominio impone que todo empleado
 * nace Activo, ni `id` porque lo asigna la base de datos.
 */
export interface DatosPersonalesEmpleado {
  primerNombre: string;
  apellidoPaterno: string;
  numeroSeguroSocial: string;
  departamento: string;
}

export interface CrearEmpleadoAsalariadoDto extends DatosPersonalesEmpleado {
  salarioSemanal: number;
}

export interface CrearEmpleadoPorHorasDto extends DatosPersonalesEmpleado {
  sueldoPorHora: number;
  horasTrabajadas: number;
}

export interface CrearEmpleadoPorComisionDto extends DatosPersonalesEmpleado {
  ventasBrutas: number;

  /** Fracción, no porcentaje: 0.10 significa 10 %, igual que en el Dominio. */
  tarifaComision: number;
}

export interface CrearEmpleadoAsalariadoPorComisionDto extends CrearEmpleadoPorComisionDto {
  salarioBase: number;
}

/**
 * Parte común a los cuatro DTOs de edición. A diferencia del alta SÍ lleva `estado`, porque
 * el RF-03 filtra por él y tiene que existir forma de cambiarlo.
 */
export interface DatosEdicionEmpleado extends DatosPersonalesEmpleado {
  estado: EstadoEmpleado;
}

export interface ActualizarEmpleadoAsalariadoDto extends DatosEdicionEmpleado {
  salarioSemanal: number;
}

export interface ActualizarEmpleadoPorHorasDto extends DatosEdicionEmpleado {
  sueldoPorHora: number;
  horasTrabajadas: number;
}

export interface ActualizarEmpleadoPorComisionDto extends DatosEdicionEmpleado {
  ventasBrutas: number;
  tarifaComision: number;
}

export interface ActualizarEmpleadoAsalariadoPorComisionDto extends ActualizarEmpleadoPorComisionDto {
  salarioBase: number;
}
