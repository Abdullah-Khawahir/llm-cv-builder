"use client";

import { api } from "@/app/ApiClient";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  SidebarTrigger,
  useSidebar,
} from "@/components/ui/sidebar";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { User2, Plus, Search } from "lucide-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";

export function AppSidebar() {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";
  const [searchFilter, setSearchFilter] = useState("")
  const sessions = useSessions();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const updateParams = (updates: Record<string, string | null>) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      if (!value) params.delete(key);
      else params.set(key, value);
    });

    router.push(`${pathname}?${params.toString()}`);
  };

  const handleNewChat = () => {
    updateParams({ sid: null });
  };


  return (
    <Sidebar side="left" variant="sidebar" className="h-full border-none">

      {/* HEADER */}
      <SidebarHeader className="p-2 space-y-3">
        <div className="flex items-center justify-between">
          {!collapsed && (
            <span className="text-sm font-semibold">Chats</span>
          )}
          <SidebarTrigger className="h-8 w-8 rounded-md hover:bg-muted" />
        </div>

        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton onClick={handleNewChat}>
              <Plus size={16} />
              {!collapsed && "New Chat"}
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>

        {!collapsed && (
          <div className="relative">
            <Search size={14} className="absolute left-2 top-2 text-muted-foreground" />
            <input
              value={searchFilter}
              onChange={(e) => setSearchFilter(e.currentTarget.value)}
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
            {sessions.data
              ?.filter(s => s.title?.toLowerCase().includes(searchFilter.toLowerCase()))
              .map((session) => (
                <SidebarMenuItem key={session.id}>
                  <SidebarMenuButton
                    asChild
                    className="h-fit text-sm"
                    isActive={searchParams.get("sid") === session.id}
                  >
                    <Link href={`/?sid=${session.id}`}>
                      {!collapsed && session.title}
                    </Link>
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

function useSessions() {
  return useQuery({
    queryKey: ["sessions-list"],
    queryFn: async () => (await api.chatSessionsList()).data,
  });
}
