import { test, expect, type Page } from '@playwright/test';
import { uniqueRoomId, joinRoom } from './helpers';

// Locates the vote indicator div (the h-24 card face) for a specific named player.
// Scopes to the player card by its unique CSS classes, then drills into the vote div.
// This avoids strict-mode violations that occur when getByText() matches parent containers.
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

            // Wait until Alice sees Bob (real-time sync) before voting
            await expect(alicePage.getByText('Bob')).toBeVisible();

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
            await expect(alicePage.getByText('Bob')).toBeVisible();

            // Both players vote with different values
            await alicePage.getByRole('button', { name: '5', exact: true }).click();
            await bobPage.getByRole('button', { name: '8', exact: true }).click();

            // After 1-second backend delay both cards auto-reveal:
            // voting controls disappear and actual vote values are shown
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
            await expect(alicePage.getByText('Bob')).toBeVisible();

            // Both vote to trigger auto-reveal
            await alicePage.getByRole('button', { name: '3', exact: true }).click();
            await bobPage.getByRole('button', { name: '13', exact: true }).click();
            await expect(alicePage.getByText('Select your estimate')).not.toBeVisible({ timeout: 5000 });

            // Alice resets the table
            await alicePage.getByRole('button', { name: 'Reset Table' }).click();

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
});

test.describe('Bot', () => {
    test('adding a bot adds a new player card to the room', async ({ page }) => {
        const roomId = uniqueRoomId('bot');

        await joinRoom(page, roomId, 'Alice');

        // Only Alice is in the room initially
        const initialCards = page.locator('.flex.flex-col.items-center.bg-white');
        await expect(initialCards).toHaveCount(1);

        await page.getByRole('button', { name: 'Add Bot' }).click();

        // A second player card should appear
        await expect(initialCards).toHaveCount(2, { timeout: 5000 });
    });
});
