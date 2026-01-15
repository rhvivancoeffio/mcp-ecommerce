import { useOpenAiGlobal } from "./useOpenAiGlobal";
import { type DisplayMode } from "./types";

/**
 * Hook para obtener el modo de visualización del widget (pip, inline, fullscreen)
 * Basado en el ejemplo oficial de OpenAI Apps SDK Examples
 */
export const useDisplayMode = (): DisplayMode | null => {
  return useOpenAiGlobal("displayMode");
};
