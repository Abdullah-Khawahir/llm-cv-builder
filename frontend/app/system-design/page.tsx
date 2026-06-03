"use client";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { useState } from "react";

type FontClass = "font-sans-preview" | "font-serif-preview" | "font-mono-preview" | "font-display-preview";

export default function DesignSystemPage() {
  const [currentFont, setCurrentFont] = useState<FontClass>("font-sans-preview");

  return (
    <div className={`p-8 max-w-6xl mx-auto space-y-12 ${currentFont} transition-all duration-200 bg-background text-foreground`}>

      {/* Header Controls & Font Switcher */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 pb-6 border-b border-border">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">System Design Kitchen Sink</h1>
          <p className="text-muted-foreground text-sm mt-1">Global interface variables, inputs, and semantic layout patterns.</p>
        </div>

        {/* Dynamic Font Controller */}
        <div className="flex items-center gap-2 bg-muted p-1.5 rounded-lg border border-border">
          <span className="text-xs font-mono px-2 text-muted-foreground">FONT VAR:</span>
          {(["font-sans-preview", "font-serif-preview", "font-mono-preview", "font-display-preview"] as FontClass[]).map((f) => (
            <button
              key={f}
              onClick={() => setCurrentFont(f)}
              className={`px-3 py-1 text-xs font-medium rounded-md transition-all ${currentFont === f
                ? "bg-card text-card-foreground shadow-xs border border-border"
                : "text-muted-foreground hover:text-foreground"
                }`}
            >
              {f.replace("-preview", "").replace("font-", "")}
            </button>
          ))}
        </div>
      </div>

      {/* Section 1: Typography & Buttons Row Matchup */}
      <section className="space-y-6">
        <h2 className="text-sm font-mono tracking-wider text-muted-foreground uppercase">01 / Typography & Inline Action Alignments</h2>
        <Card className="overflow-hidden border border-border bg-card text-card-foreground">
          <CardContent className="p-6 space-y-4 divide-y divide-border">

            {/* H1 Row */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 items-center pt-2 first:pt-0">
              <div className="lg:col-span-4"><h1 className="text-4xl font-extrabold tracking-tight text-foreground">Header 1</h1></div>
              <div className="lg:col-span-8 flex flex-wrap gap-2 items-center">
                <Button variant="default">Primary Action</Button>
                <Button variant="secondary">Secondary</Button>
                <Button variant="destructive">Error State</Button>
                <Button variant="outline">Outline</Button>
                <Button variant="ghost">Ghost</Button>
              </div>
            </div>

            {/* H2 Row */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 items-center pt-4">
              <div className="lg:col-span-4"><h2 className="text-3xl font-semibold tracking-tight text-foreground">Header 2</h2></div>
              <div className="lg:col-span-8 flex flex-wrap gap-2 items-center">
                <Button size="sm" variant="default">Primary Action</Button>
                <Button size="sm" variant="secondary">Secondary</Button>
                <Button size="sm" variant="destructive">Error State</Button>
                <Button size="sm" variant="outline">Outline</Button>
                <Button size="sm" variant="ghost">Ghost</Button>
              </div>
            </div>

            {/* H3 Row */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 items-center pt-4">
              <div className="lg:col-span-4"><h3 className="text-2xl font-semibold tracking-tight text-foreground">Header 3</h3></div>
              <div className="lg:col-span-8 flex flex-wrap gap-2 items-center">
                <Button size="sm" className="h-8" variant="default">Primary Action</Button>
                <Button size="sm" className="h-8" variant="secondary">Secondary</Button>
                <Button size="sm" className="h-8" variant="destructive">Error State</Button>
              </div>
            </div>

            {/* H4 Row */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 items-center pt-4">
              <div className="lg:col-span-4"><h4 className="text-xl font-medium tracking-tight text-foreground">Header 4</h4></div>
              <div className="lg:col-span-8 flex flex-wrap gap-2 items-center">
                <span className="text-xs font-mono text-muted-foreground mr-2">[Disabled variants]</span>
                <Button disabled variant="default">Primary</Button>
                <Button disabled variant="secondary">Secondary</Button>
                <Button disabled variant="destructive">Error</Button>
              </div>
            </div>

            {/* H5 Row */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 items-center pt-4">
              <div className="lg:col-span-4"><h5 className="text-lg font-medium text-foreground">Header 5</h5></div>
              <div className="lg:col-span-8 flex flex-wrap gap-4 items-center">
                <label className="flex items-center gap-2 text-sm text-card-foreground/80 cursor-pointer">
                  <input type="checkbox" defaultChecked className="rounded border-input text-primary focus:ring-ring bg-background" />
                  Inline Checkbox
                </label>
                <label className="flex items-center gap-2 text-sm text-card-foreground/80 cursor-pointer">
                  <input type="radio" defaultChecked className="border-input text-primary focus:ring-ring bg-background" />
                  Inline Radio
                </label>
              </div>
            </div>

            {/* H6 Row */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-4 items-center pt-4">
              <div className="lg:col-span-4"><h6 className="text-sm font-semibold uppercase tracking-wider text-muted-foreground">Header 6</h6></div>
              <div className="lg:col-span-8 flex items-center gap-2">
                <Switch id="design-system-switch" />
                <label htmlFor="design-system-switch" className="text-xs text-muted-foreground font-mono">TOGGLE FORM ELEMENT</label>
              </div>
            </div>

          </CardContent>
        </Card>
      </section>

      {/* Section 2: Input Field Matrix */}
      <section className="space-y-6">
        <h2 className="text-sm font-mono tracking-wider text-muted-foreground uppercase">02 / Comprehensive Input Matrix</h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

          {/* Text and Value Controls */}
          <Card className="border border-border bg-card text-card-foreground">
            <CardHeader><CardTitle className="text-base font-semibold text-foreground">Standard Text Fields</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-1.5">
                <label className="text-xs font-medium text-card-foreground/90">Text Entry</label>
                <Input type="text" placeholder="John Doe / Standard Input" />
              </div>

              <div className="space-y-1.5">
                <label className="text-xs font-medium text-card-foreground/90">Numerical Range/Count</label>
                <Input type="number" placeholder="2026" min="1900" max="2100" />
              </div>

              <div className="space-y-1.5">
                <label className="text-xs font-medium text-card-foreground/90">Temporal/Date Bounds</label>
                <Input type="date" className="w-full text-left" />
              </div>
            </CardContent>
          </Card>

          {/* Complex and Block Text Controls */}
          <Card className="border border-border bg-card text-card-foreground">
            <CardHeader><CardTitle className="text-base font-semibold text-foreground">Area & Selection Utilities</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-1.5">
                <label className="text-xs font-medium text-card-foreground/90">Long Text / Bio Description</label>
                <Textarea placeholder="Describe profile summaries, experience metrics or system instructions here..." rows={4} />
              </div>

              <div className="grid grid-cols-2 gap-4 pt-2">
                <div className="space-y-2">
                  <span className="block text-xs font-medium text-card-foreground/90">Select Options</span>
                  <div className="space-y-2">
                    {["Option Uniform", "Option Vector", "Option Minimal"].map((opt, idx) => (
                      <label key={opt} className="flex items-center gap-2 text-sm text-card-foreground/80 cursor-pointer">
                        <input
                          type="radio"
                          name="matrix-radio"
                          defaultChecked={idx === 0}
                          className="w-4 h-4 text-primary border-input focus:ring-ring bg-background"
                        />
                        {opt}
                      </label>
                    ))}
                  </div>
                </div>

                <div className="space-y-2">
                  <span className="block text-xs font-medium text-card-foreground/90">Multi Selection</span>
                  <div className="space-y-2">
                    {["Verify Output", "PCI Compliance", "In-Memory Render"].map((chk, idx) => (
                      <label key={chk} className="flex items-center gap-2 text-sm text-card-foreground/80 cursor-pointer">
                        <input
                          type="checkbox"
                          defaultChecked={idx < 2}
                          className="w-4 h-4 rounded text-primary border-input focus:ring-ring bg-background"
                        />
                        {chk}
                      </label>
                    ))}
                  </div>
                </div>
              </div>

            </CardContent>
          </Card>

        </div>
      </section>

      {/* Section 3: Native HTML Content Restoration */}
      <section className="space-y-6">
        <h2 className="text-sm font-mono tracking-wider text-muted-foreground uppercase">03 / Native HTML Element Defaults</h2>
        <Card className="border border-border bg-card text-card-foreground">
          <CardContent className="p-6">
            <p className="text-muted-foreground">
              This section previews global styles applied directly to standard base elements. This ensures parsed content blocks (like raw text fields or documentation nodes) keep structured presentation logic cleanly.
            </p>

            {/* Render Matrix / Table Preview */}
            <table className="border-border">
              <thead className="bg-muted">
                <tr className="border-border">
                  <th className="text-foreground">Package Name</th>
                  <th className="text-foreground">Version Target</th>
                  <th className="text-foreground">Architecture Strategy</th>
                </tr>
              </thead>
              <tbody>
                <tr className="border-border/40">
                  <td><code>Npgsql.Json.NET</code></td>
                  <td>v10.0.0-preview</td>
                  <td>Database Native Metadata mapping</td>
                </tr>
                <tr className="border-border/40">
                  <td><code>LanguageExt.Core</code></td>
                  <td>v4.4.0</td>
                  <td>Functional pipeline type safety</td>
                </tr>
                <tr className="border-border/40">
                  <td><code>Microsoft.SemanticKernel</code></td>
                  <td>v1.14.0</td>
                  <td>AI LLM Streaming orchestration</td>
                </tr>
              </tbody>
            </table>

            {/* Blockquote and Code block combo */}
            <blockquote className="border-border text-foreground/90">
              "Systems must scale out cleanly without carrying legacy state. Maintain local modular constraints everywhere possible."
            </blockquote>

            <pre className="bg-secondary text-secondary border-primary-100">

              <code>
                {`// Example validation pipe configuration
                public static Result<User> VerifyIdentity(UserRequest request) =>
                request.HasValidToken() 
                  ? Result<User>.Success(new User(request.Id))
                  : Result<User>.Failure("Unauthorized Token Sequence");`
                }</code>

            </pre>

            {/* Structural Lists */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6">
              <div>
                <span className="text-xs font-mono text-muted-foreground">UNORDERED LIST</span>
                <ul className="text-muted-foreground">
                  <li>Isolated Containerized execution environments</li>
                  <li>In-memory layout generation and compilation
                    <ul>
                      <li>WeasyPrint base configurations</li>
                      <li>No external execution dependencies</li>
                    </ul>
                  </li>
                  <li>Zero-vendor multi-cloud orchestration targets</li>
                </ul>
              </div>

              <div>
                <span className="text-xs font-mono text-muted-foreground">ORDERED STEPS</span>
                <ol className="text-muted-foreground">
                  <li>Parse unstructured data streams into JSON structures</li>
                  <li>Map properties directly to targeted fields</li>
                  <li>Compile binaries and verify diagnostic signals</li>
                </ol>
              </div>
            </div>

          </CardContent>
        </Card>
      </section>

    </div>
  );
}
