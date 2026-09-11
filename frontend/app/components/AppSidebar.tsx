"use client";

import { api } from "@/app/ApiClient";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarTrigger,
  useSidebar,
} from "@/components/ui/sidebar";

import { useQuery, useQueryClient } from "@tanstack/react-query";

import {
  MoreHorizontal,
  Plus,
  Search,
  User2,
  Trash2,
  Pencil,
} from "lucide-react";

import Link from "next/link";

import {
  usePathname,
  useRouter,
  useSearchParams,
} from "next/navigation";

import {
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";

export function AppSidebar() {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";

  const queryClient = useQueryClient();

  const sessions = useSessions();

  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();

  const activeSessionId = searchParams.get("sid");

  const [searchFilter, setSearchFilter] = useState("");
  const [openMenuId, setOpenMenuId] = useState<string | null>(null);

  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (
        menuRef.current &&
        !menuRef.current.contains(e.target as Node)
      ) {
        setOpenMenuId(null);
      }
    };

    document.addEventListener("mousedown", handler);

    return () =>
      document.removeEventListener("mousedown", handler);
  }, []);

  const updateParams = (
    updates: Record<string, string | null>
  ) => {
    const params = new URLSearchParams(
      searchParams.toString()
    );

    Object.entries(updates).forEach(([k, v]) => {
      if (!v) params.delete(k);
      else params.set(k, v);
    });

    router.push(`${pathname}?${params}`);
  };

  const handleNewChat = () => {
    updateParams({ sid: null });
  };

  const renameSession = async (
    id: string,
    currentTitle: string
  ) => {
    const title = prompt(
      "Rename chat",
      currentTitle
    );

    if (!title?.trim()) return;

    queryClient.setQueryData(
      ["sessions-list"],
      (old: any[]) =>
        old?.map((s) =>
          s.id === id
            ? { ...s, title }
            : s
        )
    );

    try {
      await api.chatSessionsPartialUpdate(id, {
        title,
      });
    } catch {
      queryClient.invalidateQueries({
        queryKey: ["sessions-list"],
      });
    }
  };

  const deleteSession = async (id: string) => {
    const ok = confirm(
      "Delete this conversation?"
    );

    if (!ok) return;

    const previous =
      queryClient.getQueryData([
        "sessions-list",
      ]);

    queryClient.setQueryData(
      ["sessions-list"],
      (old: any[]) =>
        old?.filter((x) => x.id !== id)
    );

    try {
      await api.chatSessionsDelete(id);

      if (activeSessionId === id) {
        updateParams({ sid: null });
      }
    } catch {
      queryClient.setQueryData(
        ["sessions-list"],
        previous
      );
    }
  };


  const filteredSessions = useMemo(() => {
    const data = sessions.data ?? [];

    return data
      .filter((s) =>
        s.title
          ?.toLowerCase()
          .includes(
            searchFilter.toLowerCase()
          )
      )
      .sort((a, b) => {
        return (
          new Date(
            b.updatedAt ?? b.createdAt ?? 0
          ).getTime() -
          new Date(
            a.updatedAt ?? a.createdAt ?? 0
          ).getTime()
        );
      });
  }, [
    sessions.data,
    searchFilter,
  ]);

  return (
    <Sidebar
      side="left"
      variant="sidebar"
      className="h-full border-none"
    >
      <SidebarHeader className="p-2 space-y-3">
        <div className="flex items-center justify-between">
          {!collapsed && (
            <span className="font-semibold text-sm">
              Chats
            </span>
          )}

          <SidebarTrigger />
        </div>

        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              onClick={handleNewChat}
            >
              <Plus size={16} />
              {!collapsed &&
                "New Chat"}
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>

        {!collapsed && (
          <div className="relative">
            <Search
              size={14}
              className="absolute left-2 top-2 text-muted-foreground"
            />

            <input
              value={searchFilter}
              onChange={(e) =>
                setSearchFilter(
                  e.target.value
                )
              }
              placeholder="Search chats..."
              className="w-full rounded-md border pl-7 py-1 pr-2 text-sm"
            />
          </div>
        )}
      </SidebarHeader>

      <SidebarContent className="p-2">
        <SidebarGroup>
          {!collapsed && (
            <div className="mb-2 text-xs text-muted-foreground">
              {filteredSessions.length} chats
            </div>
          )}

          <SidebarMenu>
            {filteredSessions.map(
              (session) => {
                const active = session.id === activeSessionId;
                return (
                  <SidebarMenuItem
                    key={session.id}
                    className="group relative"
                  >
                    <div
                      className={`flex items-center gap-2 rounded-md px-2 py-1 ${active
                        ? "bg-secondary"
                        : "hover:bg-muted"
                        }`}
                    >
                      <Link href={`/chat?sid=${session.id}`} className="flex-1 truncate text-sm" >  {session.title} </Link>

                      {!collapsed && (
                        <button
                          onClick={() =>
                            setOpenMenuId(
                              openMenuId ===
                                session.id
                                ? null
                                : session.id
                            )
                          }
                          className="opacity-0 transition-opacity group-hover:opacity-100"
                        >
                          <MoreHorizontal size={16} />
                        </button>
                      )}
                    </div>

                    {openMenuId ===
                      session.id && (
                        <div
                          ref={menuRef}
                          className="absolute right-2 top-8 z-50 w-44 rounded-md border bg-background shadow-lg"
                        >
                          <button
                            onClick={() => {
                              renameSession(
                                session.id,
                                session.title
                              );

                              setOpenMenuId(
                                null
                              );
                            }}
                            className="flex w-full items-center gap-2 px-3 py-2 text-sm hover:bg-muted"
                          >
                            <Pencil size={14} />
                            Rename
                          </button>



                          <button
                            onClick={() => {
                              deleteSession(
                                session.id
                              );

                              setOpenMenuId(
                                null
                              );
                            }}
                            className="flex w-full items-center gap-2 px-3 py-2 text-sm text-destructive hover:bg-muted"
                          >
                            <Trash2 size={14} />
                            Delete
                          </button>
                        </div>
                      )}
                  </SidebarMenuItem>
                );
              }
            )}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter className="p-2">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted">
            <User2 size={16} />
          </div>

          {!collapsed && (
            <div className="text-xs">
              <div className="font-medium">
                Abtuly M
              </div>
              <div className="text-muted-foreground">
                Free Plan
              </div>
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
    queryFn: async () =>
      (await api.chatSessionsList()).data,
  });
}
