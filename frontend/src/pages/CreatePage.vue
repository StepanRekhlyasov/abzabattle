<template>
    <main-layout>
        <div class="wrapper">
            <label class="item">
                <span>Battle map size:</span>
                <select v-model="battleMapSize" class="generic-input" style="width: 100px;">
                    <option value="8">8x8</option>
                    <option value="12">12x12</option>
                    <option value="16">16x16</option>
                    <option value="20">20x20</option>
                    <option value="24">24x24</option>
                    <option value="28">28x28</option>
                    <option value="32">32x32</option>
                    <option value="36">36x36</option>
                    <option value="40">40x40</option>
                </select>
            </label>
            <label class="item">
                <span>PTS limit:</span>
                <input type="number" v-model="ptsLimit" placeholder="PTS Limit" class="generic-input">
            </label>
            <label class="item">
                <span>Pick faction:</span>
                <select v-model="selectedFaction" class="generic-input">
                    <option value="imperial">Imperial</option>
                    <option value="rebel">Rebel</option>
                </select>
            </label>
            <button @click="resetBattleMap" class="generic-button">Generate Battle Map</button>
        </div>
        <div class="wrapper map-wrapper" v-if="battleMap">
            <battle-map
                :battle-map-data="battleMap"
                :preview-cells="previewCellKeys"
                :preview-valid="previewIsValid"
                @sector-click="handleSectorClick"
                @sector-hover="handleSectorHover"
                @sector-leave="hoverAnchor = null"
            />
            <div class="roster-wrapper">
                <deploy-roster />
                <div class="buttons-wrapper">
                    <button @click="resetBattleMap" class="generic-button">Reset Draft</button>
                    <button @click="handleCreateSession" class="generic-button" :disabled="isPtsOverLimit">Create Session</button>
                </div>
            </div>
        </div>
    </main-layout>
</template>
<script setup lang="ts">
import MainLayout from '@/components/layouts/MainLayout.vue';
import { computed, ref, watch } from 'vue';
import { useBattleMap } from '@/composables/useBattleMap';
import BattleMap from '@/components/widgets/battle-map/BattleMap.vue';
import DeployRoster from '@/components/widgets/roster/DeployRoster.vue';
import { useDraftStore } from '@/stores/draft.store';
import { storeToRefs } from 'pinia';
import { getEntityDefinition } from '@/data/entityDefinitions';
import { EntityRotation, EntityType, type Entity } from '@/types/entity';

const draftStore = useDraftStore();
const { battleMap, selectedFaction, ptsLimit, selectedEntity, selectedRotation, ptsRemaining, isPtsOverLimit } = storeToRefs(draftStore);
const { generateBattleMap, placeEntity, getPlacementPreview } = useBattleMap();

const battleMapSize = ref('12');
const hoverAnchor = ref<{ x: number; y: number } | null>(null);

const buildSelectedEntity = (withId = false): Entity | null => {
    if (!selectedEntity.value || selectedEntity.value.type === EntityType.Empty) return null;
    return {
        ...selectedEntity.value,
        ...(withId ? { id: crypto.randomUUID() } : {}),
        rotation: selectedEntity.value.rotation ?? selectedRotation.value ?? EntityRotation.R0,
    };
};

const selectedEntityCost = computed(() => {
    const entity = buildSelectedEntity();
    return entity ? getEntityDefinition(entity.type).ptsCost : 0;
});

const placementPreview = computed(() => {
    const entity = buildSelectedEntity();
    if (!battleMap.value || !entity || !hoverAnchor.value) return null;
    return getPlacementPreview(entity, hoverAnchor.value, battleMap.value);
});

const previewCellKeys = computed(() => placementPreview.value?.cells.map(cell => `${cell.x},${cell.y}`) ?? []);
const previewIsValid = computed(() => !!placementPreview.value?.isValid && ptsRemaining.value >= selectedEntityCost.value);

const resetBattleMap = () => {
    battleMap.value = generateBattleMap({ size: { x: parseInt(battleMapSize.value), y: parseInt(battleMapSize.value) } });
    draftStore.setPtsSpent(0);
    hoverAnchor.value = null;
};

const handleSectorHover = ({ x, y }: { x: number; y: number }) => {
    hoverAnchor.value = buildSelectedEntity() ? { x, y } : null;
};

const handleSectorClick = ({ x, y }: { x: number; y: number }) => {
    const entity = buildSelectedEntity(true);
    if (!battleMap.value || !entity) return;
    const definition = getEntityDefinition(entity.type);
    if (ptsRemaining.value < definition.ptsCost || !placeEntity(entity, { x, y }, battleMap.value)) return;
    draftStore.addPtsSpent(definition.ptsCost);
    hoverAnchor.value = null;
};

const handleCreateSession = () => {
    console.log('createSession');
};

watch(selectedFaction, () => {
    resetBattleMap();
    selectedEntity.value = null;
});
</script>
<style scoped lang="scss">
.wrapper {
    display: flex;
    align-items: flex-start;
    justify-content: flex-start;
    gap: 10px;
    width: 100%;
    background-color: var(--color-settings-background);
    border-radius: 10px;
    padding: var(--space-sm);
    .item span {
        color: #ffffff;
        margin-right: var(--space-m);
    }
}
.map-wrapper {
    margin-top: var(--space-sm);
}
.roster-wrapper {
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    gap: 10px;
    width: 100%;
    height: 100%;
    .generic-button:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }
}
.buttons-wrapper {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
}
</style>
