/// <reference types="vite/client" />

/**
 * Declaración de las variables de entorno propias del proyecto.
 *
 * Sin esto, `import.meta.env.VITE_URL_BASE_API` sería de tipo `any` y un error de tipeo en el
 * nombre no lo detectaría nadie hasta que la aplicación intentara llamar a `undefined/api`.
 */
interface ImportMetaEnv {
  readonly VITE_URL_BASE_API?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
