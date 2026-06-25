import { computed, nextTick, ref } from 'vue';
import * as sessionApi from '@/services/sessionApi';
import { useAppStore } from '@/stores/app.store';
import type { Session } from '@/types/session';
import { Faction } from '@/types/session';
import type { ReplayFrame, ReplaySnapshot, SessionActionPayload } from '@/types/replay';
import type { BattleMap } from '@/types/map';

function parseSnapshot(raw: unknown): ReplaySnapshot | null {
    if (!raw || typeof raw !== 'object') return null;

    const snapshot = raw as Record<string, unknown>;
    const currentTurn = snapshot.currentTurn;
    if (currentTurn !== Faction.Rebel && currentTurn !== Faction.Imperial) {
        return null;
    }

    return {
        rebelBattleMap: (snapshot.rebelBattleMap as BattleMap | null) ?? null,
        imperialBattleMap: (snapshot.imperialBattleMap as BattleMap | null) ?? null,
        currentTurn,
        hitsThisTurn: typeof snapshot.hitsThisTurn === 'number' ? snapshot.hitsThisTurn : 0,
    };
}

function buildFramesFromLogs(logs: Awaited<ReturnType<typeof sessionApi.fetchSessionHistory>>): ReplayFrame[] {
    return logs.flatMap(log => {
        let payload: SessionActionPayload;
        try {
            payload = JSON.parse(log.payloadJson) as SessionActionPayload;
        } catch {
            return [];
        }

        const snapshot = parseSnapshot(payload.snapshot);
        if (!snapshot) {
            return [];
        }

        return [{
            log,
            message: log.message,
            snapshot,
        }];
    });
}

function buildFallbackFrame(session: Session): ReplayFrame | null {
    const rebelBattleMap = session.rebel.battleMap;
    const imperialBattleMap = session.imperial.battleMap;
    if (!rebelBattleMap || !imperialBattleMap) {
        return null;
    }

    return {
        log: {
            id: 'fallback',
            sequence: 0,
            playerName: 'system',
            actionKind: 'battle-start',
            message: 'Battle started.',
            payloadJson: '{}',
            createdAt: new Date().toISOString(),
        },
        message: 'Replay is not available for this session.',
        snapshot: {
            rebelBattleMap,
            imperialBattleMap,
            currentTurn: Faction.Rebel,
            hitsThisTurn: 0,
        },
    };
}

export function getReplayTurnLabel(
    snapshot: ReplaySnapshot,
    session: Session,
    viewerName?: string | null,
): string {
    const turnPlayerName = snapshot.currentTurn === Faction.Rebel
        ? session.rebel.player?.name
        : session.imperial.player?.name;
    const viewerFaction = viewerName
        ? (session.rebel.player?.name === viewerName
            ? Faction.Rebel
            : session.imperial.player?.name === viewerName
                ? Faction.Imperial
                : null)
        : null;
    const isViewerTurn = viewerFaction !== null && viewerFaction === snapshot.currentTurn;
    const ownerLabel = isViewerTurn ? 'Your turn' : turnPlayerName ? `${turnPlayerName}'s turn` : "Opponent's turn";
    const hits = snapshot.hitsThisTurn;

    if (hits === 1) return `${ownerLabel} (Second shot!)`;
    if (hits === 2) return `${ownerLabel} (Third shot!)`;
    return ownerLabel;
}

export function useSessionReplay(session: () => Session | null) {
    const appStore = useAppStore();
    const frames = ref<ReplayFrame[]>([]);
    const currentIndex = ref(0);
    const loading = ref(false);
    const error = ref<string | null>(null);
    const initialized = ref(false);

    const currentFrame = computed(() => frames.value[currentIndex.value] ?? null);
    const canGoPrevious = computed(() => !loading.value && currentIndex.value > 0);
    const canGoNext = computed(() => !loading.value && currentIndex.value < frames.value.length - 1);
    const isAtLastFrame = computed(() =>
        frames.value.length > 0 && currentIndex.value === frames.value.length - 1,
    );
    const hasReplay = computed(() => frames.value.length > 1);

    const waitForFrameRender = async () => {
        await nextTick();
        await new Promise<void>(resolve => {
            requestAnimationFrame(() => {
                requestAnimationFrame(() => resolve());
            });
        });
    };

    const loadReplay = async () => {
        const current = session();
        if (!current) {
            return;
        }

        loading.value = true;
        error.value = null;
        initialized.value = false;
        try {
            const logs = await sessionApi.fetchSessionHistory(current.id);
            const parsedFrames = buildFramesFromLogs(logs);
            if (parsedFrames.length > 0) {
                frames.value = parsedFrames;
            } else {
                const fallback = buildFallbackFrame(current);
                frames.value = fallback ? [fallback] : [];
                if (frames.value.length === 1) {
                    error.value = 'Replay is not available for this session.';
                }
            }
            currentIndex.value = 0;
        } catch {
            error.value = 'Failed to load replay';
            frames.value = [];
        } finally {
            loading.value = false;
            initialized.value = true;
        }
    };

    const navigate = async (delta: -1 | 1) => {
        if (loading.value) return;

        const nextIndex = currentIndex.value + delta;
        if (nextIndex < 0 || nextIndex >= frames.value.length) {
            return;
        }

        loading.value = true;
        appStore.setLoading(true);
        try {
            currentIndex.value = nextIndex;
            await waitForFrameRender();
        } finally {
            loading.value = false;
            appStore.setLoading(false);
        }
    };

    const goPrevious = () => navigate(-1);
    const goNext = () => navigate(1);

    return {
        frames,
        currentIndex,
        currentFrame,
        loading,
        error,
        initialized,
        hasReplay,
        canGoPrevious,
        canGoNext,
        isAtLastFrame,
        loadReplay,
        goPrevious,
        goNext,
    };
}
