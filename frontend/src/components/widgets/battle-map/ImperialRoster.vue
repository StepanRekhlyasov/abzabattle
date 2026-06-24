<template>
    <div class="roster">
        <div class="roster-item">
            <star-destroyer />
        </div>
        <div class="roster-item">
            <star-destroyer />
        </div>
        <div class="roster-item">
            <star-destroyer />
        </div>
        <div class="roster-item">
            <star-destroyer />
        </div>
        <div class="roster-item">
            <star-destroyer />
        </div>
        <div class="roster-item">
            <star-destroyer />
        </div>
        <div class="roster-item">
            <star-destroyer />
        </div>
        <div class="roster-item">
            <star-destroyer />
        </div>
    </div>
</template>
<script setup lang="ts">
import StarDestroyer from '@/components/entities/StarDestroyer.vue';
import type { Entity } from '@/types/entity';
import { ref } from 'vue';

const selectedEntity = ref<Entity | null>(null);

const handleSelectEntity = (entity: Entity) => {
    selectedEntity.value = entity;
}
</script>
<style scoped lang="scss">
.roster {
    padding: var(--space-m);
    gap: 10px;
    flex-direction: column;
    align-items: flex-start;    
    max-height: calc(100vh - 268px);
    overflow: auto;

}
</style>