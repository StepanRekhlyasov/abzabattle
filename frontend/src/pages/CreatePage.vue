<template>
    <main-layout>
        <deploy-panel
            action-label="Create Session"
            :action-disabled="!battleMap"
            @action="handleCreateSession"
        />
    </main-layout>
</template>
<script setup lang="ts">
import MainLayout from '@/components/layouts/MainLayout.vue';
import DeployPanel from '@/components/session/DeployPanel.vue';
import router from '@/router';
import { useDraftStore } from '@/stores/draft.store';
import { useSessionStore } from '@/stores/session.store';
import { useUserStore } from '@/stores/user.store';
import { useBattleMap } from '@/composables/useBattleMap';
import { storeToRefs } from 'pinia';

const sessionStore = useSessionStore();
const userStore = useUserStore();
const draftStore = useDraftStore();
const { prepareBattleMapForBattle } = useBattleMap();
const { battleMap, selectedFaction, ptsLimit } = storeToRefs(draftStore);

const handleCreateSession = async (payload?: { sessionName: string }) => {
    if (!battleMap.value || !userStore.currentUser || !selectedFaction.value || !payload?.sessionName) return;

    const mapSize = battleMap.value.options?.size.x ?? parseInt(String(battleMap.value.sectors[0]?.length ?? 12), 10);

    const session = await sessionStore.createSession({
        name: payload.sessionName,
        faction: selectedFaction.value,
        ptsLimit: ptsLimit.value,
        mapSize,
        playerName: userStore.currentUser.name,
        battleMap: prepareBattleMapForBattle(battleMap.value),
    });

    router.push(`/session/${session.id}`);
};
</script>
