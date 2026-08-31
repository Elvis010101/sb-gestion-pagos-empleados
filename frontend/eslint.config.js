import js from '@eslint/js';
import configuracionPrettier from 'eslint-config-prettier';
import complementoReactHooks from 'eslint-plugin-react-hooks';
import complementoReactRefresh from 'eslint-plugin-react-refresh';
import globals from 'globals';
import typescriptEslint from 'typescript-eslint';

export default typescriptEslint.config(
  { ignores: ['dist'] },
  {
    files: ['**/*.{ts,tsx}'],
    extends: [js.configs.recommended, ...typescriptEslint.configs.recommended],
    languageOptions: {
      ecmaVersion: 2022,
      globals: globals.browser,
    },
    plugins: {
      'react-hooks': complementoReactHooks,
      'react-refresh': complementoReactRefresh,
    },
    rules: {
      ...complementoReactHooks.configs.recommended.rules,
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],

      // `any` apaga el compilador justo donde más falta hace: en el borde donde entran los
      // datos del servidor. La prueba pide explícitamente que no haya ninguno, así que es
      // error y no advertencia.
      '@typescript-eslint/no-explicit-any': 'error',
    },
  },

  // Debe ir de último: apaga las reglas de estilo que chocarían con Prettier, para que no
  // haya dos herramientas discutiendo sobre dónde va una coma.
  configuracionPrettier,
);
