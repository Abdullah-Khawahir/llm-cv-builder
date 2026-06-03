"use client"

import { useCallback, useEffect, useRef, memo } from 'react';
import type { SelectionPopupProps, PopupPosition } from '../types/pdf';

/**
 * Popup component that appears when text is selected in the PDF
 * Shows a "Reference" button to capture the selected text
 */
export const SelectionPopup: React.FC<SelectionPopupProps> = memo(({ 
  position, 
  selectedText, 
  onAction, 
  onClose 
}) => {
  const popupRef = useRef<HTMLDivElement>(null);

  // Close popup when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent): void => {
      if (popupRef.current && !popupRef.current.contains(event.target as Node)) {
        onClose();
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [onClose]);

  const handleReferenceClick = useCallback((): void => {
    if (selectedText.trim()) {
      onAction(selectedText);
    }
    onClose();
  }, [selectedText, onAction, onClose]);

  // Don't render if no position or empty selection
  if (!position || !selectedText?.trim()) {
    return null;
  }

  // Truncate preview text
  const previewText = selectedText.length > 100 
    ? `${selectedText.substring(0, 100)}...` 
    : selectedText;

  return (
    <div 
      ref={popupRef}
      className="selection-popup"
      style={{ 
        top: position.y, 
        left: position.x,
        position: 'fixed' as const,
        zIndex: 1000 
      }}
      role="dialog"
      aria-label="Text selection actions"
    >
      <div className="popup-content">
        <button 
          className="popup-btn"
          onClick={handleReferenceClick}
          title="Reference this text"
          type="button"
        >
          🔗 Reference
        </button>
        <button 
          className="popup-btn popup-btn-secondary"
          onClick={onClose}
          title="Close"
          type="button"
          aria-label="Close popup"
        >
          ✕
        </button>
      </div>
      
      {/* Preview of selected text */}
      <div className="popup-preview" aria-live="polite">
        <em>"{previewText}"</em>
      </div>
    </div>
  );
});

SelectionPopup.displayName = 'SelectionPopup';

export default SelectionPopup;
