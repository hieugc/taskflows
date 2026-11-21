import AxeBuilder from '@axe-core/playwright';
import { expect, test } from '@playwright/test';

const user = {
  id: '11111111-1111-4111-8111-111111111111',
  name: 'Hieu Nguyen',
  email: 'hieu@example.com',
  role: 'Manager' as const,
};
const projectId = '22222222-2222-4222-8222-222222222222';
const taskId = '33333333-3333-4333-8333-333333333333';
const now = '2026-11-21T05:00:00.000Z';

const authResponse = {
  accessToken: 'e2e-access-token',
  expiresAt: '2026-11-21T06:00:00.000Z',
  user,
};

const project = {
  id: projectId,
  name: 'Website redesign',
  description: 'Refresh the public website and improve conversion.',
  status: 'Active',
  startDate: null,
  dueDate: null,
  createdAt: now,
  memberCount: 1,
  taskCount: 0,
  completedTaskCount: 0,
  canManage: true,
};

const emptyDashboard = {
  projectCount: 0,
  openTaskCount: 0,
  completedTaskCount: 0,
  overdueTaskCount: 0,
  statusBreakdown: [],
  priorityBreakdown: [],
  recentTasks: [],
};

async function mockLogin(page: import('@playwright/test').Page) {
  await page.route('**/api/auth/login', route =>
    route.fulfill({ status: 200, json: authResponse }),
  );
  await page.route('**/api/dashboard/summary', route =>
    route.fulfill({ status: 200, json: emptyDashboard }),
  );
}

test('unauthenticated visitors are redirected to sign in', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('heading', { name: 'Good to see you again.' })).toBeVisible();
});

test('login errors are announced and do not create an authenticated session', async ({ page }) => {
  await page.route('**/api/auth/login', route =>
    route.fulfill({
      status: 401,
      contentType: 'application/problem+json',
      body: JSON.stringify({ title: 'Invalid credentials', detail: 'Email or password is incorrect.' }),
    }),
  );

  await page.goto('/login');
  await page.getByLabel('Email address').fill('wrong@example.com');
  await page.getByLabel('Password', { exact: true }).fill('wrong-password');
  await page.getByRole('button', { name: 'Sign in' }).click();

  await expect(page.getByRole('alert')).toHaveText('Email or password is incorrect.');
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.evaluate(() => sessionStorage.getItem('taskflow.token'))).resolves.toBeNull();
});

test('manager can create a project, add a task, update its status, and post a comment', async ({ page }) => {
  await mockLogin(page);

  const projects: typeof project[] = [];
  const tasks: Array<{
    id: string;
    projectId: string;
    title: string;
    description: string;
    status: string;
    priority: string;
    assigneeId: string | null;
    assigneeName: string | null;
    createdById: string;
    dueDate: string | null;
    createdAt: string;
    updatedAt: string;
    commentCount: number;
  }> = [];
  const comments: Array<{
    id: string;
    taskId: string;
    authorId: string;
    authorName: string;
    body: string;
    createdAt: string;
  }> = [];

  await page.route('**/api/projects', async route => {
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON() as { name: string; description: string };
      const created = { ...project, name: body.name, description: body.description };
      projects.push(created);
      await route.fulfill({ status: 201, json: created });
      return;
    }
    await route.fulfill({ status: 200, json: projects });
  });

  await page.route(`**/api/projects/${projectId}`, route =>
    route.fulfill({ status: 200, json: projects[0] ?? project }),
  );

  await page.route(`**/api/projects/${projectId}/members`, route =>
    route.fulfill({
      status: 200,
      json: [{
        userId: user.id,
        name: user.name,
        email: user.email,
        role: 'Manager',
        joinedAt: now,
      }],
    }),
  );

  await page.route(`**/api/projects/${projectId}/tasks`, async route => {
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON() as { title: string; description: string; priority: string };
      const created = {
        id: taskId,
        projectId,
        title: body.title,
        description: body.description,
        status: 'Todo',
        priority: body.priority,
        assigneeId: user.id,
        assigneeName: user.name,
        createdById: user.id,
        dueDate: null,
        createdAt: now,
        updatedAt: now,
        commentCount: 0,
      };
      tasks.push(created);
      await route.fulfill({ status: 201, json: created });
      return;
    }
    await route.fulfill({ status: 200, json: tasks });
  });

  await page.route(`**/api/tasks/${taskId}/status`, async route => {
    const body = route.request().postDataJSON() as { status: string };
    tasks[0].status = body.status;
    await route.fulfill({ status: 204, body: '' });
  });

  await page.route(`**/api/tasks/${taskId}/comments`, async route => {
    if (route.request().method() === 'POST') {
      const body = route.request().postDataJSON() as { body: string };
      const created = {
        id: '44444444-4444-4444-8444-444444444444',
        taskId,
        authorId: user.id,
        authorName: user.name,
        body: body.body.trim(),
        createdAt: now,
      };
      comments.push(created);
      await route.fulfill({ status: 201, json: created });
      return;
    }
    await route.fulfill({ status: 200, json: comments });
  });

  await page.goto('/login');
  await page.getByLabel('Email address').fill(user.email);
  await page.getByLabel('Password', { exact: true }).fill('correct-password');
  await page.getByRole('button', { name: 'Sign in' }).click();
  await expect(page.getByRole('heading', { name: 'Good morning, Hieu' })).toBeVisible();

  await page.getByRole('link', { name: 'Projects', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Projects', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'New project' }).click();
  await page.getByRole('dialog').getByLabel('Project name').fill('Website redesign');
  await page.getByRole('dialog').getByLabel('Description').fill('Refresh the public website.');
  await page.getByRole('dialog').getByRole('button', { name: 'Create project' }).click();

  await expect(page.locator('a.project-card').filter({ hasText: 'Website redesign' })).toBeVisible();
  await page.locator('a.project-card').filter({ hasText: 'Website redesign' }).click();
  await expect(page.getByRole('heading', { name: 'Website redesign' })).toBeVisible();

  await page.getByRole('button', { name: 'Add task' }).click();
  await page.getByRole('dialog').getByLabel('Task title').fill('Prepare initial release');
  await page.getByRole('dialog').getByLabel('Description').fill('Confirm the launch checklist.');
  await page.getByRole('dialog').getByRole('button', { name: 'Add task' }).click();

  const status = page.getByLabel('Status for Prepare initial release');
  await expect(status).toBeVisible();
  await status.selectOption('InProgress');
  await expect(status).toHaveValue('InProgress');

  await page.getByRole('button', { name: 'Open comments for Prepare initial release' }).click();
  await expect(page.getByRole('heading', { name: 'Prepare initial release' })).toBeVisible();
  await page.getByRole('textbox', { name: 'Write a comment' }).fill('Release checklist is ready for review.');
  await page.getByRole('button', { name: 'Send comment' }).click();
  await expect(page.getByText('Release checklist is ready for review.')).toBeVisible();
  await expect(page.getByText('Hieu Nguyen', { exact: true }).last()).toBeVisible();
});

test('sign-in page passes WCAG 2.1 A/AA automated accessibility checks including contrast', async ({ page }) => {
  await page.goto('/login');

  const accessibility = await new AxeBuilder({ page })
    .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
    .analyze();

  expect(accessibility.violations).toEqual([]);
});

test('sign-in page supports keyboard navigation and fits a narrow mobile viewport', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto('/login');

  await page.keyboard.press('Tab');
  await expect(page.getByLabel('Email address')).toBeFocused();
  await page.keyboard.press('Tab');
  await expect(page.getByLabel('Password', { exact: true })).toBeFocused();
  await page.keyboard.press('Tab');
  await expect(page.getByRole('button', { name: 'Show password' })).toBeFocused();
  await page.keyboard.press('Tab');
  await expect(page.getByRole('button', { name: 'Sign in' })).toBeFocused();
  await page.keyboard.press('Tab');
  await expect(page.getByRole('link', { name: 'Create an account' })).toBeFocused();

  const documentWidth = await page.evaluate(() => document.documentElement.scrollWidth);
  expect(documentWidth).toBeLessThanOrEqual(375);
});
