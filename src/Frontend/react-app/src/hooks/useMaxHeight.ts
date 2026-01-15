import { useOpenAiGlobal } from "./useOpenAiGlobal";

/**
 * Hook para obtener la altura máxima disponible del widget
 * Basado en el ejemplo oficial de OpenAI Apps SDK Examples
 */
export const useMaxHeight = (): number | null => {
  return useOpenAiGlobal("maxHeight");
};
