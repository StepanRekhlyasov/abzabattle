<template>
    <div class="ability">
        <div class="ability-sector">
            <img :src="ability.image" :alt="ability.name" class="ability-image" />
        </div>
        <div class="ability-tooltip" role="tooltip">
            <span class="ability-tooltip__name">{{ ability.name }}</span>
            <span class="ability-tooltip__description">{{ ability.description }}</span>
        </div>
    </div>
</template>

<script setup lang="ts">
import type { UnitAbility } from '@/data/unitAbilities';

defineProps<{
    ability: UnitAbility;
}>();
</script>

<style scoped lang="scss">
.ability {
    position: relative;
    flex-shrink: 0;

    &:hover .ability-tooltip {
        opacity: 1;
        visibility: visible;
        transform: translateX(-50%) translateY(0);
    }
}

.ability-sector {
    width: 120px;
    height: 120px;
    background-color: var(--color-sector-exposed);
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    cursor: pointer;

    &:hover {
        background-color: var(--color-sector-exposed-hover);
    }
}

.ability-image {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

.ability-tooltip {
    position: absolute;
    left: 50%;
    bottom: calc(100% + 8px);
    transform: translateX(-50%) translateY(4px);
    min-width: 180px;
    max-width: 240px;
    padding: var(--space-sm);
    border-radius: 8px;
    background-color: rgba(0, 0, 0, 0.92);
    border: 1px solid rgba(255, 255, 255, 0.2);
    color: #ffffff;
    display: flex;
    flex-direction: column;
    gap: 6px;
    opacity: 0;
    visibility: hidden;
    transition: opacity 0.15s ease, transform 0.15s ease, visibility 0.15s ease;
    pointer-events: none;
    z-index: 10;
}

.ability-tooltip__name {
    font-size: 14px;
    font-weight: 600;
}

.ability-tooltip__description {
    font-size: 12px;
    line-height: 1.4;
    color: rgba(255, 255, 255, 0.85);
}
</style>
