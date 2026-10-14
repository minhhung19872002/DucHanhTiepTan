/** Lower-case and strip Vietnamese diacritics for accent-insensitive client-side filtering. */
export function VietnameseNormalize(s?: string | null): string {
  return (s ?? '')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/đ/g, 'd')
    .replace(/Đ/g, 'D')
    .toLowerCase()
    .trim()
}
