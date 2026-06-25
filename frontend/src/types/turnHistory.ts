export type SessionActionLog = {
    id: string;
    sequence: number;
    playerName: string;
    actionKind: string;
    message: string;
    payloadJson: string;
    createdAt: string;
};
