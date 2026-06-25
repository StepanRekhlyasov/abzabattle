export type Player = {
    name: string;
    wins: number;
    loses: number;
    totalGames: number;
    createdAt?: string;
    status?: 'online' | 'offline';
}
