"use client";
import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { authApi } from "@/services";
import { useAuthStore } from "@/stores/auth-store";
import { getApiErrorMessage } from "@/lib/axios";

export function useAuth() {
  return useAuthStore();
}

export function useLogin() {
  const router = useRouter();
  const setAuth = useAuthStore((s) => s.setAuth);

  return useMutation({
    mutationFn: ({ username, password }: { username: string; password: string }) =>
      authApi.login(username, password),
    onSuccess: (data) => {
      setAuth({
        account: data.account,
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
      });
      toast.success(`Welcome back, ${data.account.fullName}`);
      router.replace("/dashboard");
    },
    onError: (error) => toast.error(getApiErrorMessage(error, "Invalid credentials.")),
  });
}

export function useLogout() {
  const router = useRouter();
  const clear = useAuthStore((s) => s.clear);

  return useMutation({
    mutationFn: () => authApi.logout(),
    onSettled: () => {
      clear();
      router.replace("/login");
    },
  });
}

export function useChangePassword() {
  return useMutation({
    mutationFn: ({ currentPassword, newPassword }: { currentPassword: string; newPassword: string }) =>
      authApi.changePassword(currentPassword, newPassword),
    onSuccess: () => toast.success("Password changed successfully."),
    onError: (error) => toast.error(getApiErrorMessage(error)),
  });
}
