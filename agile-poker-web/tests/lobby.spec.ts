import { test, expect } from '@playwright/test';
import { BASE_URL, uniqueRoomId, joinRoom } from './helpers';

test.describe('Lobby', () => {
    test('displays all join form elements', async ({ page }) => {
        await page.goto(BASE_URL);

        await expect(page.getByRole('heading', { name: 'Agile Poker' })).toBeVisible();
        await expect(page.getByPlaceholder('Room ID (e.g. team-alpha)')).toBeVisible();
        await expect(page.getByPlaceholder('Your Name')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Join Room' })).toBeVisible();
    });

    test('stays on the lobby when submitted with empty fields', async ({ page }) => {
        await page.goto(BASE_URL);

        await page.getByRole('button', { name: 'Join Room' }).click();

        // Still on the lobby — the form should still be visible
        await expect(page.getByPlaceholder('Room ID (e.g. team-alpha)')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Join Room' })).toBeVisible();
    });

    test('joining a room navigates to the poker table', async ({ page }) => {
        const roomId = uniqueRoomId('lobby');

        await joinRoom(page, roomId, 'Alice');

        await expect(page.getByRole('heading', { name: `Room: ${roomId}` })).toBeVisible();
        await expect(page.getByRole('button', { name: 'Add Bot' })).toBeVisible();
        await expect(page.getByRole('button', { name: 'Reset Table' })).toBeVisible();
        await expect(page.getByText('Cast your vote:')).toBeVisible();
        await expect(page.getByText('Alice')).toBeVisible();
    });
});
