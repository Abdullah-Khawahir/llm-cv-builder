export interface PDFViewerProps {
  /** URL to the PDF document */
  pdfUrl: string;
  /** Session ID for backend integration */
  sessionId: string;
  /** Callback when text is referenced */
  onTextReference?: (text: string, metadata: ReferenceMetadata) => void;
  /** Initial page number (default: 1) */
  initialPage?: number;
  /** Initial zoom scale (default: 1.2) */
  initialScale?: number;
}

export interface ReferenceMetadata {
  sessionId: string;
  pageNumber: number;
  timestamp: string;
  selectedText: string;
  // Future: Add coordinates, character offsets, etc.
}

export interface PopupPosition {
  x: number;
  y: number;
}

export interface SelectionPopupProps {
  position: PopupPosition | null;
  selectedText: string;
  onAction: (text: string) => void;
  onClose: () => void;
}

export interface UseTextSelectionReturn {
  selectedText: string;
  popupPosition: PopupPosition | null;
  showPopup: boolean;
  handleMouseUp: () => void;
  handleClosePopup: () => void;
  setSelectedText: React.Dispatch<React.SetStateAction<string>>;
  setPopupPosition: React.Dispatch<React.SetStateAction<PopupPosition | null>>;
  setShowPopup: React.Dispatch<React.SetStateAction<boolean>>;
}
