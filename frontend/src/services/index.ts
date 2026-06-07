import { api } from "@/lib/axios";
import { createCrudApi, unwrap, cleanParams } from "@/lib/api-helpers";
import type {
  ActiveSession, AuthResponse, Account, Battery, BatteryLog, Cabinet,
  ChartPoint, DashboardCharts, DashboardOverview, DashboardStats, PagedResult,
  QueryParameters, Slot, Station, Transaction, User,
} from "@/types";

/* ---------- CRUD resource APIs ---------- */
export const stationsApi = createCrudApi<Station, StationInput, StationInput, QueryParameters>("stations");
export const cabinetsApi = createCrudApi<Cabinet, CabinetInput, CabinetInput, CabinetQuery>("cabinets");
export const slotsApi = createCrudApi<Slot, SlotInput, SlotUpdateInput, SlotQuery>("slots");
export const usersApi = createCrudApi<User, UserInput, UserInput, UserQuery>("users");
export const batteriesApi = createCrudApi<Battery, BatteryInput, BatteryInput, BatteryQuery>("batteries");

/* ---------- Input types ---------- */
export interface StationInput { address: string; latitude: number; longitude: number; }
export interface CabinetInput { stationId: number; cabinetModel: string; status: string; }
export interface SlotInput { cabinetId: number; slotNumber: number; isHardwareLocked: boolean; currentBatteryId?: number | null; }
export interface SlotUpdateInput { slotNumber: number; isHardwareLocked: boolean; currentBatteryId?: number | null; }
export interface UserInput { fullName: string; phone: string; balanceTrips: number; isActive: boolean; }
export interface BatteryInput { soc: number; temperature: number; voltage: number; healthState: string; locationType: string; holderId?: number | null; }

/* ---------- Query param types ---------- */
export interface CabinetQuery extends QueryParameters { stationId?: number; status?: string; }
export interface SlotQuery extends QueryParameters { cabinetId?: number; isHardwareLocked?: boolean; }
export interface UserQuery extends QueryParameters { isActive?: boolean; }
export interface BatteryQuery extends QueryParameters { healthState?: string; locationType?: string; }
export interface TransactionQuery extends QueryParameters {
  userId?: number; cabinetId?: number; status?: string; fromDate?: string; toDate?: string;
}

/* ---------- Battery telemetry ---------- */
export const batteryLogsApi = {
  list: (batteryId: number, take = 100) =>
    unwrap<BatteryLog[]>(api.get(`/batteries/${batteryId}/logs`, { params: { take } })),
};

/* ---------- Sessions ---------- */
export const sessionsApi = {
  list: (query?: QueryParameters) =>
    unwrap<PagedResult<ActiveSession>>(api.get("/sessions", { params: cleanParams(query ?? {}) })),
  forceClose: async (id: number) => { await api.delete(`/sessions/${id}`); },
};

/* ---------- Transactions ---------- */
export const transactionsApi = {
  list: (query?: TransactionQuery) =>
    unwrap<PagedResult<Transaction>>(api.get("/transactions", { params: cleanParams(query ?? {}) })),
  get: (id: number) => unwrap<Transaction>(api.get(`/transactions/${id}`)),
};

/* ---------- Dashboard ---------- */
export const dashboardApi = {
  stats: () => unwrap<DashboardStats>(api.get("/dashboard/stats")),
  charts: () => unwrap<DashboardCharts>(api.get("/dashboard/charts")),
  overview: () => unwrap<DashboardOverview>(api.get("/dashboard/overview")),
};

/* ---------- Reports ---------- */
export const reportsApi = {
  dailySwaps: (from?: string, to?: string) =>
    unwrap<ChartPoint[]>(api.get("/reports/daily-swaps", { params: cleanParams({ from, to }) })),
  monthlySwaps: (year?: number) =>
    unwrap<ChartPoint[]>(api.get("/reports/monthly-swaps", { params: cleanParams({ year }) })),
  batteryUtilization: () => unwrap<ChartPoint[]>(api.get("/reports/battery-utilization")),
  batteryHealth: () => unwrap<ChartPoint[]>(api.get("/reports/battery-health")),
  cabinetUtilization: () => unwrap<ChartPoint[]>(api.get("/reports/cabinet-utilization")),
};

/* ---------- Auth ---------- */
export const authApi = {
  login: (username: string, password: string) =>
    unwrap<AuthResponse>(api.post("/auth/login", { username, password })),
  logout: async () => { await api.post("/auth/logout"); },
  me: () => unwrap<Account>(api.get("/auth/me")),
  changePassword: async (currentPassword: string, newPassword: string) => {
    await api.post("/auth/change-password", { currentPassword, newPassword });
  },
};
