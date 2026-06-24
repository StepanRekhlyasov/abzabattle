import type { BattleMap } from '@/types/map';
import { Faction, type Session } from '@/types/session';
import type { Player } from '@/types/player';
import { defineStore } from 'pinia';
import * as sessionApi from '@/services/sessionApi';
import type { CreateSessionPayload } from '@/services/sessionApi';
import { useUserStore } from '@/stores/user.store';

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
    });
}

function getPlayerInSession(session: Session, playerName: string): Player | null {
    if (session.rebel.player?.name === playerName) return session.rebel.player;
    if (session.imperial.player?.name === playerName) return session.imperial.player;
    return null;
}

export const useSessionStore = defineStore('session', {
    state: () => ({
        onlineSessions: [] as Session[],
        currentSession: null as Session | null,
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
    },
    actions: {
        setOnlineSessions(sessions: Session[]) {
            this.onlineSessions = sessions;
        },
        setCurrentSession(session: Session | null) {
            this.currentSession = session;
        },
        applySessionUpdate(session: Session) {
            const index = this.onlineSessions.findIndex(item => item.id === session.id);
            if (index === -1) {
                this.onlineSessions.unshift(session);
            } else {
                this.onlineSessions[index] = session;
            }
            if (this.currentSession?.id === session.id) {
                this.currentSession = session;
            }
        },
        commitSession(session: Session) {
            this.currentSession = session;
            this.applySessionUpdate(session);
            syncCurrentUserStats(session);
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
            const session = await sessionApi.attackSector(id, { playerName, x, y });
            this.commitSession(session);
            return session;
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
});
