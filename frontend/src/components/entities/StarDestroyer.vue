<template>
    <div class="star-destroyer" @click="handleClick" :class="{ 'selected': draftStore.selectedEntity?.type === EntityType.StarDestroyer }">
        <span class="pts-cost">50 PTS</span>
        <span class="item-name">Star Destroyer</span>
        <img src="@/assets/images/sd.png" alt="Star Destroyer" width="300"/>
    </div>
</template>
<script setup lang="ts">
import { EntityType } from '@/types/entity';
import { useDraftStore } from '@/stores/draft.store';

const draftStore = useDraftStore();

const handleClick = () => {
    draftStore.selectEntity({ type: EntityType.StarDestroyer });
}
</script>
<style scoped lang="scss">
.star-destroyer {
    width: 100%;
    height: 100%;
    img {
        width: 100%;
        height: 100%;
    }
    cursor: pointer;
    position: relative;
    &.selected {
        background-color: var(--color-sector-hidden-hover);
    }
    .pts-cost {
        position: absolute;
        top: 0;
        left: 0;
        color: #ffffff;
        padding: 5px;
        border-radius: 5px;
    }
    .item-name {
        position: absolute;
        top: 0;
        right: 0;
        color: #ffffff;
        padding: 5px;
        border-radius: 5px;
    }
    &:hover {
        transform: scale(1.05);
        transition: transform 0.2s ease-in-out;
    }
}
</style>