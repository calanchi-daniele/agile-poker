import { test, expect, chromium } from '@playwright/test';
import { BASE_URL, uniqueRoomId, joinRoom } from './helpers';

test('Alice sees Bob join her room in real-time', async () => {
    const ROOM_ID = uniqueRoomId('realtime');

    const browser = await chromium.launch();

    const aliceContext = await browser.newContext();
    const bobContext = await browser.newContext();

    const alicePage = await aliceContext.newPage();
    const bobPage = await bobContext.newPage();

    try {
        const aliceWsPromise = alicePage.waitForEvent('websocket', ws => ws.url().includes('5251'));
        const bobWsPromise = bobPage.waitForEvent('websocket', ws => ws.url().includes('5251'));

        await alicePage.goto(`${BASE_URL}/room/${ROOM_ID}`);
        await bobPage.goto(`${BASE_URL}/room/${ROOM_ID}`);

        await aliceWsPromise;
        await bobWsPromise;

        // Buffer for SignalR handshake
        await alicePage.waitForTimeout(200);
        await bobPage.waitForTimeout(200);

        // Alice joins the room
        await alicePage.getByPlaceholder('e.g. Alice').fill('Alice');
        await alicePage.getByRole('button', { name: 'Join Table' }).click();
        await expect(alicePage.locator('.flex.flex-col.items-center.bg-white').filter({ hasText: 'Alice' })).toBeVisible();

        // Bob joins the same room
        await bobPage.getByPlaceholder('e.g. Alice').fill('Bob');
        await bobPage.getByRole('button', { name: 'Join Table' }).click();

        // Alice should see Bob's card appear in real-time without any page reload
        await expect(alicePage.locator('.flex.flex-col.items-center.bg-white').filter({ hasText: 'Bob' })).toBeVisible();
    } finally {
        await aliceContext.close();
        await bobContext.close();
        await browser.close();
    }
});

test('player can exit the room and return to the splash page', async () => {
    const ROOM_ID = uniqueRoomId('exit');

    const browser = await chromium.launch();
    const context = await browser.newContext();
    const page = await context.newPage();

    try {
        await joinRoom(page, ROOM_ID, 'Alice');

        await page.getByRole('button', { name: /Exit/ }).click();

        // Back on the splash page
        await expect(page).toHaveURL(BASE_URL + '/');
        await expect(page.getByRole('heading', { name: 'Agile Poker' })).toBeVisible();
    } finally {
        await context.close();
        await browser.close();
    }
});
