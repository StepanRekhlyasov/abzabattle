<template>
    <div @click="onClickHandler" class="battle-sector">
        <div v-if="sectorData.hidden" class="battle-sector-hidden">
        </div>
        <div v-else-if="!sectorData.destroyed">
            {{ sectorData.entity.content }}
        </div>
        <div v-else class="battle-sector-destroyed">
            {{ sectorData.entity.content }}
        </div>
    </div>
</template>
<script setup lang="ts">
import type { BattleSector } from '@/types/map';

const props = defineProps<{
    sectorData: BattleSector;
}>();

const emit = defineEmits<{
    (e: 'sector-click', sector: BattleSector): void;
}>();

const onClickHandler = () => {
    emit('sector-click', props.sectorData);
}

</script>   
<style scoped lang="scss">
.battle-sector {
    width: 50px;
    height: 50px;
    background-color: var(--color-sector-exposed);
    border-radius: 10px;
    display: flex;
    justify-content: center;
    align-items: center;
    cursor: pointer;
    overflow: hidden;
    user-select: none;
    &:hover {
        background-color: var(--color-sector-exposed-hover);
    }
    .battle-sector-hidden {
        width: 100%;
        height: 100%;
        background-color: var(--color-sector-hidden);
        border-radius: 10px;
        &:hover {
            background-color: var(--color-sector-hidden-hover);
        }
    }
    .battle-sector-destroyed {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        background-color: var(--color-sector-destroyed);
    }
}
</style>