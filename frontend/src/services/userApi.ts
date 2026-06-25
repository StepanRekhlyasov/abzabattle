import api from '@/services/api';

export async function deleteUser(targetName: string, adminName: string) {
    await api.delete(`/users/${encodeURIComponent(targetName)}`, {
        params: { adminName },
    });
}
