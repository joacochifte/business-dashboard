export type ApiError = Error & { status?: number; body?: string };

// Date values across the API are represented as ISO 8601 UTC strings, e.g. "2026-02-05T00:00:00Z".
export type IsoDateTime = string;

type ProblemDetails = {
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  [key: string]: unknown;
};

function makeApiError(message: string, status?: number, body?: string): ApiError {
  const err = new Error(message) as ApiError;
  err.status = status;
  err.body = body;
  return err;
}

async function parseError(res: Response): Promise<{ message: string; bodyText: string }> {
  const contentType = res.headers.get("content-type") ?? "";

  if (contentType.includes("application/problem+json")) {
    try {
      const pd = (await res.json()) as ProblemDetails;
      const msg = (typeof pd.detail === "string" && pd.detail.trim())
        ? pd.detail
        : (typeof pd.title === "string" && pd.title.trim())
          ? pd.title
          : `Request failed: ${res.status}`;
      return { message: msg, bodyText: JSON.stringify(pd) };
    } catch {
      // Fall through to text parsing.
    }
  }

  const text = await res.text().catch(() => "");
  const msg = text.trim() ? text : `Request failed: ${res.status}`;
  return { message: msg, bodyText: text };
}

function joinUrl(baseUrl: string, path: string) {
  const base = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;
  const p = path.startsWith("/") ? path : `/${path}`;
  return `${base}${p}`;
}

export function getApiBaseUrl(): string {
  const browserBase = process.env.NEXT_PUBLIC_API_BASE_URL;
  const serverBase = process.env.API_BASE_URL_INTERNAL ?? browserBase;
  const baseUrl = typeof window === "undefined" ? serverBase : browserBase;
  if (!baseUrl) throw makeApiError("Missing API base URL");
  return baseUrl;
}

export function toIsoUtc(dt: Date): IsoDateTime {
  return dt.toISOString();
}

export function toIsoUtcStartOfDay(d: Date): IsoDateTime {
  const utc = new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate(), 0, 0, 0, 0));
  return utc.toISOString();
}

export function toIsoUtcEndOfDay(d: Date): IsoDateTime {
  const utc = new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate(), 23, 59, 59, 999));
  return utc.toISOString();
}

// Server-side fetch
export async function apiFetchServer(path: string, init?: RequestInit) {
  const res = await fetch(joinUrl(getApiBaseUrl(), path), init);

  if (!res.ok) {
    const err = await parseError(res);
    throw makeApiError(err.message, res.status, err.bodyText);
  }

  return res;
}

// Client-side fetch
export async function apiFetchClient(path: string, init?: RequestInit) {
  const res = await fetch(joinUrl(getApiBaseUrl(), path), init);

  if (!res.ok) {
    const err = await parseError(res);
    throw makeApiError(err.message, res.status, err.bodyText);
  }

  return res;
}

// JSON helpers
export async function apiJsonServer<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await apiFetchServer(path, init);
  return res.json() as Promise<T>;
}

export async function apiJsonClient<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await apiFetchClient(path, init);
  return res.json() as Promise<T>;
}

// Convenience: use in simple cases where you don't care about server/client naming.
export async function fetchJson<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(joinUrl(getApiBaseUrl(), path), init);

  if (!res.ok) {
    const err = await parseError(res);
    throw makeApiError(err.message, res.status, err.bodyText);
  }

  return res.json() as Promise<T>;
}
