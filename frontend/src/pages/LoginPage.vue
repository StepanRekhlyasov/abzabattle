<template>
<blank-layout>
    <div class="wrapper" style="background-color: #000;">
        <div class="login-form">
            <input type="text" v-model="name" placeholder="Enter your name" class="generic-input" maxlength="10" @keyup.enter="handleSubmit"/>
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

const authStore = useAuthStore();
const name = ref<string>('');
const isGameOver = ref<boolean>(false);
const battleMapData = ref<BattleMapType>({
    sectors: [
        [
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Letter, content: 'L' }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
        ],
        [
            { entity: { id: '1', type: EntityType.Letter, content: 'O' }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Letter, content: 'G' }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
        ],
        [
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Letter, content: 'I' }, hidden: true, destroyed: false },
        ],
        [
            { entity: { id: '1', type: EntityType.Letter, content: 'N' }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
            { entity: { id: '1', type: EntityType.Empty }, hidden: true, destroyed: false },
        ],
    ]
});

const onSectorClick = (sector: BattleSectorType) => {
    if(sector.hidden === false && sector.entity.type === EntityType.Letter) {
        sector.destroyed = true;
    } else {
        sector.hidden = false;
    }
    checkGameOver();
    if(isGameOver.value) {
        handleSubmit();
        return;
    }
}

const handleSubmit = () => {
    if(name.value.length === 0) {
        toast.error('Name is required');
        return;
    }
    authStore.login(name.value);
}

const checkGameOver = () => {
    const letters = battleMapData.value.sectors.flat().filter(sector => sector.entity.type === EntityType.Letter && sector.hidden === true);
    if(letters.length === 0) {
        battleMapData.value = {
            sectors: [
                [
                    { entity: { id: '1', type: EntityType.Letter, content: 'L' }, hidden: false, destroyed: false },
                    { entity: { id: '1', type: EntityType.Letter, content: 'O' }, hidden: false, destroyed: false },
                    { entity: { id: '1', type: EntityType.Letter, content: 'G' }, hidden: false, destroyed: false },
                    { entity: { id: '1', type: EntityType.Letter, content: 'I' }, hidden: false, destroyed: false },
                    { entity: { id: '1', type: EntityType.Letter, content: 'N' }, hidden: false, destroyed: false },
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
        display: flex;
        flex-direction: column;
        gap: 10px;
    }
}
</style>
