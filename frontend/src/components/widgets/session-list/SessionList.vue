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

const sessions = ref<Session[]>([
    { id: '1', battleMap: { sectors: [] }, players: [], currentPlayer: { id: '1', name: 'Player 1' }, currentRound: 1 },
    { id: '2', battleMap: { sectors: [] }, players: [], currentPlayer: { id: '2', name: 'Player 2' }, currentRound: 1 },
]);
</script>
<style scoped lang="scss">
.game-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
}
</style>