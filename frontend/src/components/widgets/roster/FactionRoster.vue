<template>
    <div class="roster">
        <div v-for="unit in deploymentUnits" :key="unit.type" class="roster-item">
            <deplyment-unit :entity-type="unit.type" />
        </div>
    </div>
</template>
<script setup lang="ts">
import DeplymentUnit from '@/components/entities/DeplymentUnit.vue';
import { getDeploymentUnitsByFaction } from '@/data/deploymentUnits';
import type { Faction } from '@/types/session';
import { computed } from 'vue';

const props = defineProps<{ faction: Faction }>();
const deploymentUnits = computed(() => getDeploymentUnitsByFaction(props.faction));
</script>
<style scoped lang="scss">
.roster {
    padding: var(--space-m);
    gap: var(--space-m);
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    max-height: calc(100vh - 268px);
    overflow: auto;
    border-radius: 10px;
    .roster-item {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 100%;
        height: 100%;
    }
}
</style>
