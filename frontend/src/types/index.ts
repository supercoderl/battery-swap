/* ------- API envelopes ------- */
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface QueryParameters {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

/* ------- Enums (string-serialised by the API) ------- */
export type CabinetStatus = "Online" | "Offline" | "Maintenance";
export type BatteryHealthState = "Good" | "Degraded";
export type BatteryLocationType = "InSlot" | "RentedByUser";
export type SwappingSessionStatus = "Processing" | "HardwareWaiting";
export type SwappingTransactionStatus = "Success" | "FailedSlotOut";

/* ------- Auth ------- */
export interface Account {
  id: number;
  username: string;
  email: string;
  fullName: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  account: Account;
}

/* ------- Entities ------- */
export interface Station {
  id: number;
  address: string;
  latitude: number;
  longitude: number;
  cabinetCount: number;
  createdAt: string;
}

export interface Cabinet {
  id: number;
  stationId: number;
  stationAddress: string;
  cabinetModel: string;
  status: CabinetStatus;
  slotCount: number;
  occupiedSlots: number;
  createdAt: string;
}

export interface Slot {
  id: number;
  cabinetId: number;
  cabinetModel: string;
  slotNumber: number;
  isHardwareLocked: boolean;
  currentBatteryId: number | null;
  batterySoc: number | null;
  createdAt: string;
}

export interface User {
  id: number;
  fullName: string;
  phone: string;
  balanceTrips: number;
  isActive: boolean;
  transactionCount: number;
  createdAt: string;
}

export interface Battery {
  id: number;
  soc: number;
  temperature: number;
  voltage: number;
  healthState: BatteryHealthState;
  locationType: BatteryLocationType;
  holderId: number | null;
  updatedAt: string;
  createdAt: string;
}

export interface BatteryLog {
  id: number;
  batteryId: number;
  soc: number;
  temperature: number;
  voltage: number;
  recordedAt: string;
}

export interface ActiveSession {
  id: number;
  userId: number;
  userFullName: string;
  cabinetId: number;
  cabinetModel: string;
  status: SwappingSessionStatus;
  createdAt: string;
}

export interface Transaction {
  id: number;
  userId: number;
  userFullName: string;
  cabinetId: number;
  cabinetModel: string;
  slotInId: number | null;
  batteryInId: number | null;
  returnedAt: string | null;
  slotOutId: number | null;
  batteryOutId: number | null;
  dispensedAt: string | null;
  status: SwappingTransactionStatus;
  createdAt: string;
  durationSeconds: number | null;
}

/* ------- Dashboard ------- */
export interface DashboardStats {
  totalStations: number;
  totalCabinets: number;
  totalSlots: number;
  totalBatteries: number;
  batteriesInSlot: number;
  batteriesRented: number;
  activeUsers: number;
  totalTransactionsToday: number;
}

export interface ChartPoint {
  label: string;
  value: number;
}

export interface DashboardCharts {
  dailySwaps: ChartPoint[];
  monthlyTrend: ChartPoint[];
  batteryHealthDistribution: ChartPoint[];
  cabinetStatusDistribution: ChartPoint[];
  batterySocDistribution: ChartPoint[];
  mostActiveStations: ChartPoint[];
}

export interface DashboardOverview {
  stats: DashboardStats;
  charts: DashboardCharts;
  latestTransactions: Transaction[];
  latestBatteryLogs: BatteryLog[];
  activeSessions: ActiveSession[];
}
