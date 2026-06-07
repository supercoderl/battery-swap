"use client";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { createResourceHooks } from "@/hooks/use-crud";
import { getApiErrorMessage } from "@/lib/axios";
import {
  stationsApi, cabinetsApi, slotsApi, usersApi, batteriesApi, batteryLogsApi,
  sessionsApi, transactionsApi, dashboardApi, reportsApi,
  type StationInput, type CabinetInput, type CabinetQuery, type SlotInput,
  type SlotUpdateInput, type SlotQuery, type UserInput, type UserQuery,
  type BatteryInput, type BatteryQuery, type TransactionQuery,
} from "@/services";
import type {
  Battery, Cabinet, QueryParameters, Slot, Station, User,
} from "@/types";

export const stationHooks = createResourceHooks<Station, StationInput, StationInput, QueryParameters>("stations", stationsApi);
export const cabinetHooks = createResourceHooks<Cabinet, CabinetInput, CabinetInput, CabinetQuery>("cabinets", cabinetsApi);
export const slotHooks = createResourceHooks<Slot, SlotInput, SlotUpdateInput, SlotQuery>("slots", slotsApi);
export const userHooks = createResourceHooks<User, UserInput, UserInput, UserQuery>("users", usersApi);
export const batteryHooks = createResourceHooks<Battery, BatteryInput, BatteryInput, BatteryQuery>("batteries", batteriesApi);

/* ---- Battery telemetry ---- */
export function useBatteryLogs(batteryId?: number, take = 100) {
  return useQuery({
    queryKey: ["batteries", "logs", batteryId, take],
    queryFn: () => batteryLogsApi.list(batteryId!, take),
    enabled: !!batteryId,
    refetchInterval: 15_000,
  });
}

/* ---- Sessions ---- */
export function useSessions(query?: QueryParameters) {
  return useQuery({
    queryKey: ["sessions", query],
    queryFn: () => sessionsApi.list(query),
    refetchInterval: 10_000,
  });
}
export function useForceCloseSession() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => sessionsApi.forceClose(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["sessions"] });
      toast.success("Session closed.");
    },
    onError: (e) => toast.error(getApiErrorMessage(e)),
  });
}

/* ---- Transactions ---- */
export function useTransactions(query?: TransactionQuery) {
  return useQuery({
    queryKey: ["transactions", query],
    queryFn: () => transactionsApi.list(query),
  });
}
export function useTransaction(id?: number) {
  return useQuery({
    queryKey: ["transactions", id],
    queryFn: () => transactionsApi.get(id!),
    enabled: !!id,
  });
}

/* ---- Dashboard ---- */
export function useDashboardOverview() {
  return useQuery({
    queryKey: ["dashboard", "overview"],
    queryFn: () => dashboardApi.overview(),
    refetchInterval: 30_000,
  });
}

/* ---- Reports ---- */
export function useReports() {
  const monthly = useQuery({ queryKey: ["reports", "monthly"], queryFn: () => reportsApi.monthlySwaps() });
  const daily = useQuery({ queryKey: ["reports", "daily"], queryFn: () => reportsApi.dailySwaps() });
  const utilization = useQuery({ queryKey: ["reports", "battery-util"], queryFn: () => reportsApi.batteryUtilization() });
  const health = useQuery({ queryKey: ["reports", "battery-health"], queryFn: () => reportsApi.batteryHealth() });
  const cabinet = useQuery({ queryKey: ["reports", "cabinet-util"], queryFn: () => reportsApi.cabinetUtilization() });
  return { monthly, daily, utilization, health, cabinet };
}
