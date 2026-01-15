export type UnknownObject = Record<string, unknown>;

export type Theme = "light" | "dark";

export type DisplayMode = "pip" | "inline" | "fullscreen";

export type OpenAiGlobals<
  ToolInput = UnknownObject,
  ToolOutput = UnknownObject,
  ToolResponseMetadata = UnknownObject,
  WidgetState = UnknownObject
> = {
  // visuals
  theme: Theme;
  locale: string;

  // layout
  maxHeight: number;
  displayMode: DisplayMode;

  // state
  toolInput: ToolInput;
  toolOutput: ToolOutput | null;
  toolResponseMetadata: ToolResponseMetadata | null;
  widgetState: WidgetState | null;
  setWidgetState: (state: WidgetState) => Promise<void>;
};

export const SET_GLOBALS_EVENT_TYPE = "openai:set_globals";

export class SetGlobalsEvent extends CustomEvent<{
  globals: Partial<OpenAiGlobals>;
}> {
  readonly type = SET_GLOBALS_EVENT_TYPE;
}

declare global {
  interface Window {
    openai?: OpenAiGlobals & {
      setWidgetState?: (state: UnknownObject) => Promise<void>;
    };
  }

  interface WindowEventMap {
    [SET_GLOBALS_EVENT_TYPE]: SetGlobalsEvent;
  }
}
