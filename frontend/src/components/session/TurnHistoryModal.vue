<template>
    <Teleport to="body">
        <div
            v-if="open"
            class="turn-history-overlay"
            @click.self="emit('close')"
        >
            <div class="turn-history-modal">
                <button
                    type="button"
                    class="turn-history-close"
                    aria-label="Close turn history"
                    @click="emit('close')"
                >
                    ×
                </button>
                <h2 class="turn-history-title">Turn History</h2>
                <div v-if="loading" class="turn-history-state">Loading...</div>
                <div v-else-if="error" class="turn-history-state turn-history-state--error">{{ error }}</div>
                <div v-else-if="logs.length === 0" class="turn-history-state">No actions recorded yet.</div>
                <ul v-else class="turn-history-list">
                    <li
                        v-for="log in displayedLogs"
                        :key="log.id"
                        class="turn-history-item"
                    >
                        {{ log.message }}
                    </li>
                </ul>
            </div>
        </div>
    </Teleport>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import * as sessionApi from '@/services/sessionApi';
import type { SessionActionLog } from '@/types/turnHistory';
import { useSessionStore } from '@/stores/session.store';

const props = defineProps<{
    open: boolean;
    sessionId: string | null;
}>();

const emit = defineEmits<{
    (e: 'close'): void;
}>();

const sessionStore = useSessionStore();
const logs = ref<SessionActionLog[]>([]);
const displayedLogs = computed(() => [...logs.value].reverse());
const loading = ref(false);
const error = ref<string | null>(null);

const loadHistory = async () => {
    if (!props.sessionId) {
        logs.value = [];
        return;
    }

    loading.value = true;
    error.value = null;
    try {
        logs.value = await sessionApi.fetchSessionHistory(props.sessionId);
    } catch {
        error.value = 'Failed to load turn history';
        logs.value = [];
    } finally {
        loading.value = false;
    }
};

watch(
    () => props.open,
    (isOpen) => {
        if (isOpen) {
            void loadHistory();
        }
    },
);

watch(
    () => sessionStore.currentSession,
    () => {
        if (props.open) {
            void loadHistory();
        }
    },
    { deep: true },
);
</script>

<style scoped lang="scss">
.turn-history-overlay {
    position: fixed;
    inset: 0;
    z-index: 1000;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: var(--space-md);
}

.turn-history-modal {
    position: relative;
    width: min(720px, 100%);
    max-height: min(80vh, 720px);
    background-color: #111;
    border: 1px solid #333;
    border-radius: 12px;
    padding: var(--space-md);
    display: flex;
    flex-direction: column;
    gap: var(--space-sm);
    padding: var(--space-m);
}

.turn-history-close {
    position: absolute;
    top: 12px;
    right: 12px;
    width: 32px;
    height: 32px;
    border: none;
    border-radius: 50%;
    background: transparent;
    color: #fff;
    font-size: 28px;
    line-height: 1;
    cursor: pointer;

    &:hover {
        background-color: rgba(255, 255, 255, 0.1);
    }
}

.turn-history-title {
    margin: 0;
    padding-right: 40px;
    color: #fff;
    font-size: 22px;
}

.turn-history-state {
    color: #ccc;
    padding: var(--space-sm) 0;

    &--error {
        color: #f44336;
    }
}

.turn-history-list {
    margin: 0;
    padding: 0;
    list-style: none;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.turn-history-item {
    color: #fff;
    line-height: 1.4;
    padding: 8px 10px;
    background-color: rgba(255, 255, 255, 0.05);
    border-radius: 8px;
}
</style>
