import { useOpenAiGlobal } from "./useOpenAiGlobal";
import type { UnknownObject } from "./types";

/**
 * Hook para obtener las props del widget desde window.openai.toolOutput.structuredContent
 * Basado en el ejemplo oficial de OpenAI Apps SDK Examples
 */
export function useWidgetProps<T extends UnknownObject>(
  defaultState?: T | (() => T)
): T | null {
  const toolOutput = useOpenAiGlobal("toolOutput");
  
  // Si toolOutput tiene structuredContent, extraerlo
  if (toolOutput && typeof toolOutput === 'object' && 'structuredContent' in toolOutput) {
    const structuredContent = (toolOutput as { structuredContent?: T }).structuredContent;
    if (structuredContent) {
      return structuredContent;
    }
  }
  
  // Si toolOutput es directamente el tipo T, retornarlo
  const props = toolOutput as T | null;

  const fallback =
    typeof defaultState === "function"
      ? (defaultState as () => T | null)()
      : defaultState ?? null;

  return props ?? fallback;
}
