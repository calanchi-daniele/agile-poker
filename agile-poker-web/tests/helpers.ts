import { expect, type Page } from '@playwright/test';

export const BASE_URL = 'http://localhost:5173';

export function uniqueRoomId(prefix: string): string {
    return `${prefix}-${Math.random().toString(36).slice(2, 7)}`;
}

export async function joinRoom(page: Page, roomId: string, playerName: string): Promise<void> {
    // Register the WebSocket listener BEFORE navigating so we never miss the event
    const wsReady = page.waitForEvent('websocket', ws => ws.url().includes('5251'));

    await page.goto(BASE_URL);

    // Wait until the SignalR WebSocket is open AND the server has confirmed the handshake
    // (server responds with `{}\x1e`), meaning connection.invoke() calls are now safe
    const ws = await wsReady;
    await ws.waitForEvent('framereceived');

    await page.getByPlaceholder('Room ID (e.g. team-alpha)').fill(roomId);
    await page.getByPlaceholder('Your Name').fill(playerName);
    await page.getByRole('button', { name: 'Join Room' }).click();
    await expect(page.getByText(`Room: ${roomId}`)).toBeVisible({ timeout: 10_000 });
}
