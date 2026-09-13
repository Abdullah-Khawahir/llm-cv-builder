"use client";

import { useEffect, useState } from "react";
import AppSettings from "@/lib/AppSettings";

export interface CvFont {
  id: string;
  displayName: string;
  family: string;
  source: string;
  supportsArabic: boolean;
  cssStack: string;
}

interface FontPickerProps {
  value: string;
  sessionId: string | null;
  onChange: (fontId: string) => void;
}

export default function FontPicker({ value, sessionId, onChange }: FontPickerProps) {
  const [fonts, setFonts] = useState<CvFont[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const res = await fetch(`${AppSettings.API_Base_URL}/api/fonts`);
        if (!res.ok) return;
        const data = (await res.json()) as CvFont[];
        if (!cancelled) setFonts(data);
      } catch (e) {
        console.error("Failed to load fonts", e);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const handleChange = async (fontId: string) => {
    onChange(fontId);
    if (!sessionId) return;
    setSaving(true);
    try {
      await fetch(`${AppSettings.API_Base_URL}/api/chat-sessions/${sessionId}/font`, {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ fontId }),
      });
    } catch (e) {
      console.error("Failed to save font", e);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <div className="text-xs text-muted-foreground px-1">Loading fonts…</div>;
  }

  return (
    <div className="flex items-center gap-2 px-1">
      <label htmlFor="cv-font" className="text-xs font-medium text-muted-foreground whitespace-nowrap">
        Font{saving ? " (saving…)" : ""}
      </label>
      <select
        id="cv-font"
        value={value}
        onChange={(e) => void handleChange(e.target.value)}
        className="text-xs bg-card border border-border rounded-lg px-2 py-1.5 max-w-55 cursor-pointer"
      >
        {fonts.map((f) => (
          <option key={f.id} value={f.id}>
            {f.displayName}
            {f.supportsArabic ? " (AR)" : ""}
          </option>
        ))}
      </select>
    </div>
  );
}
