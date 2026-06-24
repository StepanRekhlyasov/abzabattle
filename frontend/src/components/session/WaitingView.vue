<template>
    <div class="waiting-view">
        <div class="waiting-spinner" />
        <p class="waiting-text">{{ message }}</p>
    </div>
</template>

<script setup lang="ts">
defineProps<{
    message: string;
}>();
</script>

<style scoped lang="scss">
.waiting-view {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 24px;
    width: 100%;
    min-height: 300px;
    padding: var(--space-lg);
}

.waiting-spinner {
    width: 48px;
    height: 48px;
    border: 4px solid rgba(255, 255, 255, 0.2);
    border-top-color: #ffffff;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
}

.waiting-text {
    color: #ffffff;
    font-size: 20px;
    margin: 0;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}
</style>
