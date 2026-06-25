import type { BattleMap } from '@/types/map';
import { Faction, type Session } from '@/types/session';
import type { Player } from '@/types/player';
import { defineStore } from 'pinia';
import * as sessionApi from '@/services/sessionApi';
import type { CreateSessionPayload } from '@/services/sessionApi';
import { useUserStore } from '@/stores/user.store';
import { useAppStore } from '@/stores/app.store';

function getPlayerFaction(session: Session, playerName: string): Faction | null {
    if (session.rebel.player?.name === playerName) return Faction.Rebel;
    if (session.imperial.player?.name === playerName) return Faction.Imperial;
    return null;
}

function isSessionFull(session: Session) {
    return !!session.rebel.player && !!session.imperial.player;
}

function syncCurrentUserStats(session: Session) {
    const userStore = useUserStore();
    const currentUser = userStore.currentUser;
    if (!currentUser || session.status !== 'finished') return;

    const player = getPlayerInSession(session, currentUser.name);
    if (!player) return;

    userStore.setUser({
        ...currentUser,
        wins: player.wins,
        loses: player.loses,
        totalGames: player.totalGames,
        status: currentUser.status,
    });
}

function getPlayerInSession(session: Session, playerName: string): Player | null {
    if (session.rebel.player?.name === playerName) return session.rebel.player;
    if (session.imperial.player?.name === playerName) return session.imperial.player;
    return null;
}

function getAbilityTurnKey(session: Session) {
    return session.currentTurn;
}

export const useSessionStore = defineStore('session', {
    state: () => ({
        onlineSessions: [] as Session[],
        currentSession: null as Session | null,
        usedAbilityIdsBySession: {} as Record<string, string[]>,
        abilityTurnKeyBySession: {} as Record<string, string>,
    }),
    getters: {
        myFaction: (state) => (playerName: string) => {
            if (!state.currentSession) return null;
            return getPlayerFaction(state.currentSession, playerName);
        },
        isCreator: (state) => (playerName: string) =>
            state.currentSession?.creatorPlayerName === playerName,
        isJoiner: (state) => (playerName: string) =>
            !!state.currentSession &&
            state.currentSession.creatorPlayerName !== playerName &&
            !!getPlayerFaction(state.currentSession, playerName),
        isMyTurn: (state) => (playerName: string) => {
            if (!state.currentSession) return false;
            const faction = getPlayerFaction(state.currentSession, playerName);
            return faction !== null && state.currentSession.currentTurn === faction;
        },
        usedAbilityIdsForSession: (state) => (sessionId: string) =>
            state.usedAbilityIdsBySession[sessionId] ?? [],
    },
    actions: {
        setOnlineSessions(sessions: Session[]) {
            this.onlineSessions = sessions;
        },
        setCurrentSession(session: Session | null) {
            this.currentSession = session;
        },
        syncAbilityTurnState(session: Session) {
            const turnKey = getAbilityTurnKey(session);
            if (this.abilityTurnKeyBySession[session.id] !== turnKey) {
                this.abilityTurnKeyBySession = {
                    ...this.abilityTurnKeyBySession,
                    [session.id]: turnKey,
                };
                this.usedAbilityIdsBySession = {
                    ...this.usedAbilityIdsBySession,
                    [session.id]: [],
                };
                return;
            }

            if (!this.usedAbilityIdsBySession[session.id]) {
                this.usedAbilityIdsBySession = {
                    ...this.usedAbilityIdsBySession,
                    [session.id]: [],
                };
            }
        },
        isAbilityUsed(sessionId: string, entityId: string) {
            return this.usedAbilityIdsBySession[sessionId]?.includes(entityId) ?? false;
        },
        markAbilityUsed(sessionId: string, entityId: string) {
            const usedIds = this.usedAbilityIdsBySession[sessionId] ?? [];
            if (usedIds.includes(entityId)) {
                return;
            }

            this.usedAbilityIdsBySession = {
                ...this.usedAbilityIdsBySession,
                [sessionId]: [...usedIds, entityId],
            };
        },
        applySessionUpdate(session: Session) {
            const index = this.onlineSessions.findIndex(item => item.id === session.id);
            if (index === -1) {
                this.onlineSessions.unshift(session);
            } else {
                this.onlineSessions[index] = session;
            }
            if (this.currentSession?.id === session.id) {
                this.syncAbilityTurnState(session);
                this.currentSession = session;
            }
        },
        removeSession(sessionId: string) {
            this.onlineSessions = this.onlineSessions.filter(session => session.id !== sessionId);
            if (this.currentSession?.id === sessionId) {
                this.currentSession = null;
            }

            const { [sessionId]: _usedIds, ...remainingUsedIds } = this.usedAbilityIdsBySession;
            this.usedAbilityIdsBySession = remainingUsedIds;

            const { [sessionId]: _turnKey, ...remainingTurnKeys } = this.abilityTurnKeyBySession;
            this.abilityTurnKeyBySession = remainingTurnKeys;
        },
        commitSession(session: Session) {
            this.syncAbilityTurnState(session);
            this.currentSession = session;
            this.applySessionUpdate(session);
            syncCurrentUserStats(session);
            if (session.status === 'finished') {
                void useUserStore().getUsers();
            }
        },
        async loadSession(id: string, playerName?: string) {
            const session = await sessionApi.fetchSession(id, playerName);
            this.commitSession(session);
            return session;
        },
        async createSession(payload: CreateSessionPayload) {
            const session = await sessionApi.createSession(payload);
            this.commitSession(session);
            return session;
        },
        async joinSession(id: string, playerName: string) {
            const session = await sessionApi.joinSession(id, playerName);
            this.commitSession(session);
            return session;
        },
        async startBattle(id: string, playerName: string, battleMap: BattleMap) {
            const session = await sessionApi.startBattle(id, { playerName, battleMap });
            this.commitSession(session);
            return session;
        },
        async attackSector(id: string, playerName: string, x: number, y: number) {
            const appStore = useAppStore();
            appStore.setLoading(true);
            try {
                const session = await sessionApi.attackSector(id, { playerName, x, y });
                this.commitSession(session);
                return session;
            } finally {
                appStore.setLoading(false);
            }
        },
        async useAbility(
            id: string,
            playerName: string,
            abilityKind: string,
            x: number,
            y: number,
            sourceEntityId?: string,
        ) {
            const appStore = useAppStore();
            appStore.setLoading(true);
            try {
                const session = await sessionApi.useAbility(id, {
                    playerName,
                    abilityKind,
                    x,
                    y,
                    sourceEntityId,
                });
                this.commitSession(session);
                return session;
            } finally {
                appStore.setLoading(false);
            }
        },
        async giveUp(id: string, playerName: string) {
            const appStore = useAppStore();
            appStore.setLoading(true);
            try {
                const session = await sessionApi.giveUp(id, playerName);
                this.commitSession(session);
                return session;
            } finally {
                appStore.setLoading(false);
            }
        },
        async deleteSession(id: string, adminName: string) {
            await sessionApi.deleteSession(id, adminName);
            this.removeSession(id);
        },
        isWaitingForOpponent(playerName: string) {
            if (!this.currentSession) return false;
            return this.currentSession.status === 'pending' &&
                this.isCreator(playerName) &&
                !isSessionFull(this.currentSession);
        },
        isWaitingForDeploy(playerName: string) {
            if (!this.currentSession) return false;
            return this.currentSession.status === 'pending' &&
                this.isCreator(playerName) &&
                isSessionFull(this.currentSession);
        },
        isDeployPhase(playerName: string) {
            if (!this.currentSession) return false;
            return this.currentSession.status === 'pending' &&
                this.isJoiner(playerName) &&
                isSessionFull(this.currentSession);
        },
        isBattlePhase() {
            return this.currentSession?.status === 'in_progress';
        },
        isFinishedPhase() {
            return this.currentSession?.status === 'finished';
        },
    },
    persist: {
        pick: ['usedAbilityIdsBySession', 'abilityTurnKeyBySession'],
    },
});
