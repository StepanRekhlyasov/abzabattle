import * as signalR from '@microsoft/signalr'
import { useSessionStore } from '@/stores/session.store'
import type { Session } from '@/types/session'

let connection: signalR.HubConnection | null = null

function registerHandlers(hub: signalR.HubConnection) {
    hub.on('SessionSnapshot', (sessions: Session[]) => {
        useSessionStore().setOnlineSessions(sessions)
    })
    hub.on('SessionUpdated', (session: Session) => {
        useSessionStore().applySessionUpdate(session)
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
