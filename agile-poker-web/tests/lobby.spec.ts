import { test, expect } from '@playwright/test';
import { BASE_URL, uniqueRoomId, joinRoom } from './helpers';

test.describe('Lobby', () => {
    test('displays all join form elements', async ({ page }) => {
        await page.goto(BASE_URL);

        await expect(page.getByRole('heading', { name: 'Agile Poker' })).toBeVisible();
        await expect(page.getByPlaceholder('e.g. sprint-planning')).toBeVisible(); // Updated
        await expect(page.getByPlaceholder('Alice')).toBeVisible(); // Updated
        await expect(page.getByRole('button', { name: 'Join Table' })).toBeVisible(); // Updated
    });

    test('stays on the lobby when submitted with empty fields', async ({ page }) => {
        await page.goto(BASE_URL);

        await page.getByRole('button', { name: 'Join Table' }).click(); // Updated

        // Still on the lobby — the form should still be visible
        await expect(page.getByPlaceholder('e.g. sprint-planning')).toBeVisible(); // Updated
        await expect(page.getByRole('button', { name: 'Join Table' })).toBeVisible(); // Updated
    });

    test('joining a room navigates to the poker table', async ({ page }) => {
        const roomId = uniqueRoomId('lobby');

        await joinRoom(page, roomId, 'Alice');

        await expect(page.getByRole('heading', { name: `Room: ${roomId}` })).toBeVisible();
        await expect(page.getByRole('button', { name: 'Add Bot' })).toBeVisible();
        await expect(page.getByRole('button', { name: 'Reset Table' })).toBeVisible();
        await expect(page.getByText('Select your estimate')).toBeVisible();
        await expect(page.getByText('Alice')).toBeVisible();
    });
});
