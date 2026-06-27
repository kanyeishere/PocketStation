<script setup lang="ts">
import { ref } from "vue";
import type { CommandShortcut } from "@/types";

const props = defineProps<{
  shortcuts: CommandShortcut[];
}>();

const emit = defineEmits<{
  send: [command: string];
  save: [shortcuts: CommandShortcut[]];
}>();

const editing = ref(false);
const editList = ref<CommandShortcut[]>([]);
const newLabel = ref("");
const newCommand = ref("");

function startEdit() {
  editList.value = props.shortcuts.map(s => ({ ...s }));
  editing.value = true;
}

function cancelEdit() {
  editing.value = false;
}

function saveEdit() {
  emit("save", [...editList.value]);
  editing.value = false;
}

function addShortcut() {
  const label = newLabel.value.trim();
  const command = newCommand.value.trim();
  if (!label || !command) return;

  editList.value.push({
    id: "custom-" + Date.now(),
    label,
    command
  });
  newLabel.value = "";
  newCommand.value = "";
}

function removeShortcut(index: number) {
  editList.value.splice(index, 1);
}

function moveShortcut(from: number, to: number) {
  const list = editList.value;
  if (
    !Number.isInteger(from)
    || !Number.isInteger(to)
    || from === to
    || from < 0
    || to < 0
    || from >= list.length
    || to >= list.length
  ) return;

  const [item] = list.splice(from, 1);
  list.splice(to, 0, item);
}

function moveUp(index: number) {
  moveShortcut(index, index - 1);
}

function moveDown(index: number) {
  moveShortcut(index, index + 1);
}

</script>

<template>
  <section id="tab-shortcuts" class="view">
    <div class="shortcuts-toolbar">
      <button v-if="!editing" type="button" @click="startEdit">编辑</button>
      <template v-else>
        <button type="button" @click="saveEdit">保存</button>
        <button type="button" @click="cancelEdit">取消</button>
      </template>
    </div>

    <div v-if="!editing" class="shortcuts-grid">
      <button
        v-for="s in shortcuts"
        :key="s.id"
        type="button"
        class="shortcut-chip"
        @click="emit('send', s.command)"
      >
        <span class="shortcut-label">{{ s.label }}</span>
        <span class="shortcut-cmd">{{ s.command }}</span>
      </button>
    </div>

    <div v-else class="shortcuts-editor">
      <div
        v-for="(s, i) in editList"
        :key="s.id"
        class="shortcut-edit-row"
      >
        <div class="move-controls">
          <button
            type="button"
            class="btn-move"
            :disabled="i === 0"
            title="上移"
            aria-label="上移"
            @click="moveUp(i)"
          >
            ↑
          </button>
          <button
            type="button"
            class="btn-move"
            :disabled="i === editList.length - 1"
            title="下移"
            aria-label="下移"
            @click="moveDown(i)"
          >
            ↓
          </button>
        </div>
        <input v-model="s.label" placeholder="名称" class="edit-label">
        <input v-model="s.command" placeholder="指令" class="edit-cmd">
        <button type="button" class="btn-remove" title="删除" aria-label="删除" @click="removeShortcut(i)">×</button>
      </div>

      <div class="shortcut-edit-row add-row">
        <div class="move-controls placeholder" aria-hidden="true"></div>
        <input v-model="newLabel" placeholder="新名称" class="edit-label">
        <input v-model="newCommand" placeholder="新指令" class="edit-cmd">
        <button type="button" class="btn-add" title="添加" aria-label="添加" @click="addShortcut">+</button>
      </div>
    </div>
  </section>
</template>

<style scoped>
.shortcuts-toolbar {
  display: flex;
  gap: 8px;
  padding: 8px 12px;
}

.shortcuts-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 12px;
}

.shortcut-chip {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 12px 16px;
  border: 1px solid var(--border-color, #555);
  border-radius: 8px;
  background: var(--bg-input, #1a1a1a);
  color: var(--text-color, #eee);
  cursor: pointer;
  transition: background 0.15s;
  min-width: 120px;
}

.shortcut-chip:hover {
  background: var(--border-color, #555);
}

.shortcut-label {
  font-size: 0.9rem;
  font-weight: 600;
}

.shortcut-cmd {
  font-size: 0.75rem;
  color: var(--text-dim, #888);
  font-family: monospace;
}

.shortcuts-editor {
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.shortcut-edit-row {
  display: grid;
  grid-template-columns: 34px minmax(96px, 120px) minmax(0, 1fr) 28px;
  gap: 6px;
  align-items: center;
}

.move-controls {
  display: grid;
  grid-template-rows: repeat(2, 18px);
  gap: 4px;
  width: 34px;
}

.edit-label {
  width: 100%;
  min-width: 0;
  min-height: 30px;
  padding: 4px 8px;
  border: 1px solid var(--border-color, #555);
  border-radius: 4px;
  background: var(--bg-input, #1a1a1a);
  color: var(--text-color, #eee);
  font-size: 0.875rem;
}

.edit-cmd {
  width: 100%;
  min-width: 0;
  min-height: 30px;
  padding: 4px 8px;
  border: 1px solid var(--border-color, #555);
  border-radius: 4px;
  background: var(--bg-input, #1a1a1a);
  color: var(--text-color, #eee);
  font-size: 0.875rem;
  font-family: monospace;
}

.btn-remove {
  width: 28px;
  min-height: 36px;
  padding: 4px 8px;
  border: none;
  border-radius: 4px;
  background: #622;
  color: #f99;
  cursor: pointer;
  font-size: 0.75rem;
}

.btn-move {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  min-height: 18px;
  padding: 0;
  border: 1px solid var(--border-color, #555);
  border-radius: 4px;
  background: transparent;
  color: var(--text-dim, #888);
  cursor: pointer;
  font-size: 0.7rem;
  line-height: 1;
}

.btn-move:disabled {
  opacity: 0.3;
  cursor: default;
}

.btn-move:not(:disabled):hover {
  background: var(--border-color, #555);
  color: var(--text-color, #eee);
}

.btn-add {
  width: 28px;
  min-height: 36px;
  padding: 4px 0;
  border: 1px dashed var(--border-color, #555);
  border-radius: 4px;
  background: transparent;
  color: var(--text-color, #eee);
  cursor: pointer;
  font-size: 0.875rem;
}

@media (max-width: 430px) {
  .shortcuts-editor {
    padding: 8px;
  }

  .shortcut-edit-row {
    grid-template-columns: 34px minmax(84px, 112px) minmax(0, 1fr) 28px;
  }
}
</style>
