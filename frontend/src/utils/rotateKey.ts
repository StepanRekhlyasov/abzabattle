const ROTATE_KEYS = new Set(['r', 'к']);

export function isRotateKey(key: string): boolean {
    return ROTATE_KEYS.has(key.toLowerCase());
}
