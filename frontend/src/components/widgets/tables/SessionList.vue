<template>
    <div class="game-list">
        <div class="game-list-item" v-for="session in sessions" :key="session.id">
            <div class="game-list-item-title">
                {{ session.id }}
            </div>
        </div>
    </div>
</template>
<script setup lang="ts">
import type { Session } from '@/types/session';
import { ref } from 'vue';

const sessions = ref<Session[]>([]);
</script>
<style scoped lang="scss">
.game-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
}
</style>