import * as signalR from '@microsoft/signalr'
import { useUserStore } from '@/stores/user.store'
import type { PresenceSnapshot, PresenceUpdate } from '@/types/presence'

let connection: signalR.HubConnection | null = null

function registerHandlers(hub: signalR.HubConnection) {
    hub.on('PresenceSnapshot', (users: PresenceSnapshot) => {
        useUserStore().setOnlineUsers(users)
    })

    hub.on('PresenceUpdated', (update: PresenceUpdate) => {
        useUserStore().updatePresence(update)
    })
}

export async function connectWs(name: string) {
    if (connection?.state === signalR.HubConnectionState.Connected) {
        return
    }

    await disconnectWs()

    connection = new signalR.HubConnectionBuilder()
        .withUrl(`/hubs/presence?name=${encodeURIComponent(name)}`)
        .withAutomaticReconnect()
        .build()

    registerHandlers(connection)
    await connection.start()
}

export async function disconnectWs() {
    if (!connection) {
        return
    }

    await connection.stop()
    connection = null
    useUserStore().clearOnlineUsers()
}
