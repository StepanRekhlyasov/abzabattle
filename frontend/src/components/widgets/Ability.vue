<template>
    <div
        class="ability"
        :class="{
            'ability--disabled': !passive && (disabled || used),
            'ability--selected': !passive && selected && !used,
            'ability--used': !passive && used,
            'ability--passive': passive,
        }"
        @click="handleClick"
    >
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
import type { PassiveAbility, UnitAbility } from '@/data/unitAbilities';

const props = defineProps<{
    ability: UnitAbility | PassiveAbility;
    selected?: boolean;
    used?: boolean;
    disabled?: boolean;
    passive?: boolean;
}>();

const emit = defineEmits<{
    (e: 'select', entityId: string): void;
}>();

const handleClick = () => {
    if (props.passive || props.disabled || props.used) return;
    if (!('entityId' in props.ability)) return;
    emit('select', props.ability.entityId);
};
</script>

<style scoped lang="scss">
.ability {
    position: relative;
    flex-shrink: 0;
    cursor: pointer;

    &:hover .ability-tooltip {
        opacity: 1;
        visibility: visible;
        transform: translateX(-50%) translateY(0);
    }

    &--disabled {
        cursor: not-allowed;
        opacity: 0.5;
    }

    &--used {
        cursor: not-allowed;
        opacity: 1;
    }

    &--passive {
        cursor: default;
    }
}

.ability-sector {
    width: 150px;
    height: 150px;
    background-color: var(--color-sector-exposed);
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    transition: background-color 0.15s ease;

    .ability--selected & {
        background-color: var(--color-sector-hidden-hover);
    }

    .ability--used & {
        background-color: var(--color-sector-destroyed);
    }

    .ability--passive & {
        background-color: var(--color-passive-sector);
    }
}

.ability-image {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

</style>
