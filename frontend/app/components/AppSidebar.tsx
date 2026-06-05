"use client"

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  useSidebar,
  SidebarTrigger,
} from "@/components/ui/sidebar";

import { User2, Plus, Search } from "lucide-react";

export function AppSidebar() {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";

  return (
    <Sidebar side="left" variant="sidebar" className="h-full border-none">

      {/* HEADER */}
      <SidebarHeader className="p-2 space-y-2">

        {/* Trigger (better UX: inside header) */}
        <div className="flex items-center justify-between">
          {!collapsed && (
            <span className="text-sm font-semibold">Chats</span>
          )}

          <SidebarTrigger className="h-8 w-8 rounded-md hover:bg-muted transition" />
        </div>

        {/* New Chat */}
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton className="w-full justify-start gap-2">
              <Plus size={16} />
              {!collapsed && "New Chat"}
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>

        {/* Search */}
        {!collapsed && (
          <div className="relative">
            <Search size={14} className="absolute left-2 top-2 text-muted-foreground" />
            <input
              className="w-full rounded-md border pl-7 pr-2 py-1 text-sm"
              placeholder="Search chats..."
            />
          </div>
        )}
      </SidebarHeader>

      {/* CONTENT */}
      <SidebarContent className="p-2">
        <SidebarGroup>
          {!collapsed && (
            <div className="text-xs text-muted-foreground mb-2">
              Recent
            </div>
          )}

          <SidebarMenu>
            {[
              "React Query Guide",
              "ASP.NET Identity Flow",
              "Tailwind Sidebar UX",
              "Markdown Fix",
            ].map((chat) => (
              <SidebarMenuItem key={chat}>
                <SidebarMenuButton>
                  {!collapsed && chat}
                  {collapsed && "•"}
                </SidebarMenuButton>
              </SidebarMenuItem>
            ))}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>

      {/* FOOTER */}
      <SidebarFooter className="p-2">
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-full bg-muted flex items-center justify-center">
            <User2 size={16} />
          </div>

          {!collapsed && (
            <div className="text-xs leading-tight">
              <div className="font-medium">Abtuly M</div>
              <div className="text-muted-foreground">Free Plan</div>
            </div>
          )}
        </div>
      </SidebarFooter>
    </Sidebar>
  );
}
