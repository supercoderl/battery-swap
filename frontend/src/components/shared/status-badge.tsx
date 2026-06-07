import { Badge } from "@/components/ui/badge";
import type {
  BatteryHealthState, BatteryLocationType, CabinetStatus,
  SwappingSessionStatus, SwappingTransactionStatus,
} from "@/types";

export function CabinetStatusBadge({ status }: { status: CabinetStatus }) {
  const variant = status === "Online" ? "success" : status === "Maintenance" ? "warning" : "secondary";
  return <Badge variant={variant}>{status}</Badge>;
}

export function HealthBadge({ state }: { state: BatteryHealthState }) {
  return <Badge variant={state === "Good" ? "success" : "warning"}>{state}</Badge>;
}

export function LocationBadge({ type }: { type: BatteryLocationType }) {
  return <Badge variant={type === "InSlot" ? "default" : "secondary"}>{type === "InSlot" ? "In Slot" : "Rented"}</Badge>;
}

export function SessionStatusBadge({ status }: { status: SwappingSessionStatus }) {
  return <Badge variant={status === "Processing" ? "default" : "warning"}>{status === "HardwareWaiting" ? "Hardware Waiting" : "Processing"}</Badge>;
}

export function TransactionStatusBadge({ status }: { status: SwappingTransactionStatus }) {
  return <Badge variant={status === "Success" ? "success" : "destructive"}>{status === "FailedSlotOut" ? "Failed (Slot Out)" : "Success"}</Badge>;
}

export function SocBadge({ soc }: { soc: number }) {
  const variant = soc >= 60 ? "success" : soc >= 30 ? "warning" : "destructive";
  return <Badge variant={variant}>{soc}%</Badge>;
}
