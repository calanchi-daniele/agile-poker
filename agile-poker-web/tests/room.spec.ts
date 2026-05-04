import { test, expect, chromium } from '@playwright/test';
import { BASE_URL } from './helpers';

const ROOM_ID = 'e2e-test';

test('Alice sees Bob join her room in real-time', async () => {
    const browser = await chromium.launch();

    const aliceContext = await browser.newContext();
    const bobContext = await browser.newContext();

    const alicePage = await aliceContext.newPage();
    const bobPage = await bobContext.newPage();

    try {
        // Register WebSocket listeners BEFORE navigating so we never miss the handshake
        const aliceWsReady = alicePage.waitForEvent('websocket', ws => ws.url().includes('5251'));
        const bobWsReady = bobPage.waitForEvent('websocket', ws => ws.url().includes('5251'));

        await alicePage.goto(BASE_URL);
        await bobPage.goto(BASE_URL);

        // Wait until each SignalR connection is fully established before invoking hub methods
        const aliceWs = await aliceWsReady;
        await aliceWs.waitForEvent('framereceived');

        const bobWs = await bobWsReady;
        await bobWs.waitForEvent('framereceived');

        // Alice joins the room
        await alicePage.getByPlaceholder('Room ID (e.g. team-alpha)').fill(ROOM_ID);
        await alicePage.getByPlaceholder('Your Name').fill('Alice');
        await alicePage.getByRole('button', { name: 'Join Room' }).click();

        // Alice should see herself in the room
        await expect(alicePage.getByText('Alice')).toBeVisible();

        // Bob joins the same room
        await bobPage.getByPlaceholder('Room ID (e.g. team-alpha)').fill(ROOM_ID);
        await bobPage.getByPlaceholder('Your Name').fill('Bob');
        await bobPage.getByRole('button', { name: 'Join Room' }).click();

        // Alice should see Bob's name appear in real-time without any page reload
        await expect(alicePage.getByText('Bob')).toBeVisible();
    } finally {
        await aliceContext.close();
        await bobContext.close();
        await browser.close();
    }
});
