import api from '@/services/api';
import type { BattleMap } from '@/types/map';
import type { Faction, Session } from '@/types/session';
import type { SessionActionLog } from '@/types/turnHistory';

export type CreateSessionPayload = {
    name: string;
    faction: Faction;
    ptsLimit: number;
    mapSize: number;
    playerName: string;
    battleMap: BattleMap;
};

export type StartBattlePayload = {
    playerName: string;
    battleMap: BattleMap;
};

export type AttackSectorPayload = {
    playerName: string;
    x: number;
    y: number;
};

export type UseAbilityPayload = {
    playerName: string;
    abilityKind: string;
    x: number;
    y: number;
    sourceEntityId?: string;
};

export async function fetchSession(id: string, playerName?: string) {
    const response = await api.get<Session>(`/sessions/${id}`, {
        params: playerName ? { playerName } : undefined,
    });
    return response.data;
}

export async function createSession(payload: CreateSessionPayload) {
    const response = await api.post<Session>('/sessions', payload);
    return response.data;
}

export async function joinSession(id: string, playerName: string) {
    const response = await api.post<Session>(`/sessions/${id}/join`, { playerName });
    return response.data;
}

export async function startBattle(id: string, payload: StartBattlePayload) {
    const response = await api.post<Session>(`/sessions/${id}/start`, payload);
    return response.data;
}

export async function attackSector(id: string, payload: AttackSectorPayload) {
    const response = await api.post<Session>(`/sessions/${id}/attack`, payload);
    return response.data;
}

export async function useAbility(id: string, payload: UseAbilityPayload) {
    const response = await api.post<Session>(`/sessions/${id}/ability`, payload);
    return response.data;
}

export async function fetchSessionHistory(id: string) {
    const response = await api.get<SessionActionLog[]>(`/sessions/${id}/history`);
    return response.data;
}
