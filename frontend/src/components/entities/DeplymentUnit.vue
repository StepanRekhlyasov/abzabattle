<template>
    <div class="deployment-unit" :class="{ selected: isSelected }" @click="handleClick">
        <span class="pts-cost">{{ definition.ptsCost }} PTS</span>
        <span class="item-name">{{ unitAsset.name }}</span>
        <div v-if="unitTraits.length" class="ability-list">
            <span
                v-for="trait in unitTraits"
                :key="`${trait.label}-${trait.name}`"
                class="ability-badge"
                :class="{ 'ability-badge--passive': trait.passive }"
            >
                <div class="ability-tooltip" role="tooltip">
                    <span class="ability-tooltip__name">{{ trait.name }}</span>
                    <span class="ability-tooltip__description">{{ trait.description }}</span>
                </div>
                {{ trait.label }}: {{ trait.name }}
            </span>
        </div>
        <button v-if="isSelected" type="button" class="rotate-button" @click="handleRotate">{{ draftStore.selectedRotation }}° - Rotate</button>
        <img :src="unitAsset.image" :alt="unitAsset.name" />
    </div>
</template>
<script setup lang="ts">
import { computed } from 'vue';
import type { Entity, EntityType } from '@/types/entity';
import { useDraftStore } from '@/stores/draft.store';
import { getEntityDefinition } from '@/data/entityDefinitions';
import { getDeploymentUnitAsset } from '@/data/deploymentUnits';
import { getUnitDraftTraits } from '@/data/unitAbilities';

const props = defineProps<{ entityType: EntityType }>();
const draftStore = useDraftStore();
const unitAsset = computed(() => getDeploymentUnitAsset(props.entityType));
const definition = computed(() => getEntityDefinition(props.entityType));
const isSelected = computed(() => draftStore.selectedEntity?.type === props.entityType);
const unitTraits = computed(() => getUnitDraftTraits(props.entityType));
const handleClick = () => {
    draftStore.selectEntity({
        type: props.entityType,
        rotation: draftStore.selectedRotation,
        content: definition.value.content,
    } as Entity);
};

const handleRotate = (event: MouseEvent) => {
    event.stopPropagation();
    draftStore.rotateSelectedEntity();
};
</script>
<style scoped lang="scss">
.deployment-unit {
    width: 90%;
    height: 100%;
    cursor: pointer;
    position: relative;
    border-radius: 10px;
    img { height: 100%; object-fit: contain; margin: auto; max-height: 350px; }
    &.selected { background-color: var(--color-sector-hidden-hover); }
    .pts-cost, .item-name {
        position: absolute;
        top: var(--space-sm);
        color: #ffffff;
        padding: 5px;
        border-radius: 5px;
        z-index: 1;
    }
    .pts-cost { left: var(--space-sm); }
    .item-name { right: var(--space-sm); }
    .ability-list {
        position: absolute;
        bottom: var(--space-sm);
        left: var(--space-sm);
        display: flex;
        flex-direction: column;
        gap: 4px;
        z-index: 1;
        max-width: calc(100% - 120px);
    }
    .ability-badge {
        position: relative;
        color: #ffffff;
        padding: 5px;
        border-radius: 5px;
        background-color: rgba(0, 0, 0, 0.45);
        font-size: 12px;
        line-height: 1.2;
        &--passive {
            background-color: var(--color-passive-sector);
            color: #1a1530;
        }
        &:hover .ability-tooltip {
            opacity: 1;
            visibility: visible;
            transform: translateX(-50%) translateY(0);
        }
    }
    .rotate-button {
        position: absolute;
        bottom: var(--space-sm);
        right: var(--space-sm);
        cursor: pointer;
        z-index: 1;
    }
    &:hover { transform: scale(1.05); transition: transform 0.2s ease-in-out; }
}
</style>
