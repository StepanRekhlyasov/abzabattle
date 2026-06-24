<template>
<blank-layout>
    <div class="wrapper" style="background-color: #000;">
        <div class="login-form">
            <div v-if="isLoading" class="login-form__loader">
                <v-progress-circular indeterminate color="primary" size="48" />
            </div>
            <input
                type="text"
                v-model="name"
                placeholder="Enter your name"
                class="generic-input"
                maxlength="10"
                :disabled="isLoading"
                @keyup.enter="handleSubmit"
            />
            <battle-map :battle-map-data="battleMapData" @sector-click="onSectorClick" />
        </div>
    </div>
</blank-layout>
</template>

<script setup lang="ts">
import BlankLayout from '@/components/layouts/BlankLayout.vue';
import BattleMap from '@/components/widgets/battle-map/BattleMap.vue';
import type { BattleMap as BattleMapType, BattleSector as BattleSectorType } from '@/types/map';
import { EntityType } from '@/types/entity';
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth.store';
import { toast } from 'vuetify-sonner';
import { useBattleMap } from '@/composables/useBattleMap';

const authStore = useAuthStore();
const name = ref<string>('');
const isLoading = ref<boolean>(false);
const isGameOver = ref<boolean>(false);
const { generateBattleMap, randomlyDeployEntities } = useBattleMap();
const battleMap = ref<BattleMapType>(generateBattleMap({ size: { x: 5, y: 5 } }));
randomlyDeployEntities([
  { type: EntityType.Letter, content: 'L' },
  { type: EntityType.Letter, content: 'O' },
  { type: EntityType.Letter, content: 'G' },
  { type: EntityType.Letter, content: 'I' },
  { type: EntityType.Letter, content: 'N' },
], battleMap.value)
const battleMapData = ref<BattleMapType>(battleMap.value);

const onSectorClick = ({ sector }: { sector: BattleSectorType; x: number; y: number }) => {
    if (isLoading.value) {
        return;
    }

    if(sector.hidden === false && sector.entity.content) {
        sector.destroyed = true;
    } else {
        sector.hidden = false;
    }
    if(isGameOver.value) {
        handleSubmit();
        return;
    }
    checkGameOver();
}

const handleSubmit = async () => {
    if (isLoading.value) {
        return;
    }

    if(name.value.length === 0) {
        toast.error('Name is required');
        return;
    }

    isLoading.value = true;
    try {
        await authStore.login(name.value);
    } finally {
        isLoading.value = false;
    }
}

const checkGameOver = () => {
    const letters = battleMapData.value.sectors.flat().filter(sector => sector.entity.type === EntityType.Letter && sector.hidden === true);
    if(letters.length === 0) {
        battleMapData.value = {
            sectors: [
                [
                    { entity: { type: EntityType.Letter, content: 'L' }, hidden: false, destroyed: false },
                    { entity: { type: EntityType.Letter, content: 'O' }, hidden: false, destroyed: false },
                    { entity: { type: EntityType.Letter, content: 'G' }, hidden: false, destroyed: false },
                    { entity: { type: EntityType.Letter, content: 'I' }, hidden: false, destroyed: false },
                    { entity: { type: EntityType.Letter, content: 'N' }, hidden: false, destroyed: false },
                ]
            ]
        }
        isGameOver.value = true;
    }
}
</script>

<style scoped lang="scss">
.wrapper {
    padding: var(--space-m);
    border-radius: 10px;
    .login-form {
        position: relative;
        display: flex;
        flex-direction: column;
        gap: 10px;

        &__loader {
            position: absolute;
            inset: 0;
            z-index: 1;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 10px;
            background: rgba(0, 0, 0, 0.6);
        }
    }
}
</style>
