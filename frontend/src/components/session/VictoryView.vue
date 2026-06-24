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
    position: absolute;
    inset: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(0, 0, 0, 0.65);
    z-index: 10;
    pointer-events: none;
}

.victory-text {
    color: #ffffff;
    font-size: 40px;
    font-weight: 700;
    margin: 0;
    text-align: center;
    text-shadow: 0 2px 12px rgba(0, 0, 0, 0.8);
}
</style>
