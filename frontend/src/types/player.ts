export type Player = {
    name: string;
    wins: number;
    loses: number;
    totalGames: number;
    status?: 'online' | 'offline';
}
