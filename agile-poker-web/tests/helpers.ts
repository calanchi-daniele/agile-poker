import { expect, type Page } from '@playwright/test';

export const BASE_URL = 'http://localhost:5173';

export function uniqueRoomId(prefix: string): string {
    return `${prefix}-${Math.random().toString(36).slice(2, 7)}`;
}

export async function joinRoom(page: Page, roomId: string, playerName: string): Promise<void> {
    const wsPromise = page.waitForEvent('websocket', ws => ws.url().includes('5251'));
    await page.goto(BASE_URL);
    await wsPromise;

    // Buffer for handshake
    await page.waitForTimeout(200);

    await page.getByPlaceholder('e.g. sprint-planning').fill(roomId);
    await page.getByPlaceholder('Alice').fill(playerName);
    await page.getByRole('button', { name: 'Join Table' }).click(); // Also changed button text!
    await expect(page.getByText(`Room: ${roomId}`)).toBeVisible({ timeout: 10_000 });
}
