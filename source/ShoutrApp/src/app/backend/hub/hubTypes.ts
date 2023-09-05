export interface HubPeer {
    id?: string;
    nickname?: string;
    publicKey?: string;
}

export interface HubBroadcast {
    id?: string;
    completed?: boolean;
    percentComplete?: number;
}

