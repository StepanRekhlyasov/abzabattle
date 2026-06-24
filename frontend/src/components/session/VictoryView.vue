<template>
    <div class="victory-view">
        <p class="victory-text">{{ winnerName }} wins!</p>
    </div>
</template>

<script setup lang="ts">
defineProps<{
    winnerName: string;
}>();
</script>

<style scoped lang="scss">
.victory-view {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    min-height: 100%;
    padding: var(--space-lg);
    background: #000;
}

.victory-text {
    color: #ffffff;
    font-size: 32px;
    font-weight: 700;
    margin: 0;
    text-align: center;
}
</style>
