export function formatCreatedAt(value: string | undefined): string {
    if (!value) {
        return '—';
    }

    return new Date(value).toLocaleString();
}
