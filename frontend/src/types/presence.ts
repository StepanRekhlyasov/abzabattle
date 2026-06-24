import type { Player } from "@/types/player"

export type PresenceStatus = 'online' | 'offline'

export type PresenceUpdate = {
    name: string
    status: PresenceStatus
}

export type PresenceSnapshot = Player[]
