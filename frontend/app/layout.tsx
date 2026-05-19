"use client"
import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar";
import "./globals.css";
import { AppSidebar } from "./components/AppSidebar";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

const geistSans = Geist({ variable: "--font-geist-sans", subsets: ["latin"] });
const geistMono = Geist_Mono({ variable: "--font-geist-mono", subsets: ["latin"] });
const queryClient = new QueryClient()

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}>
      <body className="h-full ">
        <QueryClientProvider client={queryClient} >

          <main className="relative flex flex-col flex-1 w-full h-dvh overflow-hidden">
            <div className="absolute top-4 left-4 z-50">
              {/* <SidebarTrigger /> */}
            </div>
            {children}
          </main>

          {/* <AppSidebar /> */}
          {/* <SidebarProvider> */}
          {/**/}
          {/* </SidebarProvider> */}

        </QueryClientProvider>
      </body>
    </html>
  );
}
