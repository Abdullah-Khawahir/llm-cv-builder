"use client"

import { AppSidebar } from "@/app/components/AppSidebar"
import { SidebarProvider, SidebarTrigger, useSidebar } from "@/components/ui/sidebar"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import "./globals.css"

const queryClient = new QueryClient()

function SidebarNav() {
  const sidebar = useSidebar()
  if (sidebar.state === "expanded") return null

  return (
    <nav className="h-full w-12 flex flex-col items-center">
      <header className="h-12 flex items-center px-2">
        <SidebarTrigger className="h-8 w-8 rounded-md hover:bg-muted border border-muted-foreground/20" />
      </header>
    </nav>
  )
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html>
      <body>
        <QueryClientProvider client={queryClient}>
          <SidebarProvider>
            <AppSidebar />
            <SidebarNav />
            {children}
          </SidebarProvider>
        </QueryClientProvider>
      </body>
    </html>
  )
}
