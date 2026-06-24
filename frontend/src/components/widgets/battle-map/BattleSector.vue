<template>
    <div 
        class="battle-sector" 
        @click="emit('sector-click', sectorData)"
        @mouseenter="emit('sector-hover', sectorData)" 
        :class="{'empty': !sectorData.entity.content}"
    >
        <div v-if="sectorData.hidden" class="battle-sector-hidden" />
        <div v-else-if="!sectorData.destroyed" class="battle-sector-content" :class="{'with-contend' : sectorData.entity.content}">{{ sectorData.entity.content }}</div>
        <div v-else class="battle-sector-destroyed">{{ sectorData.entity.content }}</div>
        <div
            v-if="isShieldVisible(sectorData)"
            class="battle-sector-shield"
        />
        <div v-if="previewState" class="battle-sector-preview" :class="`battle-sector-preview--${previewState}`" />
    </div>
</template>
<script setup lang="ts">
import type { BattleSector } from '@/types/map';
import { isShieldVisible } from '@/utils/battleSectorRules';

defineProps<{
    sectorData: BattleSector;
    denseMode: boolean;
    previewState?: 'valid' | 'invalid' | null;
}>();

const emit = defineEmits<{
    (e: 'sector-click', sector: BattleSector): void;
    (e: 'sector-hover', sector: BattleSector): void;
}>();
</script>
<style scoped lang="scss">
.battle-sector {
    position: relative;
    width: v-bind('denseMode ? "30px" : "50px"');
    height: v-bind('denseMode ? "30px" : "50px"');
    background-color: var(--color-sector-exposed);
    border-radius: v-bind('denseMode ? "5px" : "10px"');
    display: flex;
    justify-content: center;
    align-items: center;
    cursor: pointer;
    overflow: hidden;
    user-select: none;
    &:hover { background-color: var(--color-sector-exposed-hover); }
    .battle-sector-hidden {
        width: 100%;
        height: 100%;
        background-color: var(--color-sector-hidden);
        border-radius: v-bind('denseMode ? "5px" : "10px"');
        &:hover { background-color: var(--color-sector-hidden-hover); }
    }
    .battle-sector-content.with-contend {
        background-color: var(--color-sector-exposed-content);
    }
    .battle-sector-content,
    .battle-sector-destroyed {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
    }
    .battle-sector-destroyed { background-color: var(--color-sector-destroyed); }
    .battle-sector-shield {
        position: absolute;
        inset: 0;
        background-color: var(--color-sector-shield);
        opacity: 0.75;
        border-radius: v-bind('denseMode ? "5px" : "10px"');
        pointer-events: none;
        z-index: 1;
    }
    .battle-sector-preview {
        position: absolute;
        inset: 0;
        border-radius: v-bind('denseMode ? "5px" : "10px"');
        pointer-events: none;
        z-index: 2;
        &--valid { background-color: var(--color-sector-hidden-hover); }
        &--invalid { background-color: var(--color-sector-destroyed); }
    }
    &.empty {
        .battle-sector-destroyed { background-color: var(--color-sector-empty-destroyed); }
    }
}
</style>
