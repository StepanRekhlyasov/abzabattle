import * as signalR from '@microsoft/signalr'
import router from '@/router'
import { useSessionStore } from '@/stores/session.store'
import type { Session } from '@/types/session'

let connection: signalR.HubConnection | null = null

function registerHandlers(hub: signalR.HubConnection) {
    hub.on('SessionSnapshot', (sessions: Session[]) => {
        useSessionStore().setOnlineSessions(sessions)
    })
    hub.on('SessionUpdated', (session: Session) => {
        const sessionStore = useSessionStore();
        if (sessionStore.currentSession?.id === session.id) {
            sessionStore.commitSession(session);
            return;
        }
        sessionStore.applySessionUpdate(session);
    })
    hub.on('SessionDeleted', (sessionId: string) => {
        const sessionStore = useSessionStore();
        const wasCurrent = sessionStore.currentSession?.id === sessionId;
        sessionStore.removeSession(sessionId);
        if (wasCurrent) {
            void router.push('/');
        }
    })
}

export async function connectSessionWs(name: string) {
    if (connection?.state === signalR.HubConnectionState.Connected) {
        return
    }
    await disconnectSessionWs()
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`/hubs/sessions?name=${encodeURIComponent(name)}`)
        .withAutomaticReconnect()
        .build()
    registerHandlers(connection)
    await connection.start()
}

export async function disconnectSessionWs() {
    if (!connection) {
        return
    }
    await connection.stop()
    connection = null
}
