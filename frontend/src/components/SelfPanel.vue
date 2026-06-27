<script setup lang="ts">
import CharacterPanel from "@/components/CharacterPanel.vue";
import type { CharacterState, CurrencyInfo } from "@/types";

defineProps<{
  character: CharacterState | null | undefined;
  currencies: CurrencyInfo[] | null | undefined;
  territoryName?: string;
  worldName?: string;
  dataCenterName?: string;
}>();

function hasWeeklyProgress(currency: CurrencyInfo) {
  return typeof currency.weeklyAcquired === "number" && typeof currency.weeklyLimit === "number";
}

function formatNumber(value: number | null | undefined) {
  return (value ?? 0).toLocaleString();
}
</script>

<template>
  <div class="section-title">自身</div>
  <CharacterPanel
    :character="character"
    empty-text="未登录"
    :territory-name="territoryName"
    :world-name="worldName"
    :data-center-name="dataCenterName"
  />

  <div v-if="currencies && currencies.length > 0" class="currency-list">
    <div
      v-for="c in currencies"
      :key="c.itemId"
      class="currency-item"
    >
      <span class="currency-name">{{ c.name }}</span>
      <span class="currency-count">{{ formatNumber(c.count) }}</span>
      <span v-if="hasWeeklyProgress(c)" class="currency-weekly">
        本周 {{ formatNumber(c.weeklyAcquired) }}/{{ formatNumber(c.weeklyLimit) }}
      </span>
    </div>
  </div>
</template>
