'use client';

import { useState, useEffect, useRef } from 'react';
import { Document, Page, pdfjs } from 'react-pdf';

import 'react-pdf/dist/Page/TextLayer.css';
import 'react-pdf/dist/Page/AnnotationLayer.css';

if (typeof window !== 'undefined') {
  pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;
}

interface PdfViewerProps {
  pdfUrl: string;
}

type ScaleMode = 'custom' | 'fitWidth' | 'fitHeight';

export default function PdfViewer({ pdfUrl }: PdfViewerProps) {
  const [numPages, setNumPages] = useState<number | null>(null);
  const [scale, setScale] = useState<number>(1.0);
  const [scaleMode, setScaleMode] = useState<ScaleMode>('fitWidth');
  const [isMounted, setIsMounted] = useState(false);

  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  // Recalculate dimensions for Fit Width / Fit Height
  useEffect(() => {
    if (scaleMode === 'custom' || !containerRef.current || !numPages) return;

    const handleResize = () => {
      const container = containerRef.current;
      if (!container) return;

      if (scaleMode === 'fitWidth') {
        const targetScale = (container.clientWidth - 32) / 612;
        setScale(Math.max(0.5, Math.min(targetScale, 3.0)));
      } else if (scaleMode === 'fitHeight') {
        const targetScale = (container.clientHeight - 32) / 792;
        setScale(Math.max(0.5, Math.min(targetScale, 3.0)));
      }
    };

    handleResize();
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [scaleMode, numPages]);

  const onDocumentLoadSuccess = ({ numPages }: { numPages: number }) => {
    setNumPages(numPages);
  };

  const onDocumentLoadError = (error: Error) => {
    console.error('PDF load error:', error);
  };

  const handleZoomIn = () => {
    setScaleMode('custom');
    setScale(prev => Math.min(prev + 0.1, 3.0));
  };

  const handleZoomOut = () => {
    setScaleMode('custom');
    setScale(prev => Math.max(prev - 0.1, 0.5));
  };

  if (!isMounted) {
    return <div className="p-8 text-center text-muted-foreground font-medium bg-background">Loading PDF viewer...</div>;
  }

  return (
    <div className="flex flex-col h-screen w-full bg-muted/30 border border-border rounded-xl overflow-hidden shadow-xs">

      {/* Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-4 p-3 bg-card border-b border-border shadow-xs z-10">

        {/* Document Info */}
        <div className="text-sm font-medium text-muted-foreground select-none px-2">
          {numPages ? `${numPages} Pages` : 'Loading document details...'}
        </div>

        {/* Zoom & Fit Controls */}
        <div className="flex items-center gap-2">
          <button
            onClick={handleZoomOut}
            disabled={scale <= 0.5}
            className="p-2 bg-muted text-card-foreground font-bold border border-border rounded-lg hover:bg-muted/80 disabled:opacity-40 transition cursor-pointer disabled:cursor-not-allowed"
            title="Zoom Out"
          >
            <span className="block w-4 h-4 text-center leading-3">−</span>
          </button>

          <span className="text-sm font-semibold text-foreground min-w-15 text-center select-none">
            {Math.round(scale * 100)}%
          </span>

          <button
            onClick={handleZoomIn}
            disabled={scale >= 3.0}
            className="p-2 bg-muted text-card-foreground font-bold border border-border rounded-lg hover:bg-muted/80 disabled:opacity-40 transition cursor-pointer disabled:cursor-not-allowed"
            title="Zoom In"
          >
            <span className="block w-4 h-4 text-center leading-3">+</span>
          </button>

          <div className="h-6 w-[1px] bg-border mx-1" />

          <button
            onClick={() => setScaleMode('fitWidth')}
            className={`px-3 py-1.5 text-xs font-medium border rounded-lg transition cursor-pointer ${scaleMode === 'fitWidth'
              ? 'bg-primary text-primary-foreground border-primary'
              : 'bg-card text-muted-foreground border-border hover:bg-muted'
              }`}
          >
            Fit Width
          </button>

          <button
            onClick={() => setScaleMode('fitHeight')}
            className={`px-3 py-1.5 text-xs font-medium border rounded-lg transition cursor-pointer ${scaleMode === 'fitHeight'
              ? 'bg-primary text-primary-foreground border-primary'
              : 'bg-card text-muted-foreground border-border hover:bg-muted'
              }`}
          >
            Fit Height
          </button>
        </div>
      </div>

      {/* Continuous Scroll Canvas Container */}
      <div
        ref={containerRef}
        className="flex-1 overflow-auto p-6 flex flex-col items-center bg-muted"
        style={{ userSelect: 'text', WebkitUserSelect: 'text' }}
      >
        <Document
          file={pdfUrl}
          onLoadSuccess={onDocumentLoadSuccess}
          onLoadError={onDocumentLoadError}
          loading={<div className="p-8 text-muted-foreground font-medium">Loading document...</div>}
          error={<div className="p-8 text-destructive font-medium">Failed to load PDF</div>}
          className="flex flex-col gap-6">
          {numPages &&
            Array.from(new Array(numPages), (_, index) => (
              <div
                key={`page_${index + 1}`}
                className="border border-border shadow-md bg-card rounded-xs transition-transform duration-200"
              >
                <Page
                  pageNumber={index + 1}
                  scale={scale}
                  renderTextLayer={true}
                  renderAnnotationLayer={true}
                  loading={
                    <div
                      className="bg-card flex items-center justify-center border border-border/40 "
                      style={{ width: 612 * scale, height: 792 * scale }}
                    >
                      <span className="text-sm text-muted-foreground">Loading Page {index + 1}...</span>
                    </div>
                  }
                />
              </div>
            ))
          }
        </Document>
      </div>
    </div>
  );
}
