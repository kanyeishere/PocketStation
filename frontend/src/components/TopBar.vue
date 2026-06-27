<script setup lang="ts">
import type { ThemePreset, ThemePresetId } from "@/composables/useThemePreset";
import type { ConnectionMode } from "@/types";

defineProps<{
  connectionText: string;
  connectionMode: ConnectionMode;
  theme: ThemePresetId;
  themePresets: ThemePreset[];
  activePreset: ThemePreset;
}>();

const emit = defineEmits<{
  refresh: [];
  "update:theme": [theme: ThemePresetId];
}>();
</script>

<template>
  <header class="topbar">
    <div class="brand-block">
      <div class="title">Pocket Station</div>
      <div class="connection" :class="connectionMode">{{ connectionText }}</div>
    </div>
    <div class="topbar-actions">
      <label class="theme-picker" :title="activePreset.hint">
        <span class="theme-picker-label">主题</span>
        <select
          class="theme-select"
          :value="theme"
          aria-label="切换主题"
          @change="emit('update:theme', ($event.target as HTMLSelectElement).value as ThemePresetId)"
        >
          <option v-for="preset in themePresets" :key="preset.id" :value="preset.id">
            {{ preset.name }}
          </option>
        </select>
        <span class="theme-swatches" aria-hidden="true">
          <span
            v-for="color in activePreset.swatches"
            :key="color"
            class="theme-swatch"
            :style="{ background: color }"
          />
        </span>
      </label>
      <button class="icon-button" type="button" title="刷新" @click="emit('refresh')">↻</button>
    </div>
  </header>
</template>
