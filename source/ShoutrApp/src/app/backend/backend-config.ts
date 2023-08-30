export interface BackendModel {
    readonly UserFingerprint?: string;
    readonly UserPublicKey?: string;
}

export interface BackendConfig {
    readonly UserFingerprint: string;
    readonly UserPublicKey: string;
}
