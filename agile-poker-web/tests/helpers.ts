import { expect, type Page } from '@playwright/test';

export const BASE_URL = 'http://localhost:5173';

export function uniqueRoomId(prefix: string): string {
    return `${prefix}-${Math.random().toString(36).slice(2, 7)}`;
}

/**
 * Navigate directly to a room URL and join with a player name.
 * This bypasses the SplashPage and goes straight to /room/:roomId,
 * which shows the name-entry form when `room` state is null.
 */
export async function joinRoom(page: Page, roomId: string, playerName: string): Promise<void> {
    const wsPromise = page.waitForEvent('websocket', ws => ws.url().includes('5251'));
    await page.goto(`${BASE_URL}/room/${roomId}`);
    await wsPromise;

    // Buffer for SignalR handshake
    await page.waitForTimeout(200);

    await page.getByPlaceholder('e.g. Alice').fill(playerName);
    await page.getByRole('button', { name: 'Join Table' }).click();

    // Wait until the player card appears — confirms the server acknowledged the join
    await expect(
        page.locator('.flex.flex-col.items-center.bg-white').filter({ hasText: playerName })
    ).toBeVisible({ timeout: 10_000 });
}
