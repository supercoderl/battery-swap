import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { Account } from "@/types";

interface AuthState {
  account: Account | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: (data: {
    account: Account;
    accessToken: string;
    refreshToken: string;
  }) => void;
  setTokens: (accessToken: string, refreshToken: string) => void;
  clear: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      account: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      setAuth: ({ account, accessToken, refreshToken }) =>
        set({ account, accessToken, refreshToken, isAuthenticated: true }),
      setTokens: (accessToken, refreshToken) => set({ accessToken, refreshToken }),
      clear: () =>
        set({ account: null, accessToken: null, refreshToken: null, isAuthenticated: false }),
    }),
    { name: "battery-swap-auth" }
  )
);

/** Non-reactive accessors for use inside the axios interceptor. */
export const authStorage = {
  get: () => useAuthStore.getState(),
  setTokens: (a: string, r: string) => useAuthStore.getState().setTokens(a, r),
  clear: () => useAuthStore.getState().clear(),
};
