import { test, expect } from '@playwright/test';
import { BASE_URL, uniqueRoomId, joinRoom } from './helpers';

test.describe('SplashPage – UI elements', () => {
    test('displays the create-room and join-by-code form elements', async ({ page }) => {
        await page.goto(BASE_URL);

        // Branding
        await expect(page.getByRole('heading', { name: 'Agile Poker' })).toBeVisible();
        await expect(page.getByText('Real-time team estimation')).toBeVisible();

        // Create-room form
        await expect(page.getByPlaceholder('e.g. Sprint 42 Planning')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Create' })).toBeVisible();

        // Join-by-code form
        await expect(page.getByPlaceholder('ENTER CODE')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Join' })).toBeVisible();

        // Lobby panel
        await expect(page.getByRole('heading', { name: 'Lobby' })).toBeVisible();
    });

    test('Create button is disabled when the room name is empty', async ({ page }) => {
        await page.goto(BASE_URL);
        await expect(page.getByRole('button', { name: 'Create' })).toBeDisabled();
    });

    test('Join button is disabled when the code field is empty', async ({ page }) => {
        await page.goto(BASE_URL);
        await expect(page.getByRole('button', { name: 'Join' })).toBeDisabled();
    });

    test('Create button becomes enabled after typing a room name', async ({ page }) => {
        await page.goto(BASE_URL);
        await page.getByPlaceholder('e.g. Sprint 42 Planning').fill('Sprint 42');
        await expect(page.getByRole('button', { name: 'Create' })).toBeEnabled();
    });

    test('Join button becomes enabled after typing a room code', async ({ page }) => {
        await page.goto(BASE_URL);
        await page.getByPlaceholder('ENTER CODE').fill('ABC123');
        await expect(page.getByRole('button', { name: 'Join' })).toBeEnabled();
    });
});

test.describe('SplashPage – navigation', () => {
    test('creating a room navigates to the room URL and shows the player-name entry form', async ({ page }) => {
        await page.goto(BASE_URL);

        await page.getByPlaceholder('e.g. Sprint 42 Planning').fill('Test Sprint');
        await page.getByRole('button', { name: 'Create' }).click();

        // URL must contain /room/ and the room-name query param
        await expect(page).toHaveURL(/\/room\/.+\?name=Test(%20|\+)Sprint/);

        // The PokerRoom name-entry form appears
        await expect(page.getByPlaceholder('e.g. Alice')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Join Table' })).toBeVisible();
    });

    test('joining via code navigates to the room URL and shows the player-name entry form', async ({ page }) => {
        await page.goto(BASE_URL);

        await page.getByPlaceholder('ENTER CODE').fill('TESTAB');
        await page.getByRole('button', { name: 'Join' }).click();

        // Code is uppercased before navigation
        await expect(page).toHaveURL(/\/room\/TESTAB/);

        // The PokerRoom name-entry form appears
        await expect(page.getByPlaceholder('e.g. Alice')).toBeVisible();
        await expect(page.getByRole('button', { name: 'Join Table' })).toBeVisible();
    });

    test('fully joining a room from the splash page shows the poker table', async ({ page }) => {
        const wsPromise = page.waitForEvent('websocket', ws => ws.url().includes('5251'));
        await page.goto(BASE_URL);
        await wsPromise;
        await page.waitForTimeout(200);

        // Step 1: create a room
        await page.getByPlaceholder('e.g. Sprint 42 Planning').fill('E2E Session');
        await page.getByRole('button', { name: 'Create' }).click();
        await expect(page).toHaveURL(/\/room\/.+\?name=E2E(%20|\+)Session/);

        // Step 2: enter player name and join
        await page.getByPlaceholder('e.g. Alice').fill('Alice');
        await page.getByRole('button', { name: 'Join Table' }).click();

        // Poker table appears with key controls
        await expect(page.locator('.flex.flex-col.items-center.bg-white').filter({ hasText: 'Alice' }))
            .toBeVisible({ timeout: 10_000 });
        await expect(page.getByRole('button', { name: /Add Bot/ })).toBeVisible();
        await expect(page.getByRole('button', { name: /Reset Table/ })).toBeVisible();
        await expect(page.getByText('Select your estimate')).toBeVisible();
    });

    test('exit button returns to the splash page', async ({ page }) => {
        const roomId = uniqueRoomId('exit');
        await joinRoom(page, roomId, 'Alice');

        await page.getByRole('button', { name: /Exit/ }).click();

        await expect(page).toHaveURL(BASE_URL + '/');
        await expect(page.getByRole('heading', { name: 'Agile Poker' })).toBeVisible();
    });
});
