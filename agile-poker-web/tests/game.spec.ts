import { test, expect, type Page } from '@playwright/test';
import { uniqueRoomId, joinRoom } from './helpers';

/**
 * Locates the vote-card face (the tall card div) for a specific player.
 * Scopes to the player card by its CSS classes + player name text, then
 * drills into the inner h-36 vote face. Using a scoped locator avoids
 * strict-mode violations from getByText() matching parent containers.
 */
const voteCard = (page: Page, name: string) =>
    page.locator('.flex.flex-col.items-center.bg-white')
        .filter({ hasText: name })
        .locator('.h-36');

test.describe('Voting', () => {
    test('voted player shows thumbs-up while unvoted player shows waiting indicator', async ({ browser }) => {
        const roomId = uniqueRoomId('voting');

        const aliceCtx = await browser.newContext();
        const bobCtx = await browser.newContext();
        const alicePage = await aliceCtx.newPage();
        const bobPage = await bobCtx.newPage();

        try {
            await joinRoom(alicePage, roomId, 'Alice');
            await joinRoom(bobPage, roomId, 'Bob');

            // Wait until Alice sees Bob's card (real-time sync) before voting
            await expect(alicePage.locator('.flex.flex-col.items-center.bg-white').filter({ hasText: 'Bob' })).toBeVisible();

            // Only Alice votes
            await alicePage.getByRole('button', { name: '5', exact: true }).click();

            // Alice's card shows thumbs-up; Bob's card still shows waiting indicator
            await expect(voteCard(alicePage, 'Alice')).toHaveText('👍');
            await expect(voteCard(alicePage, 'Bob')).toHaveText('...');

            // Bob's view reflects Alice's vote status in real-time
            await expect(voteCard(bobPage, 'Alice')).toHaveText('👍');
        } finally {
            await aliceCtx.close();
            await bobCtx.close();
        }
    });

    test('cards auto-reveal when all players have voted', async ({ browser }) => {
        const roomId = uniqueRoomId('reveal');

        const aliceCtx = await browser.newContext();
        const bobCtx = await browser.newContext();
        const alicePage = await aliceCtx.newPage();
        const bobPage = await bobCtx.newPage();

        try {
            await joinRoom(alicePage, roomId, 'Alice');
            await joinRoom(bobPage, roomId, 'Bob');
            await expect(alicePage.locator('.flex.flex-col.items-center.bg-white').filter({ hasText: 'Bob' })).toBeVisible();

            // Both players vote with different values
            await alicePage.getByRole('button', { name: '5', exact: true }).click();
            await bobPage.getByRole('button', { name: '8', exact: true }).click();

            // After backend auto-reveal: voting controls disappear and actual values are shown
            await expect(alicePage.getByText('Select your estimate')).not.toBeVisible({ timeout: 5000 });
            await expect(voteCard(alicePage, 'Alice')).toHaveText('5');
            await expect(voteCard(alicePage, 'Bob')).toHaveText('8');

            // Bob's view shows the same revealed state
            await expect(voteCard(bobPage, 'Alice')).toHaveText('5');
            await expect(voteCard(bobPage, 'Bob')).toHaveText('8');
        } finally {
            await aliceCtx.close();
            await bobCtx.close();
        }
    });

    test('reset table clears votes and shows voting controls again', async ({ browser }) => {
        const roomId = uniqueRoomId('reset');

        const aliceCtx = await browser.newContext();
        const bobCtx = await browser.newContext();
        const alicePage = await aliceCtx.newPage();
        const bobPage = await bobCtx.newPage();

        try {
            await joinRoom(alicePage, roomId, 'Alice');
            await joinRoom(bobPage, roomId, 'Bob');
            await expect(alicePage.locator('.flex.flex-col.items-center.bg-white').filter({ hasText: 'Bob' })).toBeVisible();

            // Both vote to trigger auto-reveal
            await alicePage.getByRole('button', { name: '3', exact: true }).click();
            await bobPage.getByRole('button', { name: '13', exact: true }).click();
            await expect(alicePage.getByText('Select your estimate')).not.toBeVisible({ timeout: 5000 });

            // Alice resets the table
            await alicePage.getByRole('button', { name: /Reset Table/ }).click();

            // Voting controls reappear and every card resets to the waiting indicator
            await expect(alicePage.getByText('Select your estimate')).toBeVisible();
            await expect(voteCard(alicePage, 'Alice')).toHaveText('...');
            await expect(voteCard(alicePage, 'Bob')).toHaveText('...');

            // Bob also sees the reset state
            await expect(bobPage.getByText('Select your estimate')).toBeVisible();
        } finally {
            await aliceCtx.close();
            await bobCtx.close();
        }
    });

    test('lone player can vote and card is revealed immediately', async ({ page }) => {
        const roomId = uniqueRoomId('solo');
        await joinRoom(page, roomId, 'Alice');

        await page.getByRole('button', { name: '13', exact: true }).click();

        // With a single player, voting should auto-reveal
        await expect(voteCard(page, 'Alice')).toHaveText('13', { timeout: 5000 });
    });
});

test.describe('Bot', () => {
    test('adding a bot adds a new player card to the room', async ({ page }) => {
        const roomId = uniqueRoomId('bot');

        await joinRoom(page, roomId, 'Alice');

        // Only Alice is in the room initially
        const playerCards = page.locator('.flex.flex-col.items-center.bg-white');
        await expect(playerCards).toHaveCount(1);

        await page.getByRole('button', { name: /Add Bot/ }).click();

        // A second player card should appear
        await expect(playerCards).toHaveCount(2, { timeout: 5000 });
    });

    test('bot votes automatically and triggers reveal when only bot and one player', async ({ page }) => {
        const roomId = uniqueRoomId('botreveal');
        await joinRoom(page, roomId, 'Alice');

        await page.getByRole('button', { name: /Add Bot/ }).click();

        // Wait for bot to appear
        const playerCards = page.locator('.flex.flex-col.items-center.bg-white');
        await expect(playerCards).toHaveCount(2, { timeout: 5000 });

        // Alice votes; the bot has a random 3–8 s timer on the backend,
        // so we need a generous timeout to cover 8 s + 1 s reveal delay.
        await page.getByRole('button', { name: '5', exact: true }).click();

        await expect(page.getByText('Select your estimate')).not.toBeVisible({ timeout: 10_000 });
    });
});

test.describe('Navigation', () => {
    test('exit button returns the player to the splash page', async ({ page }) => {
        const roomId = uniqueRoomId('nav');
        await joinRoom(page, roomId, 'Alice');

        await page.getByRole('button', { name: /Exit/ }).click();

        // Should land back on the root SplashPage
        await expect(page).toHaveURL('http://localhost:5173/');
        await expect(page.getByRole('heading', { name: 'Agile Poker' })).toBeVisible();
    });
});
