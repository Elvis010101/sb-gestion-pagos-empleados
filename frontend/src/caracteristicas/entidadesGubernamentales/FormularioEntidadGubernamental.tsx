import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';

import { traducirError, type ErrorApi } from '../../comunes/api/ErrorApi';
import { Boton } from '../../comunes/componentes/Boton';
import { CampoDeTexto } from '../../comunes/componentes/CampoDeTexto';
import estilosDeDialogo from '../../comunes/componentes/Dialogo.module.css';
import { MensajeDeError } from '../../comunes/componentes/MensajeDeError';

import estilos from './FormularioEntidadGubernamental.module.css';
import type { EntidadGubernamentalDto } from './tipos';

/** Longitudes de `LongitudMaxima` en la capa Aplicación del backend. */
const LONGITUD_MAXIMA = {
  NOMBRE: 200,
  CATEGORIA: 100,
  PODER_DEL_ESTADO: 100,
  SECTOR: 150,
} as const;

function textoObligatorio(etiqueta: string, longitudMaxima: number) {
  return z
    .string()
    .trim()
    .min(1, `El campo '${etiqueta}' es obligatorio.`)
    .max(longitudMaxima, `El campo '${etiqueta}' no puede exceder ${longitudMaxima} caracteres.`);
}

const esquemaEntidad = z.object({
  nombre: textoObligatorio('Nombre', LONGITUD_MAXIMA.NOMBRE),
  categoria: textoObligatorio('Categoría', LONGITUD_MAXIMA.CATEGORIA),
  poderDelEstado: textoObligatorio('Poder del Estado', LONGITUD_MAXIMA.PODER_DEL_ESTADO),
  sector: textoObligatorio('Sector', LONGITUD_MAXIMA.SECTOR),
});

export type ValoresFormularioEntidad = z.infer<typeof esquemaEntidad>;

const VALORES_EN_BLANCO: ValoresFormularioEntidad = {
  nombre: '',
  categoria: '',
  poderDelEstado: '',
  sector: '',
};

interface PropiedadesFormularioEntidadGubernamental {
  /** La entidad que se edita, o `null` para un alta. */
  entidad: EntidadGubernamentalDto | null;

  alGuardar: (valores: ValoresFormularioEntidad) => Promise<void>;
  alCancelar: () => void;
}

/**
 * Formulario de alta y edición de una entidad del catálogo.
 *
 * Es UNO solo para los dos casos porque los campos son idénticos; lo único que cambia es el
 * rótulo del botón y de dónde salen los valores iniciales. Duplicarlo en dos componentes
 * dejaría dos sitios donde agregar el campo siguiente.
 */
export function FormularioEntidadGubernamental({
  entidad,
  alGuardar,
  alCancelar,
}: PropiedadesFormularioEntidadGubernamental) {
  const [errorAlGuardar, establecerErrorAlGuardar] = useState<ErrorApi | null>(null);

  const {
    register: registrarCampo,
    handleSubmit: manejarEnvio,
    formState: { errors: erroresDeCampo, isSubmitting: estaEnviando },
  } = useForm<ValoresFormularioEntidad>({
    resolver: zodResolver(esquemaEntidad),
    defaultValues:
      entidad === null
        ? VALORES_EN_BLANCO
        : {
            nombre: entidad.nombre,
            categoria: entidad.categoria,
            poderDelEstado: entidad.poderDelEstado,
            sector: entidad.sector,
          },
  });

  async function enviar(valores: ValoresFormularioEntidad): Promise<void> {
    establecerErrorAlGuardar(null);

    try {
      await alGuardar(valores);
    } catch (error: unknown) {
      establecerErrorAlGuardar(traducirError(error));
    }
  }

  return (
    <form onSubmit={manejarEnvio(enviar)} noValidate>
      <fieldset className={estilos.campos} disabled={estaEnviando}>
        <CampoDeTexto
          etiqueta="Nombre"
          autoFocus
          mensajeDeError={erroresDeCampo.nombre?.message}
          {...registrarCampo('nombre')}
        />
        <CampoDeTexto
          etiqueta="Categoría"
          mensajeDeError={erroresDeCampo.categoria?.message}
          {...registrarCampo('categoria')}
        />
        <CampoDeTexto
          etiqueta="Poder del Estado"
          mensajeDeError={erroresDeCampo.poderDelEstado?.message}
          {...registrarCampo('poderDelEstado')}
        />
        <CampoDeTexto
          etiqueta="Sector"
          mensajeDeError={erroresDeCampo.sector?.message}
          {...registrarCampo('sector')}
        />
      </fieldset>

      {errorAlGuardar !== null ? <MensajeDeError error={errorAlGuardar} /> : null}

      <div className={estilosDeDialogo.acciones}>
        <Boton variante="secundario" type="button" onClick={alCancelar} disabled={estaEnviando}>
          Cancelar
        </Boton>
        <Boton type="submit" estaProcesando={estaEnviando}>
          {entidad === null ? 'Agregar entidad' : 'Guardar cambios'}
        </Boton>
      </div>
    </form>
  );
}
