<template>
    <div
        class="ability"
        :class="{
            'ability--disabled': disabled || used,
            'ability--selected': selected && !used,
            'ability--used': used,
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
import type { UnitAbility } from '@/data/unitAbilities';

const props = defineProps<{
    ability: UnitAbility;
    selected?: boolean;
    used?: boolean;
    disabled?: boolean;
}>();

const emit = defineEmits<{
    (e: 'select', entityId: string): void;
}>();

const handleClick = () => {
    if (props.disabled || props.used) return;
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
}

.ability-image {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

</style>
