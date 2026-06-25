export const ADMIN_USER_NAME = 'Admin';

export function isAdmin(name?: string | null): boolean {
    return name === ADMIN_USER_NAME;
}
