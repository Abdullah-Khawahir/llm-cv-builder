import * as pdfjs from 'pdfjs-dist';

/**
 * Configure PDF.js worker for React-PDF
 * Call this once at app initialization
 */
export const configurePdfWorker = (): void => {
  // Dynamic import for worker - works with Vite/Webpack
  const workerUrl = new URL(
    'pdfjs-dist/build/pdf.worker.min.mjs',
    import.meta.url
  ).toString();
  
  pdfjs.GlobalWorkerOptions.workerSrc = workerUrl;
};

export { pdfjs };
