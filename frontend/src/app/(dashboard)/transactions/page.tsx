"use client";
import * as React from "react";
import { ArrowLeftRight, Eye } from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { DataTable, type Column } from "@/components/shared/data-table";
import { TransactionStatusBadge } from "@/components/shared/status-badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from "@/components/ui/dialog";
import { useTransactions } from "@/hooks/use-resources";
import { useDebounce } from "@/hooks/use-debounce";
import { formatDate, formatDuration } from "@/lib/utils";
import type { Transaction } from "@/types";

function DetailRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between border-b py-2 text-sm last:border-0">
      <span className="text-muted-foreground">{label}</span>
      <span className="font-medium">{value ?? "—"}</span>
    </div>
  );
}

export default function TransactionsPage() {
  const [page, setPage] = React.useState(1);
  const [search, setSearch] = React.useState("");
  const [status, setStatus] = React.useState("all");
  const [fromDate, setFromDate] = React.useState("");
  const [toDate, setToDate] = React.useState("");
  const debouncedSearch = useDebounce(search);

  const { data, isLoading } = useTransactions({
    page, pageSize: 10, search: debouncedSearch,
    status: status === "all" ? undefined : status,
    fromDate: fromDate || undefined,
    toDate: toDate || undefined,
  });

  const [detail, setDetail] = React.useState<Transaction | null>(null);

  const columns: Column<Transaction>[] = [
    { header: "ID", cell: (t) => <span className="text-muted-foreground">#{t.id}</span>, className: "w-16" },
    { header: "User", cell: (t) => <div className="flex items-center gap-2 font-medium"><ArrowLeftRight className="h-4 w-4 text-primary" /> {t.userFullName}</div> },
    { header: "Cabinet", cell: (t) => <span className="text-muted-foreground">{t.cabinetModel}</span> },
    { header: "Returned", cell: (t) => t.batteryInId ? `#${t.batteryInId}` : "—" },
    { header: "Dispensed", cell: (t) => t.batteryOutId ? `#${t.batteryOutId}` : "—" },
    { header: "Duration", cell: (t) => formatDuration(t.durationSeconds) },
    { header: "Status", cell: (t) => <TransactionStatusBadge status={t.status} /> },
    { header: "Time", cell: (t) => <span className="text-muted-foreground">{formatDate(t.createdAt)}</span> },
    {
      header: "", className: "w-12 text-right",
      cell: (t) => <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => setDetail(t)}><Eye className="h-4 w-4" /></Button>,
    },
  ];

  return (
    <div>
      <PageHeader title="Transactions" description="Browse and filter all battery swapping transactions." />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        page={page}
        onPageChange={setPage}
        search={search}
        onSearchChange={(v) => { setSearch(v); setPage(1); }}
        searchPlaceholder="Search by user or cabinet…"
        rowKey={(t) => t.id}
        onRowClick={(t) => setDetail(t)}
        toolbar={
          <>
            <Input type="date" className="w-40" value={fromDate} onChange={(e) => { setFromDate(e.target.value); setPage(1); }} />
            <Input type="date" className="w-40" value={toDate} onChange={(e) => { setToDate(e.target.value); setPage(1); }} />
            <Select value={status} onValueChange={(v) => { setStatus(v); setPage(1); }}>
              <SelectTrigger className="w-44"><SelectValue placeholder="Status" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                <SelectItem value="Success">Success</SelectItem>
                <SelectItem value="FailedSlotOut">Failed (Slot Out)</SelectItem>
              </SelectContent>
            </Select>
          </>
        }
      />

      <Dialog open={!!detail} onOpenChange={(o) => !o && setDetail(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Transaction #{detail?.id}</DialogTitle>
            <DialogDescription>Full swap transaction details.</DialogDescription>
          </DialogHeader>
          {detail && (
            <div className="mt-2">
              <DetailRow label="User" value={detail.userFullName} />
              <DetailRow label="Cabinet" value={`${detail.cabinetModel} #${detail.cabinetId}`} />
              <DetailRow label="Status" value={<TransactionStatusBadge status={detail.status} />} />
              <DetailRow label="Battery Returned" value={detail.batteryInId ? `#${detail.batteryInId}` : "—"} />
              <DetailRow label="Returned At" value={formatDate(detail.returnedAt)} />
              <DetailRow label="Slot In" value={detail.slotInId ? `#${detail.slotInId}` : "—"} />
              <DetailRow label="Battery Dispensed" value={detail.batteryOutId ? `#${detail.batteryOutId}` : "—"} />
              <DetailRow label="Dispensed At" value={formatDate(detail.dispensedAt)} />
              <DetailRow label="Slot Out" value={detail.slotOutId ? `#${detail.slotOutId}` : "—"} />
              <DetailRow label="Duration" value={formatDuration(detail.durationSeconds)} />
              <DetailRow label="Created" value={formatDate(detail.createdAt)} />
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
