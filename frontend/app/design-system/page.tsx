"use client"

import { CSSProperties, useState } from "react"
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip"
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

export default function SystemDesign() {
  const [primary, setPrimary] = useState('#525252')
  const [secondary, setSecondary] = useState('#737373')
  const [bg, setBg] = useState('#1f1f1f')
  const [text, setText] = useState('#f5f5f5')

  const themeStyle = {
    '--primary': primary,
    '--secondary': secondary,
    '--background': bg,
    '--foreground': text,
  } as CSSProperties

  const [dark, setDark] = useState(false)
  const toggleTheme = () => setDark(!dark)

  return (
    <main>
      <header>
        <section>
          <h1>System Architecture Identity</h1>
          <p>Living design token spec sheet and components dictionary.</p>
        </section>
        <button data-action="toggle-theme">Switch to Dark Mode</button>
      </header>

      <section>
        <h2>01. Typography Hierarchy</h2>
        <article>
          <div>
            <small>Editorial Display Heading</small>
            <h1>The quick brown fox jumps over the lazy dog</h1>
          </div>
          <div>
            <small>Standard UI Heading</small>
            <h2>Pack my box with five dozen liquor jugs</h2>
          </div>
          <div>
            <small>Body Large</small>
            <p>Design style is the silent ambassador of your software's underlying architectural integrity.</p>
          </div>
          <div>
            <small>Body Normal</small>
            <p>Our layout methodology relies on strict asymmetric grids, allowing information density to remain practical without overloading human cognitive limits.</p>
          </div>
          <div>
            <small>Body Muted / Meta Specs</small>
            <p><small>Component state matrix initialized. Built with hardware-accelerated CSS custom parameters.</small></p>
          </div>
        </article>
      </section>


      <section>
        <h2>02. Color Palette System</h2>
        <nav>
          <figure data-color="surface">
            <div></div>
            <figcaption>
              <strong>Surface BG</strong>
              <p><small>Component Main</small></p>
            </figcaption>
          </figure>
          <figure data-color="secondary">
            <div></div>
            <figcaption>
              <strong>Secondary BG</strong>
              <p><small>Structure Shell</small></p>
 a           </figcaption>
          </figure>
          <figure data-color="primary">
            <div></div>
            <figcaption>
              <strong>Primary Tint</strong>
              <p><small>Brand Actions</small></p>
            </figcaption>
          </figure>
          <figure data-color="error">
            <div></div>
            <figcaption>
              <strong>Error State</strong>
              <p><small>Alerts & Breaches</small></p>
            </figcaption>
          </figure>
        </nav>
      </section>

      <section>
        <h2>03. Primary Action Buttons</h2>
        <fieldset>
          <div>
            <small>Default State</small>
            <button type="button">Execute Protocol</button>
          </div>
          <div>
            <small>Hover State Mock</small>
            <button type="button" data-state="hover">Execute Protocol</button>
          </div>
          <div>
            <small>Active State Mock</small>
            <button type="button" data-state="active">Execute Protocol</button>
          </div>
          <div>
            <small>Focus State Mock</small>
            <button type="button" data-state="focus">Execute Protocol</button>
          </div>
          <div>
            <small>Disabled State</small>
            <button type="button" disabled>Execute Protocol</button>
          </div>
        </fieldset>
      </section>

      <section>
        <h2>04. Structural Form Inputs</h2>
        <fieldset>
          <div>
            <small>Empty / Default</small>
            <div>
              <label>Database URI</label>
              <input type="text" placeholder="postgresql://user:pass@host..." />
            </div>
          </div>
          <div>
            <small>Filled / Hover Mock</small>
            <div>
              <label>Database URI</label>
              <input type="text" data-state="hover" value="postgresql://admin_cluster_prod" />
            </div>
          </div>
          <div>
            <small>Active Focus Mock</small>
            <div>
              <label>Database URI</label>
              <input type="text" data-state="focus" value="postgresql://typing_active..." />
            </div>
          </div>
          <div>
            <small>Error State</small>
            <div>
              <label>Database URI</label>
              <input type="text" aria-invalid="true" value="invalid_string://" />
              <span>Malformed protocol metadata identifier connection rejected.</span>
            </div>
          </div>
          <div>
            <small>Disabled State</small>
            <div>
              <label>Database URI</label>
              <input type="text" value="Read-only protected engine" disabled />
            </div>
          </div>
        </fieldset>
      </section>

      <section>
        <h2>05. Functional Card Components</h2>
        <fieldset>
          <div>
            <small>Unselected / Default</small>
            <article>
              <h3>Standard Node</h3>
              <p>Delivers baseline data parsing pipelines across sandbox networks.</p>
            </article>
          </div>
          <div>
            <small>Selected State</small>
            <article aria-selected="true">
              <mark>ACTIVE</mark>
              <h3>Standard Node</h3>
              <p>Delivers baseline data parsing pipelines across sandbox networks.</p>
            </article>
          </div>
          <div>
            <small>Disabled State</small>
            <article aria-disabled="true">
              <h3>Standard Node</h3>
              <p>Delivers baseline data parsing pipelines across sandbox networks.</p>
            </article>
          </div>
        </fieldset>
      </section>

    </main>
  )
}

