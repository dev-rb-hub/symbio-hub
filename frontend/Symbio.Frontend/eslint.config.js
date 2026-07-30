import { fileURLToPath } from 'url';
import path from 'path';
import parser from '@typescript-eslint/parser';
import tsPlugin from '@typescript-eslint/eslint-plugin';

const rootDir = path.dirname(fileURLToPath(import.meta.url));

export default [
  {
    files: ['src/**/*.{ts,tsx}'],
    languageOptions: {
      parser,
      parserOptions: {
        ecmaVersion: 2021,
        sourceType: 'module',
        project: [path.join(rootDir, 'tsconfig.json')],
        tsconfigRootDir: rootDir,
      },
    },
    plugins: {
      '@typescript-eslint': tsPlugin,
    },
    rules: {
      '@typescript-eslint/no-unused-vars': ['warn', { argsIgnorePattern: '^_' }],
    },
  },
];
