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

  // Local state container to isolate lightning-fast stream chunks from cache mutations
  const [assistantStreamingText, setAssistantStreamingText] = useState("");

  const { data: session } = useChatSessionById(sessionId);
  const newSessionMutation = useCreateNewSession();
  const promptMutation = usePromptMutation(setAssistantStreamingText);

  // Compute messages reactively by overlaying live stream state onto your query data cache
  const messages = useMemo(() => {
    const cachedMessages = session?.chatHistory?.messages ?? [];
    if (assistantStreamingText) {
      return [
        ...cachedMessages,
        { role: "assistant" as const, message: assistantStreamingText },
      ];
    }
    return cachedMessages;
  }, [session, assistantStreamingText]);

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

  const hash = quickHash(session?.htmlDocument!);
  const pdfUrl = session?.htmlDocument ? `${AppSettings.API_Base_URL}/api/cv/preview/${sessionId}?v=${hash}` : null;

  return (
    <main className="flex h-screen w-screen overflow-hidden bg-background text-foreground font-sans-preview">
      {/* LEFT COMPARTMENT: COMPILATION & PDF ENGINE PREVIEW */}
      <section className="relative w-1/2 h-full px-2 flex flex-col justify-between border-r border-border">
        <div className="flex-1 overflow-hidden rounded-xl border border-border bg-muted/30 shadow-xs relative">
          {(pdfUrl && session?.htmlDocument) ? (
            <PdfViewer key={hash} pdfUrl={pdfUrl} />
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

function usePromptMutation(
  setAssistantStreamingText: React.Dispatch<React.SetStateAction<string>>
) {
  const qc = useQueryClient();

  return useMutation({
    mutationKey: ['sessions-list'],
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

      // Append user prompt cleanly into backend cache before data resolution loops execute
      qc.setQueryData(["session", sessionId], (old: ChatSession | undefined) => ({
        ...old,
        chatHistory: {
          messages: [
            ...(old?.chatHistory?.messages || []),
            { role: "user", message: userPrompt },
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
              const eventType = (data.Type || data.type || "").toLowerCase();

              if (eventType === "token") {
                const content = data.Content ?? data.content ?? "";
                assistantText += content;

                // Push token into local fast-rendering react hook
                setAssistantStreamingText(assistantText);
              }

              else if (eventType === "session_update") {
                const freshSession = data.ChatSessionDto || data.chatSessionDto || data;

                if (freshSession) {
                  qc.setQueryData(["session", sessionId], (old: ChatSession | undefined) => {
                    // Adapt safely to both C# backend PascalCase arrays or camelCase objects
                    const backendHistory = freshSession.ChatHistory || freshSession.chatHistory;
                    const backendMessages = backendHistory?.Messages || backendHistory?.messages;

                    const normalizedMessages = backendMessages
                      ? backendMessages.map((m: any) => ({
                        role: (m.Role || m.role || "assistant").toLowerCase(),
                        message: m.Message || m.message || ""
                      }))
                      : (old?.chatHistory?.messages || []);

                    return {
                      ...old,
                      id: sessionId,
                      htmlDocument: freshSession.HtmlDocument ?? freshSession.htmlDocument ?? old?.htmlDocument,
                      version: freshSession.Version ?? freshSession.version ?? old?.version,
                      chatHistory: {
                        messages: normalizedMessages.length > 0 ? normalizedMessages : old?.chatHistory?.messages || []
                      },
                    };
                  });
                }
                console.log('Session metadata synched cleanly from pipeline end chunk.');
              }
            } catch (e) {
              console.error("Error parsing stream line payload:", e, line);
            }
          }
        }
      } finally {
        reader.releaseLock();
      }

      // Flush final structural message chunk directly back into TanStack Query Cache
      qc.setQueryData(["session", sessionId], (old: ChatSession | undefined) => {
        // Exclude the user prompt added optimistically so items are not duplicated
        const previousMessages = old?.chatHistory?.messages || [];
        const finalMessages = [...previousMessages];

        // Safety check to avoid double injection if backend already pushed history down session_update
        const lastMessage = finalMessages[finalMessages.length - 1];
        if (!lastMessage || lastMessage.role !== "assistant" || lastMessage.message !== assistantText) {
          finalMessages.push({ role: "assistant", message: assistantText });
        }

        return {
          ...old,
          chatHistory: { messages: finalMessages }
        };
      });

      // Erase streaming layout holder
      setAssistantStreamingText("");
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
  if (!str) str = '';
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
