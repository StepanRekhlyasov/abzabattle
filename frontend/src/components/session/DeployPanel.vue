<template>
    <div class="deploy-panel">
        <div v-if="!readOnlySettings" class="wrapper settings-wrapper">
            <label class="item">
                <span>Session name:</span>
                <input v-model="sessionName" type="text" placeholder="Session name" class="generic-input">
            </label>
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
                <input type="number" v-model.number="ptsLimit" placeholder="PTS Limit" class="generic-input">
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
        <div v-else-if="readOnlySummary" class="wrapper settings-wrapper settings-wrapper--readonly">
            <div class="item"><span>Battle map size:</span> {{ readOnlySummary.mapSize }}x{{ readOnlySummary.mapSize }}</div>
            <div class="item"><span>PTS limit:</span> {{ readOnlySummary.ptsLimit }}</div>
            <div class="item"><span>Your faction:</span> {{ readOnlySummary.faction }}</div>
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
                    <button v-if="showResetButton" @click="resetBattleMap" class="generic-button">Reset Draft</button>
                    <button
                        @click="handleAction"
                        class="generic-button"
                        :disabled="isPtsOverLimit || isActionDisabled"
                    >
                        {{ actionLabel }}
                    </button>
                </div>
            </div>
        </div>
        <div v-else class="wrapper map-wrapper">
            <button @click="resetBattleMap" class="generic-button">Generate Battle Map</button>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import BattleMap from '@/components/widgets/battle-map/BattleMap.vue';
import DeployRoster from '@/components/widgets/roster/DeployRoster.vue';
import { useBattleMap } from '@/composables/useBattleMap';
import { getEntityDefinition } from '@/data/entityDefinitions';
import { useDraftStore } from '@/stores/draft.store';
import { EntityRotation, EntityType, type Entity } from '@/types/entity';
import type { Faction } from '@/types/session';
import { storeToRefs } from 'pinia';
import { useUserStore } from '@/stores/user.store';
import { isRotateKey } from '@/utils/rotateKey';

const props = withDefaults(defineProps<{
    readOnlySettings?: boolean;
    lockedFaction?: Faction | null;
    fixedMapSize?: number | null;
    fixedPtsLimit?: number | null;
    actionLabel?: string;
    actionDisabled?: boolean;
    showResetButton?: boolean;
    autoGenerate?: boolean;
}>(), {
    readOnlySettings: false,
    lockedFaction: null,
    fixedMapSize: null,
    fixedPtsLimit: null,
    actionLabel: 'Create Session',
    actionDisabled: false,
    showResetButton: true,
    autoGenerate: true,
});

const emit = defineEmits<{
    (e: 'action', payload?: { sessionName: string }): void;
}>();

const draftStore = useDraftStore();
const { battleMap, selectedFaction, ptsLimit, selectedEntity, selectedRotation, ptsRemaining, isPtsOverLimit } = storeToRefs(draftStore);

const { generateBattleMap, placeEntity, getPlacementPreview } = useBattleMap();
const userStore = useUserStore();
const { currentUser } = storeToRefs(userStore);
const battleMapSize = ref(String(props.fixedMapSize ?? 12));
const sessionName = ref(currentUser.value?.name + '\'s session');
const hoverAnchor = ref<{ x: number; y: number } | null>(null);

const isActionDisabled = computed(() =>
    props.actionDisabled || (!props.readOnlySettings && !sessionName.value.trim()),
);

const handleAction = () => {
    if (props.readOnlySettings) {
        emit('action');
        return;
    }
    emit('action', { sessionName: sessionName.value.trim() });
};

const readOnlySummary = computed(() => {
    if (!props.readOnlySettings) return null;
    return {
        mapSize: props.fixedMapSize ?? parseInt(battleMapSize.value, 10),
        ptsLimit: props.fixedPtsLimit ?? ptsLimit.value,
        faction: selectedFaction.value,
    };
});

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
    const size = props.fixedMapSize ?? parseInt(battleMapSize.value, 10);
    battleMap.value = generateBattleMap({ size: { x: size, y: size } });
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

const isEditableTarget = (target: EventTarget | null) => {
    if (!(target instanceof HTMLElement)) return false;
    const tag = target.tagName;
    return tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || target.isContentEditable;
};

const handleKeyDown = (event: KeyboardEvent) => {
    if (!isRotateKey(event.key) || isEditableTarget(event.target)) return;
    if (!buildSelectedEntity()) return;
    event.preventDefault();
    draftStore.rotateSelectedEntity();
};

const applySettings = () => {
    if (props.fixedPtsLimit != null) {
        ptsLimit.value = props.fixedPtsLimit;
    }
    if (props.lockedFaction) {
        selectedFaction.value = props.lockedFaction;
    }
    if (props.fixedMapSize != null) {
        battleMapSize.value = String(props.fixedMapSize);
    }
};

onMounted(() => {
    draftStore.setDrafting(true);
    window.addEventListener('keydown', handleKeyDown);
    applySettings();
    if (props.autoGenerate || props.readOnlySettings) {
        resetBattleMap();
    }
});

onUnmounted(() => {
    draftStore.setDrafting(false);
    window.removeEventListener('keydown', handleKeyDown);
});

watch(() => props.lockedFaction, (faction) => {
    if (faction) {
        selectedFaction.value = faction;
    }
});

watch(selectedFaction, () => {
    if (!props.readOnlySettings) {
        resetBattleMap();
        selectedEntity.value = null;
    }
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
.settings-wrapper--readonly .item {
    color: #ffffff;
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
