"use client";

import { api } from "@/app/ApiClient";
import { Markdown } from "@/app/components/Markdown";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import AppSettings from "@/lib/AppSettings";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dynamic from "next/dynamic";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useEffect, useRef, useState, useTransition, useMemo } from "react";

const PdfViewer = dynamic(() => import("@/app/components/PDFViewer"), {
  ssr: false,
});

interface Message {
  role: "user" | "assistant";
  message: string;
}

interface ChatSession {
  id: string;
  htmlDocument?: string;
  version?: number;
  chatHistory?: {
    messages: Message[];
  };
}

export default function Home() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();
  const [, startTransition] = useTransition();

  const sessionId = searchParams.get("sid") ?? "";
  const [inputPrompt, setInputPrompt] = useState("");
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const [mounted, setMounted] = useState(false);

  const { data: session } = useChatSessionById(sessionId);
  const newSessionMutation = useCreateNewSession();
  const promptMutation = usePromptMutation();

  const messages = session?.chatHistory?.messages ?? [];

  // Protect against SSR layout hydration attribute switches
  useEffect(() => {
    setMounted(true);
  }, []);

  // Smooth scroll view context down layout safely during live streaming updates
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages.length, messages[messages.length - 1]?.message]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const trimmedPrompt = inputPrompt.trim();
    if (!trimmedPrompt || promptMutation.isPending) return;

    setInputPrompt("");

    let currentSessionId = sessionId;

    // Handle Lazy Creation of session space cleanly
    if (!currentSessionId) {
      try {
        const newSession = await newSessionMutation.mutateAsync();
        if (!newSession.id) return;
        currentSessionId = newSession.id;

        startTransition(() => {
          const params = new URLSearchParams(searchParams.toString());
          params.set("sid", currentSessionId);
          router.push(`${pathname}?${params.toString()}`);
        });
      } catch (err) {
        console.error("Failed to provision session:", err);
        return;
      }
    }

    promptMutation.mutate({ sessionId: currentSessionId, userPrompt: trimmedPrompt });
  };

  // Derive layout hash and target preview URL directly from changes to htmlDocument
  const { pdfUrl, shortHash } = useMemo(() => {
    if (!sessionId || !session?.htmlDocument) {
      return { pdfUrl: "", shortHash: "" };
    }
    const hash = quickHash(session.htmlDocument);
    return {
      pdfUrl: `${AppSettings.API_Base_URL}/api/cv/preview/${sessionId}?v=${hash}`,
      shortHash: hash,
    };
  }, [sessionId, session?.htmlDocument]);

  return (
    <main className="flex h-screen w-screen overflow-hidden bg-background text-foreground font-sans-preview">
      {/* LEFT COMPARTMENT: COMPILATION & PDF ENGINE PREVIEW */}
      <section className="relative w-1/2 h-full px-2 flex flex-col justify-between border-r border-border">
        <div className="flex-1 overflow-hidden rounded-xl border border-border bg-muted/30 shadow-xs relative">
          {pdfUrl ? (
            /* Using shortHash as a key forces a clean component remount when HTML string updates */
            <PdfViewer key={shortHash} pdfUrl={pdfUrl} />
          ) : (
            <div className="absolute inset-0 flex flex-col items-center justify-center text-muted-foreground p-12 text-center">
              <div className="w-12 h-12 mb-3 rounded-xl bg-card border border-border flex items-center justify-center text-lg shadow-xs">
                📄
              </div>
              <h3 className="text-sm font-medium text-foreground">No Document Rendered</h3>
              <p className="text-xs text-muted-foreground max-w-xs mt-1">
                Provide structural modifications within the conversational workspace to stream updates.
              </p>
            </div>
          )}
        </div>
      </section>

      {/* RIGHT COMPARTMENT: ORCHESTRATION & CHAT WORKSPACE */}
      <section className="w-1/2 h-full flex flex-col bg-card">
        {/* INTERACTIVE MESSAGE PIPELINE */}
        <div className="flex-1 overflow-y-auto px-6 py-6 space-y-6 scrollbar-thin scrollbar-thumb-border bg-background/40">
          {messages.map((msg, idx) => {
            const isUser = msg.role === "user";
            return (
              <div key={idx} className={`flex ${isUser ? "justify-end" : "justify-start"}`}>
                <div
                  className={`relative max-w-[85%] px-4 py-3.5 rounded-xl border transition-all ${isUser
                    ? "bg-card text-foreground border-border shadow-xs dark:bg-accent dark:text-accent-foreground dark:border-border/50"
                    : "bg-muted/40 text-foreground border-border/60 shadow-2xs"
                    }`}
                >
                  <span
                    className={`block text-[10px] font-mono uppercase tracking-wider mb-1.5 ${isUser ? "text-accent-foreground/70" : "text-muted-foreground opacity-70"
                      }`}
                  >
                    {isUser ? `${idx} / User Request` : `${idx} / Assistant Output`}
                  </span>

                  <div className="prose prose-sm max-w-none overflow-hidden dark:prose-invert text-foreground">
                    {msg.message ? (
                      <Markdown content={msg.message.trim()} />
                    ) : (
                      <div className="flex gap-1.5 py-2 items-center">
                        <span className="w-1.5 h-1.5 bg-muted-foreground rounded-full animate-bounce [animation-delay:0ms]" />
                        <span className="w-1.5 h-1.5 bg-muted-foreground rounded-full animate-bounce [animation-delay:150ms]" />
                        <span className="w-1.5 h-1.5 bg-muted-foreground rounded-full animate-bounce [animation-delay:300ms]" />
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
          <div ref={messagesEndRef} />
        </div>

        {/* INPUT SUBMISSION MATRIX */}
        <div className="p-6 border-t border-border bg-card">
          <form onSubmit={handleSubmit} className="space-y-3">
            <div className="space-y-1.5">
              <Textarea
                rows={4}
                value={inputPrompt}
                onChange={(e) => setInputPrompt(e.target.value)}
                placeholder="e.g., 'Add my experience with system architecture' or 'Refactor styling options'..."
                className="w-full resize-none bg-background border-border text-sm rounded-xl focus-visible:ring-ring focus-visible:ring-offset-0"
              />
            </div>

            <div className="flex items-center justify-between pt-1">
              <div className="text-[11px] text-muted-foreground">
                {mounted && promptMutation.isPending && (
                  <span className="flex items-center gap-1.5 text-amber-500 font-mono">
                    <span className="w-2 h-2 rounded-full bg-amber-500 animate-ping" />
                    Executing data pipe streaming...
                  </span>
                )}
              </div>
              <Button
                type="submit"
                size="sm"
                variant={mounted && inputPrompt.trim() ? "default" : "secondary"}
                disabled={!mounted || (!inputPrompt.trim() || promptMutation.isPending)}
                className="font-semibold px-5 rounded-lg shadow-xs"
              >
                {mounted && promptMutation.isPending ? "Generating..." : "Apply Prompt"}
              </Button>
            </div>
          </form>
        </div>
      </section>
    </main>
  );
}

function usePromptMutation() {
  const qc = useQueryClient();

  return useMutation({
    mutationFn: async ({ sessionId, userPrompt }: { sessionId: string; userPrompt: string }) => {
      const response = await fetch(`${AppSettings.API_Base_URL}/api/chat-sessions/${sessionId}/stream`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt: userPrompt }),
      });

      if (!response.ok || !response.body) {
        throw new Error("Stream connection unexpected diagnostic failure");
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();

      let assistantText = "";
      let lineBuffer = "";

      // Batch frequency throttle limit for cache injections (reduces component rendering overhead)
      let lastUpdateTime = Date.now();
      const THROTTLE_MS = 60;

      // Optimistic cache update injection to simulate snappy text rendering
      qc.setQueryData(["session", sessionId], (old: ChatSession | undefined) => ({
        ...old,
        id: sessionId,
        chatHistory: {
          messages: [
            ...(old?.chatHistory?.messages || []),
            { role: "user", message: userPrompt },
            { role: "assistant", message: "" },
          ],
        },
      }));

      try {
        while (true) {
          const { value, done } = await reader.read();
          if (done) break;

          lineBuffer += decoder.decode(value, { stream: true });
          const lines = lineBuffer.split("\n");
          lineBuffer = lines.pop() || "";

          for (const line of lines) {
            const cleanedLine = line.trim();
            if (!cleanedLine.startsWith("data: ")) continue;

            const rawJson = cleanedLine.slice(6).trim();
            if (!rawJson) continue;

            try {
              const data = JSON.parse(rawJson);
              console.log(data)
              const eventType = (data.Type || data.type || "").toLowerCase();

              if (eventType === "token") {
                const content = data.Content ?? data.content ?? "";
                assistantText += content;

                const now = Date.now();
                if (now - lastUpdateTime > THROTTLE_MS) {
                  lastUpdateTime = now;
                  qc.setQueryData(["session", sessionId], (old: ChatSession | undefined) => {
                    const msgs = [...(old?.chatHistory?.messages || [])];
                    if (msgs.length > 0) {
                      msgs[msgs.length - 1] = { role: "assistant", message: assistantText };
                    }
                    return { ...old, chatHistory: { messages: msgs } };
                  });
                }
              } else if (eventType === "updated") {
                // Exact model object layout pass-through
                const freshSession = data.ChatSessionDto || data.chatSessionDto || data;
                if (freshSession) {
                  qc.setQueryData(["session", sessionId], freshSession);
                }
              }
            } catch (e) {
              console.error("Error parsing stream line payload:", e, line);
            }
          }
        }
      } finally {
        reader.releaseLock();
      }

      // Sync and force layout execution for any lagging non-throttled text chunks
      qc.setQueryData(["session", sessionId], (old: ChatSession | undefined) => {
        const msgs = [...(old?.chatHistory?.messages || [])];
        if (msgs.length > 0) {
          msgs[msgs.length - 1] = { role: "assistant", message: assistantText };
        }
        return { ...old, chatHistory: { messages: msgs } };
      });
    },
  });
}

function useChatSessionById(id: string) {
  return useQuery({
    queryKey: ["session", id],
    queryFn: async () => (await api.chatSessionsDetail(id)).data,
    enabled: !!id,
    staleTime: Infinity,
    refetchOnMount: false,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  });
}

function quickHash(str: string): string {
  if (!str) return "0";
  let hash = 5381;
  for (let i = 0; i < str.length; i++) {
    hash = (hash * 33) ^ str.charCodeAt(i);
  }
  return (hash >>> 0).toString(16);
}

function useCreateNewSession() {
  return useMutation({
    mutationKey: ["sessions-list"],
    mutationFn: async () => (await api.chatSessionsCreate()).data,
  });
}
