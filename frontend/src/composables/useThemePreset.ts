import { computed, ref, watch } from "vue";
import { getJson, postJson } from "@/services/pocketApi";

export type ThemePresetId =
  | "phantom"
  | "spotlight"
  | "velvet"
  | "neon"
  | "afterlife"
  | "braindance";

export interface ThemePreset {
  id: ThemePresetId;
  name: string;
  hint: string;
  swatches: string[];
}

const storageKey = "pocket-station-theme";
const defaultTheme: ThemePresetId = "phantom";

export const themePresets: ThemePreset[] = [
  {
    id: "phantom",
    name: "怪盗红",
    hint: "猩红 / 黑曜 / 纸白",
    swatches: ["#e60012", "#111111", "#fff8ea"]
  },
  {
    id: "spotlight",
    name: "聚光黄",
    hint: "警戒黄 / 墨黑 / 赤红",
    swatches: ["#ffd400", "#101010", "#e4001b"]
  },
  {
    id: "velvet",
    name: "天鹅绒",
    hint: "深蓝 / 绯红 / 骨白",
    swatches: ["#171d3f", "#f0142f", "#fff3df"]
  },
  {
    id: "neon",
    name: "霓虹街",
    hint: "电光黄 / 赛博青 / 夜黑",
    swatches: ["#fcee09", "#00f5ff", "#111318"]
  },
  {
    id: "afterlife",
    name: "来生酒吧",
    hint: "酸绿 / 热粉 / 深紫",
    swatches: ["#b6ff00", "#ff2bd6", "#16001f"]
  },
  {
    id: "braindance",
    name: "超梦蓝",
    hint: "量子蓝 / 紫光 / 黑场",
    swatches: ["#00a3ff", "#9b5cff", "#050716"]
  }
];

function isThemePresetId(value: string | null): value is ThemePresetId {
  return themePresets.some((preset) => preset.id === value);
}

export function useThemePreset() {
  const initialTheme = isThemePresetId(localStorage.getItem(storageKey))
    ? localStorage.getItem(storageKey) as ThemePresetId
    : defaultTheme;

  const activeTheme = ref<ThemePresetId>(initialTheme);
  const activePreset = computed(() => themePresets.find((preset) => preset.id === activeTheme.value) ?? themePresets[0]);
  let loadedTheme = false;

  watch(
    activeTheme,
    (theme) => {
      document.documentElement.dataset.theme = theme;
      localStorage.setItem(storageKey, theme);
      if (loadedTheme) {
        void saveTheme(theme);
      }
    },
    { immediate: true }
  );

  void loadTheme();

  async function loadTheme() {
    try {
      const result = await getJson<{ theme?: string }>("/api/settings/theme");
      if (isThemePresetId(result.theme ?? null)) {
        activeTheme.value = result.theme as ThemePresetId;
      } else {
        activeTheme.value = defaultTheme;
      }
    } catch {
      // Local storage remains a fallback for dev server or older plugin builds.
    } finally {
      loadedTheme = true;
    }
  }

  async function saveTheme(theme: ThemePresetId) {
    try {
      await postJson("/api/settings/theme", { theme });
    } catch {
      // Keep the UI responsive even if the plugin config endpoint is unavailable.
    }
  }

  return {
    activePreset,
    activeTheme,
    themePresets
  };
}
