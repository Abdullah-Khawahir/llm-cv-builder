"use client";
import { jsPDF } from "jspdf";
import React, { useState } from "react";
import Markdown from "react-markdown";
import { api } from "@/app/ApiClient";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export default function Home() {
  const sessionId = "bd4b8ce2-4916-4f96-b642-f243823474d7";
  const queryClient = useQueryClient();

  const { data, refetch: refetchSession, isLoading } = useChatSessionById(sessionId);

  const [showChat, setShowChat] = useState(false);
  const [prompt, setPrompt] = useState("");

  const [streamingMessage, setStreamingMessage] = useState("");

  const savedMessages = (data?.chatHistory ?? [])
    .filter((m: any) => {
      const label = m?.role?.label?.toLowerCase?.();
      return label && ["user", "assistant"].includes(label);
    })
    .map((m: any) => ({
      role: m.role.label.toLowerCase(),
      text: (m.items ?? [])
        .filter((i: any) => i?.$type === "TextContent")
        .map((i: any) => i?.text ?? "")
        .join("\n"),
    }));

  // Setup your custom fetch streaming mutation
  const promptMutation = useMutation({
    mutationFn: async (userPrompt: string) => {
      setStreamingMessage("");
      setShowChat(true);

      const response = await fetch(`http://localhost:5044/api/ChatSession/chat/${sessionId}/stream`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(userPrompt),
      });

      if (!response.ok || !response.body) {
        throw new Error("Failed to initialize response stream.");
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let accumulatedText = "";

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        accumulatedText += chunk;

        setStreamingMessage(accumulatedText);
      }

      return accumulatedText;
    },
    onSuccess: () => {
      setStreamingMessage("");
      queryClient.invalidateQueries({ queryKey: ["session", sessionId] });
    },
  });

  const generate = () => {
    const pdf = new jsPDF();

    pdf.html(data?.htmlDocument ?? "", {
      callback: (doc) => {
        doc.save("document.pdf");
      },
      x: 10,
      y: 10,
      margin: [10, 10, 10, 10],
      autoPaging: "text",
    });
  };

  const sendPrompt = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!prompt.trim() || promptMutation.isPending) return;

    const currentPrompt = prompt;
    setPrompt("");
    await promptMutation.mutateAsync(currentPrompt);
    await refetchSession()
  };

  if (isLoading) {
    return (
      <main className="flex h-screen items-center justify-center bg-background text-muted-foreground">
        <div className="flex flex-col items-center gap-3">
          <div className="h-6 w-6 animate-spin rounded-full border-2 border-muted border-t-foreground" />
          <span className="text-sm font-medium tracking-wide">Loading Workspace…</span>
        </div>
      </main>
    );
  }

  // Constructing isolated context styling for the iframe document string
  const isolatedHtmlSrcDoc = data?.htmlDocument ?? "";

  return (
    <main className="w-full h-screen bg-background text-foreground overflow-hidden relative antialiased selection:bg-accent selection:text-accent-foreground">
      <div className="absolute inset-0 flex flex-row h-full w-full overflow-hidden">
        <Button onClick={generate}>download</Button>
        {/* LEFT WORKSPACE SIDEBAR */}
        {showChat && (
          <>
            <div
              id="project-page-sidebar-panel"
              className="flex flex-col h-full bg-card border-r border-border transition-all duration-300 overflow-hidden"
              style={{ flex: "35 1 0px", minWidth: "320px" }}
            >
              {/* Sidebar Header */}
              <div className="flex flex-row items-center justify-between h-12 px-4 border-b border-border bg-muted/30 flex-shrink-0">
                <div className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-primary" />
                  <span className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Assistant History</span>
                </div>
                <Button
                  variant="ghost"
                  onClick={() => setShowChat(false)}
                  className="h-7 px-2 text-xs text-muted-foreground hover:text-foreground rounded-md"
                >
                  Hide panel
                </Button>
              </div>

              {/* Chat Thread Interface Messages Area */}
              <div className="flex-1 overflow-y-auto p-4 space-y-4 app-scrollbar bg-background">
                {savedMessages.length === 0 && !promptMutation.isPending && !streamingMessage ? (
                  <div className="h-full flex flex-col items-center justify-center text-center p-6 text-muted-foreground">
                    <p className="text-sm font-medium">No conversation history yet.</p>
                    <p className="text-xs mt-1 max-w-55">Your interaction updates will stream down here synchronously.</p>
                  </div>
                ) : (
                  <>
                    {/* Render saved history array items */}
                    {savedMessages.map((msg, idx) => (
                      <div
                        key={idx}
                        className={`flex flex-col max-w-[85%] rounded-2xl p-3.5 text-sm transition-all shadow-sm ${msg.role === "assistant"
                          ? "bg-muted text-foreground border border-border self-start rounded-tl-none"
                          : "bg-primary text-primary-foreground self-end rounded-tr-none ml-auto"
                          }`}
                      >
                        <div className={`text-[10px] font-bold tracking-wider uppercase mb-1.5 opacity-60 ${msg.role === "assistant" ? "text-muted-foreground" : "text-primary-foreground"
                          }`}>
                          {msg.role}
                        </div>
                        <div className="prose prose-sm max-w-none leading-relaxed break-words dark:prose-invert">
                          <Markdown>{msg.text}</Markdown>
                        </div>
                      </div>
                    ))}

                    {/* RENDER IN-FLIGHT STREAM CHUNKS LIVE */}
                    {streamingMessage && (
                      <div className="flex flex-col max-w-[85%] rounded-2xl p-3.5 text-sm bg-muted text-foreground border border-border self-start rounded-tl-none transition-all shadow-sm">
                        <div className="text-[10px] font-bold tracking-wider uppercase mb-1.5 opacity-60 text-muted-foreground">
                          assistant (typing...)
                        </div>
                        <div className="prose prose-sm max-w-none leading-relaxed break-words dark:prose-invert">
                          <Markdown>{streamingMessage}</Markdown>
                        </div>
                      </div>
                    )}
                  </>
                )}
              </div>
            </div>

            {/* Splitter Line Separator */}
            <div className="w-[1px] bg-border relative z-10 flex-shrink-0" />
          </>
        )}

        {/* RIGHT PREVIEW PANEL */}
        <div
          id="project-page-pdf-panel"
          className="flex flex-col h-full bg-muted/20 overflow-hidden relative"
          style={{ flex: "65 1 0px" }}
        >
          {!showChat && (
            <div className="absolute top-4 left-4 z-50">
              <Button
                onClick={() => setShowChat(true)}
                variant="outline"
                className="h-8 text-xs font-medium shadow-sm bg-background border-border rounded-lg"
              >
                ✨ Open Assistant Sidebar
              </Button>
            </div>
          )}

          {/* Clean Render Sandbox Area */}
          <div className="flex-1 overflow-y-auto p-6 md:p-10 flex justify-center app-scrollbar">
            {data?.htmlDocument ? (
              <div
                className="w-full max-w-[816px] bg-white rounded-sm min-h-[1056px] h-fit overflow-hidden border border-border"
                style={{
                  boxShadow: `
                    rgba(0, 0, 0, 0.02) 0px 1px 3px 0px, 
                    rgba(0, 0, 0, 0.06) 0px 10px 15px -3px, 
                    rgba(0, 0, 0, 0.03) 0px 4px 6px -2px
                  `
                }}
              >
                <iframe
                  title="Document Preview"
                  srcDoc={isolatedHtmlSrcDoc}
                  className="w-full h-264 border-0 block"
                  sandbox="allow-same-origin"
                />
              </div>
            ) : (
              <div className="m-auto text-center max-w-sm">
                <div className="w-12 h-12 rounded-2xl bg-muted border border-border flex items-center justify-center mx-auto mb-4 text-xl">📄</div>
                <h3 className="text-sm font-medium">No preview document context generated</h3>
                <p className="text-xs text-muted-foreground mt-1">Submit instructions below to automatically compose and display your resume page markup grid.</p>
              </div>
            )}
          </div>

          {/* FLOATING ACTION INTERACTION PROMPT INTERFACE */}
          <div className="absolute bottom-6 left-1/2 -translate-x-1/2 w-full max-w-2xl px-4 z-50">
            <form
              onSubmit={sendPrompt}
              className="bg-background/95 backdrop-blur-md border border-border shadow-xl rounded-[24px] p-2 flex items-center gap-2 transition-all focus-within:ring-1 focus-within:ring-ring focus-within:border-ring"
            >
              <div className="flex-1 relative pl-2">
                <Textarea
                  value={prompt}
                  onChange={e => setPrompt(e.target.value)}
                  placeholder={showChat ? "Ask AI to change font, rewrite descriptions..." : "✨ What would you like to build or edit in your CV?"}
                  className="w-full bg-transparent border-0 ring-0 focus-visible:ring-0 focus-visible:ring-offset-0 text-sm text-foreground placeholder-muted-foreground resize-none py-2 min-h-[40px] max-h-[120px] app-scrollbar"
                  rows={1}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && !e.shiftKey) {
                      e.preventDefault();
                      sendPrompt(e);
                    }
                  }}
                />
              </div>
              <div className="flex items-center gap-1.5 self-end shrink-0">
                {!showChat && (
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={() => setShowChat(true)}
                    className="h-9 w-9 p-0 rounded-full text-muted-foreground hover:text-foreground"
                    title="View History"
                  >
                    💬
                  </Button>
                )}
                <Button
                  type="submit"
                  disabled={promptMutation.isPending || !prompt.trim()}
                  className="h-9 px-4 rounded-full font-medium transition-all text-xs"
                >
                  {promptMutation.isPending ? "Streaming..." : "Generate"}
                </Button>
              </div>
            </form>
          </div>

        </div>
      </div>
    </main>
  );
}

function useChatSessionById(id: string) {
  return useQuery({
    queryKey: ["session", id],
    queryFn: async () => (await api.chatSessionDetail(id)).data,
  });
}
