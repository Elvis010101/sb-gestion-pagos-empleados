import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';

import { traducirError, type ErrorApi } from '../../comunes/api/ErrorApi';
import { Boton } from '../../comunes/componentes/Boton';
import { CampoDeSeleccion } from '../../comunes/componentes/CampoDeSeleccion';
import { CampoDeTexto } from '../../comunes/componentes/CampoDeTexto';
import { MensajeDeError } from '../../comunes/componentes/MensajeDeError';

import estilos from './FormularioEmpleado.module.css';
import {
  construirEsquemaDeEmpleado,
  type ConfiguracionTipoEmpleado,
  type ValoresFormularioEmpleado,
} from './configuracionDeTiposDeEmpleado';
import { ETIQUETAS_ESTADO_EMPLEADO, EstadoEmpleado } from './tipos';

const OPCIONES_DE_ESTADO = [
  {
    valor: String(EstadoEmpleado.Activo),
    etiqueta: ETIQUETAS_ESTADO_EMPLEADO[EstadoEmpleado.Activo],
  },
  {
    valor: String(EstadoEmpleado.Inactivo),
    etiqueta: ETIQUETAS_ESTADO_EMPLEADO[EstadoEmpleado.Inactivo],
  },
];

interface PropiedadesFormularioEmpleado {
  /** Qué tipo de contrato se está capturando. De aquí salen los campos y las validaciones. */
  configuracion: ConfiguracionTipoEmpleado;

  valoresIniciales: ValoresFormularioEmpleado;

  /** El estado solo se edita: al crear, el Dominio impone que el empleado nace Activo. */
  permiteEditarEstado: boolean;

  etiquetaDeGuardar: string;

  /**
   * Se avisa hacia arriba con los valores validados. El formulario NO llama a la API ni
   * navega: no sabe si esto es un alta o una edición, ni a dónde hay que ir después. Ese
   * reparto es lo que le permite servir a las dos pantallas sin una sola condición.
   */
  alGuardar: (valores: ValoresFormularioEmpleado) => Promise<void>;

  alCancelar: () => void;
}

/**
 * Formulario de empleado, común a los cuatro tipos de contrato.
 *
 * No hay ningún `if` ni `switch` sobre el tipo: los campos propios del contrato se dibujan
 * recorriendo `configuracion.campos`, y el esquema de validación se construye a partir de esa
 * misma lista. Un quinto tipo de empleado no toca este archivo.
 */
export function FormularioEmpleado({
  configuracion,
  valoresIniciales,
  permiteEditarEstado,
  etiquetaDeGuardar,
  alGuardar,
  alCancelar,
}: PropiedadesFormularioEmpleado) {
  const [errorAlGuardar, establecerErrorAlGuardar] = useState<ErrorApi | null>(null);

  // Se memoriza porque construir el esquema recorre los campos y crea objetos de Zod: sin
  // esto se rehace en cada pulsación de tecla que provoque un dibujado.
  const esquema = useMemo(() => construirEsquemaDeEmpleado(configuracion), [configuracion]);

  const {
    register: registrarCampo,
    handleSubmit: manejarEnvio,
    formState: { errors: erroresDeCampo, isSubmitting: estaEnviando },
  } = useForm<ValoresFormularioEmpleado>({
    resolver: zodResolver(esquema),
    defaultValues: valoresIniciales,
  });

  async function enviar(valores: ValoresFormularioEmpleado): Promise<void> {
    establecerErrorAlGuardar(null);

    try {
      await alGuardar(valores);
    } catch (error: unknown) {
      establecerErrorAlGuardar(traducirError(error));
    }
  }

  return (
    <form className={estilos.formulario} onSubmit={manejarEnvio(enviar)} noValidate>
      <fieldset className={estilos.grupo} disabled={estaEnviando}>
        <legend className={estilos.tituloDelGrupo}>Datos personales</legend>

        <div className={estilos.rejilla}>
          <CampoDeTexto
            etiqueta="Primer nombre"
            autoComplete="off"
            mensajeDeError={erroresDeCampo.primerNombre?.message}
            {...registrarCampo('primerNombre')}
          />

          <CampoDeTexto
            etiqueta="Apellido paterno"
            autoComplete="off"
            mensajeDeError={erroresDeCampo.apellidoPaterno?.message}
            {...registrarCampo('apellidoPaterno')}
          />

          <CampoDeTexto
            etiqueta="Número de seguro social"
            autoComplete="off"
            mensajeDeError={erroresDeCampo.numeroSeguroSocial?.message}
            {...registrarCampo('numeroSeguroSocial')}
          />

          <CampoDeTexto
            etiqueta="Departamento"
            autoComplete="off"
            mensajeDeError={erroresDeCampo.departamento?.message}
            {...registrarCampo('departamento')}
          />

          {permiteEditarEstado ? (
            <CampoDeSeleccion
              etiqueta="Estado"
              opciones={OPCIONES_DE_ESTADO}
              mensajeDeError={erroresDeCampo.estado?.message}
              // `valueAsNumber` convierte antes de validar: un `<select>` entrega el texto
              // "1", y el esquema —igual que el backend— espera el número 1.
              {...registrarCampo('estado', { valueAsNumber: true })}
            />
          ) : null}
        </div>
      </fieldset>

      <fieldset className={estilos.grupo} disabled={estaEnviando}>
        <legend className={estilos.tituloDelGrupo}>{configuracion.etiqueta}</legend>

        <div className={estilos.rejilla}>
          {configuracion.campos.map((campo) => (
            <div className={estilos.campoConAyuda} key={campo.nombre}>
              <CampoDeTexto
                etiqueta={campo.etiqueta}
                // `inputMode="decimal"` levanta el teclado numérico en un móvil; el tipo
                // sigue siendo `number` para que el navegador ofrezca las flechas.
                type="number"
                inputMode="decimal"
                step={campo.paso}
                min={campo.valorMinimo}
                max={campo.valorMaximo}
                autoComplete="off"
                mensajeDeError={erroresDeCampo.camposDeContrato?.[campo.nombre]?.message}
                {...registrarCampo(`camposDeContrato.${campo.nombre}`)}
              />
              {campo.ayuda !== undefined ? (
                <span className={estilos.ayuda}>{campo.ayuda}</span>
              ) : null}
            </div>
          ))}
        </div>
      </fieldset>

      {errorAlGuardar !== null ? <MensajeDeError error={errorAlGuardar} /> : null}

      <div className={estilos.acciones}>
        <Boton variante="secundario" type="button" onClick={alCancelar} disabled={estaEnviando}>
          Cancelar
        </Boton>
        <Boton type="submit" estaProcesando={estaEnviando}>
          {etiquetaDeGuardar}
        </Boton>
      </div>
    </form>
  );
}
