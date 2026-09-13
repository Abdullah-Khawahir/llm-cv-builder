# CV Fonts — plug-and-play guide

Fonts are served by `FontCatalog` (`WebAPI/src/Services/Fonts/`) and listed at
`GET /api/fonts`. The picker in the chat UI and the agent's `SetFont` tool both
read from this catalog, so anything registered here works everywhere with no
code changes.

## Option 1 — Google Fonts API (recommended)

1. Get a free key at <https://developers.google.com/fonts/docs/developer_api>
   (Google Cloud project, no billing needed).
2. Set it in `.env`:
   ```
   Fonts__GoogleFontsApiKey=PASTE_KEY_HERE
   ```
3. Add the families you want to `Fonts:GoogleFontFamilies` in
   `WebAPI/appsettings.json` (8 pre-seeded: Inter, Cairo, IBM Plex Sans Arabic,
   Noto Sans, Noto Serif, Arimo, Tinos, Roboto).
4. Restart the API. On startup it downloads regular/bold/italic TTFs into
   `fonts/<slug>/`, detects Arabic support from the family's `subsets` list,
   and refreshes fontconfig.

Without a key, startup falls back to a small built-in GitHub mirror
(Inter/Cairo/IBM Plex) and everything else resolves via system fonts — it never
fails offline.

## Option 2 — drop-in folder

1. Copy `*.ttf` / `*.otf` / `*.woff` / `*.woff2` into `WebAPI/fonts/<id>/`
   (e.g. `fonts/amiri/Amiri-Regular.ttf`).
2. Add one row to `fonts/manifest.json`:
   ```json
   { "id": "amiri", "family": "Amiri", "displayName": "Amiri", "supportsArabic": true }
   ```
   (`supportsArabic` adds the `(AR)` badge; when omitted it is guessed from the
   name. `dejavu` is the built-in fallback and needs no entry.)
3. Run `POST /api/fonts/refresh` — or just restart. No rebuild needed.

Downloaded binaries are gitignored (`fonts/.gitignore`); only `manifest.json`
is tracked.

## Option 3 — upload at runtime

```
POST /api/fonts/upload   (multipart, field "file", ≤8MB)
```

Files are magic-byte validated (fake fonts → 400 and nothing is written) and
stored under `fonts/uploaded/<id>/`, appearing as `uploaded-<id>`.

## Notes & constraints

- **Arial / Times New Roman are Microsoft proprietary fonts** — they cannot come
  from Google Fonts. They resolve via `fonts-liberation2` (installed in both
  Dockerfiles): Arial → Liberation Sans, Times New Roman → Liberation Serif
  (metrically identical). For pixel-perfect Google versions use Arimo / Tinos.
- **One font per CV** (ATS rule). `PdfGenerator` injects the selected font as
  `@font-face file://` + a fallback chain ending in Arabic-capable fonts, and
  neutralizes any font the LLM invents — so prompts stay clean.
- **WeasyPrint is pinned to 70.0 via pip** (see Dockerfiles). Do NOT downgrade
  to Debian apt's 61.x: it writes a broken ToUnicode map for RTL text (Arabic
  selects as garbage in pdf.js). If text selection ever shows random symbols,
  check `weasyprint --version` inside the container first.
- **Stale PDFs:** renders are cached in Minio keyed by `hash(html + font)`.
  Changing code/fonts does not bust old entries — edit any character or flush
  the `pdfs` bucket after font changes.
- Switching a session's font: picker UI, `PATCH /api/chat-sessions/{id}/font`,
  or tell the agent ("use Cairo" → `SetFont` tool).
