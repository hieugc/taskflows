import { afterEach, describe, expect, it, vi } from 'vitest';
import { api } from './client';

afterEach(() => vi.unstubAllGlobals());

describe('api client', () => {
  it('sends bearer authorization and parses a successful JSON response', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify({ ok: true }), {
      status: 200, headers: { 'Content-Type': 'application/json' },
    }));
    vi.stubGlobal('fetch', fetchMock);

    const result = await api<{ ok: boolean }>('/health', {}, 'test-token');

    expect(result).toEqual({ ok: true });
    expect(fetchMock.mock.calls[0][1]?.headers).toBeInstanceOf(Headers);
    expect((fetchMock.mock.calls[0][1]?.headers as Headers).get('Authorization')).toBe('Bearer test-token');
  });

  it('returns undefined for a 204 response', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })));
    await expect(api('/projects/1', { method: 'DELETE' })).resolves.toBeUndefined();
  });

  it('throws an ApiError with the problem detail for failed requests', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ title: 'Forbidden', detail: 'Not a project member.' }), {
      status: 403, headers: { 'Content-Type': 'application/problem+json' },
    })));
    await expect(api('/projects/1')).rejects.toMatchObject({ status: 403, message: 'Not a project member.' });
  });
});
