import { test, expect, chromium } from '@playwright/test';
import {BASE_URL, uniqueRoomId} from './helpers';

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

        await alicePage.goto(BASE_URL);
        await bobPage.goto(BASE_URL);

        // Wait for sockets to open
        await aliceWsPromise;
        await bobWsPromise;

        // Buffer for handshake
        await alicePage.waitForTimeout(200);
        await bobPage.waitForTimeout(200);

        // Alice joins the room
        await alicePage.getByPlaceholder('e.g. sprint-planning').fill(ROOM_ID); // Updated
        await alicePage.getByPlaceholder('Alice').fill('Alice'); // Updated
        await alicePage.getByRole('button', { name: 'Join Table' }).click(); // Updated

        await expect(alicePage.getByText('Alice')).toBeVisible();

        // Bob joins the same room
        await bobPage.getByPlaceholder('e.g. sprint-planning').fill(ROOM_ID); // Updated
        await bobPage.getByPlaceholder('Alice').fill('Bob'); // Updated
        await bobPage.getByRole('button', { name: 'Join Table' }).click(); // Updated

        // Alice should see Bob's name appear in real-time without any page reload
        await expect(alicePage.getByText('Bob')).toBeVisible();
    } finally {
        await aliceContext.close();
        await bobContext.close();
        await browser.close();
    }
});
