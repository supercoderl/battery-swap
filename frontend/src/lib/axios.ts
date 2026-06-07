import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { authStorage } from "@/stores/auth-store";
import type { ApiResponse, AuthResponse } from "@/types";

export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000/api";

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
});

/* ---- Request: attach access token ---- */
api.interceptors.request.use((config) => {
  const token = authStorage.get().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

/* ---- Response: transparent refresh on 401 ---- */
let isRefreshing = false;
let queue: { resolve: (t: string) => void; reject: (e: unknown) => void }[] = [];

function flushQueue(error: unknown, token: string | null) {
  queue.forEach((p) => (token ? p.resolve(token) : p.reject(error)));
  queue = [];
}

api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    const refreshToken = authStorage.get().refreshToken;

    const isAuthCall = original?.url?.includes("/auth/");

    if (error.response?.status === 401 && !original._retry && refreshToken && !isAuthCall) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          queue.push({
            resolve: (token) => {
              original.headers.Authorization = `Bearer ${token}`;
              resolve(api(original));
            },
            reject,
          });
        });
      }

      original._retry = true;
      isRefreshing = true;

      try {
        const { data } = await axios.post<ApiResponse<AuthResponse>>(
          `${API_BASE_URL}/auth/refresh`,
          { refreshToken }
        );
        const auth = data.data;
        authStorage.setTokens(auth.accessToken, auth.refreshToken);
        flushQueue(null, auth.accessToken);
        original.headers.Authorization = `Bearer ${auth.accessToken}`;
        return api(original);
      } catch (err) {
        flushQueue(err, null);
        authStorage.clear();
        if (typeof window !== "undefined") window.location.href = "/login";
        return Promise.reject(err);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

/** Extracts a friendly message from an API error response. */
export function getApiErrorMessage(error: unknown, fallback = "Something went wrong."): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiResponse<unknown> | undefined;
    if (data?.errors?.length) return data.errors.join(" ");
    if (data?.message) return data.message;
  }
  return fallback;
}
