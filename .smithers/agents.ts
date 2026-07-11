// smithers-source: generated
import { type AgentLike } from "smithers-orchestrator";
import { ClaudeCodeAgent as SmithersClaudeCodeAgent } from "smithers-orchestrator";
// import { OpenAIAgent as SmithersOpenAIAgent } from "smithers-orchestrator";
// import { PiAgent as SmithersPiAgent } from "smithers-orchestrator";
// import { KimiAgent as SmithersKimiAgent } from "smithers-orchestrator";
// import { AmpAgent as SmithersAmpAgent } from "smithers-orchestrator";
// import { VibeAgent as SmithersVibeAgent } from "smithers-orchestrator";
// import { HermesCliAgent as SmithersHermesCliAgent } from "smithers-orchestrator";
// import { OpenClawAgent as SmithersOpenClawAgent } from "smithers-orchestrator";
import { ClaudeCodeAgent } from "./agents/claude-code";
// import { CodexAgent } from "./agents/codex";
// import { OpenCodeAgent } from "./agents/opencode";
// import { AntigravityAgent } from "./agents/antigravity";

export { ClaudeCodeAgent } from "./agents/claude-code";
// export { CodexAgent } from "./agents/codex";
// export { OpenCodeAgent } from "./agents/opencode";
// export { AntigravityAgent } from "./agents/antigravity";

// class SmithersOpenRouterAgent extends SmithersOpenAIAgent {
//   generate(args = {}) {
//     if (!process.env.OPENROUTER_API_KEY) {
//       throw new Error("Smithers generated an OpenRouter default agent, but OPENROUTER_API_KEY is not set. Set OPENROUTER_API_KEY, or run `smithers agent add` to configure another agent, then rerun this workflow.");
//     }
//     return super.generate(args);
//   }
// }
// 
// function createOpenRouterAgent() {
//   return new SmithersOpenRouterAgent({
//     model: "openai/gpt-4.1-mini",
//     baseURL: "https://openrouter.ai/api/v1",
//     apiKey: process.env.OPENROUTER_API_KEY,
//   });
// }

export const providers = {
  claude: ClaudeCodeAgent,
//   codex: CodexAgent,
//   openrouter: createOpenRouterAgent(),
//   opencode: OpenCodeAgent,
//   antigravity: AntigravityAgent,
//   pi: new SmithersPiAgent({ provider: "openai", model: "gpt-5.5" }),
//   kimi: new SmithersKimiAgent({ model: "kimi-k2.6" }),
//   amp: new SmithersAmpAgent(),
//   vibe: new SmithersVibeAgent({ agent: "auto-approve", cwd: process.cwd() }),
//   hermes: new SmithersHermesCliAgent({ cwd: process.cwd() }),
//   openclaw: new SmithersOpenClawAgent({ cwd: process.cwd() }),
  claudeOpus: new SmithersClaudeCodeAgent({ model: "claude-opus-4-8", cwd: process.cwd() }),
  claudeSonnet: new SmithersClaudeCodeAgent({ model: "claude-sonnet-5", cwd: process.cwd() }),
} as const;

export const agents = {
  cheapFast: [
    providers.claudeSonnet,
    // providers.kimi,
    // providers.vibe,
    // providers.antigravity,
    // providers.openclaw,
    // providers.pi,
  ],
  smart: [
    providers.claude,
    providers.claudeOpus,
    // providers.codex,
    // providers.opencode,
    // providers.openclaw,
    // providers.openrouter,
    // providers.kimi,
    // providers.antigravity,
    // providers.amp,
  ],
  smartTool: [
    providers.claude,
    providers.claudeOpus,
    // providers.codex,
    // providers.opencode,
    // providers.openclaw,
    // providers.openrouter,
    // providers.kimi,
    // providers.antigravity,
    // providers.amp,
  ],
  review: [
    providers.claude,
    providers.claudeOpus,
    providers.claudeSonnet,
    // providers.amp,
    // providers.codex,
    // providers.opencode,
    // providers.openclaw,
    // providers.openrouter,
  ],
  planning: [
    providers.claude,
    providers.claudeOpus,
    providers.claudeSonnet,
    // providers.codex,
    // providers.opencode,
    // providers.openclaw,
    // providers.openrouter,
  ],
  // implement: Smithers would normally suggest Codex here, but Codex is not available: missing `codex` on PATH.
  implement: [
    providers.claudeSonnet,
    providers.claude,
    // providers.codex,
    // providers.antigravity,
    // providers.opencode,
    // providers.openclaw,
    // providers.openrouter,
  ],
} as const satisfies Record<string, AgentLike[]>;
