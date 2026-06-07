"use client";
import * as React from "react";
import { Activity, XCircle } from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { DataTable, type Column } from "@/components/shared/data-table";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { SessionStatusBadge } from "@/components/shared/status-badge";
import { Button } from "@/components/ui/button";
import { useSessions, useForceCloseSession } from "@/hooks/use-resources";
import { formatDate } from "@/lib/utils";
import type { ActiveSession } from "@/types";

export default function SessionsPage() {
  const [page, setPage] = React.useState(1);
  const { data, isLoading } = useSessions({ page, pageSize: 10 });
  const forceClose = useForceCloseSession();
  const [closing, setClosing] = React.useState<ActiveSession | null>(null);

  const columns: Column<ActiveSession>[] = [
    { header: "ID", cell: (s) => <span className="text-muted-foreground">#{s.id}</span>, className: "w-16" },
    { header: "User", cell: (s) => <div className="flex items-center gap-2 font-medium"><Activity className="h-4 w-4 text-primary" /> {s.userFullName}</div> },
    { header: "Cabinet", cell: (s) => <span className="text-muted-foreground">{s.cabinetModel} #{s.cabinetId}</span> },
    { header: "Status", cell: (s) => <SessionStatusBadge status={s.status} /> },
    { header: "Started", cell: (s) => <span className="text-muted-foreground">{formatDate(s.createdAt)}</span> },
    {
      header: "", className: "w-32 text-right",
      cell: (s) => (
        <Button variant="outline" size="sm" className="text-destructive" onClick={() => setClosing(s)}>
          <XCircle className="h-4 w-4" /> Force close
        </Button>
      ),
    },
  ];

  return (
    <div>
      <PageHeader title="Active Sessions" description="Live swapping sessions. Auto-refreshes every 10 seconds." />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        page={page}
        onPageChange={setPage}
        rowKey={(s) => s.id}
      />

      <ConfirmDialog
        open={!!closing}
        onOpenChange={(o) => !o && setClosing(null)}
        title="Force close session?"
        description={`This will terminate the active session for "${closing?.userFullName}". Use only when a session is stuck.`}
        confirmLabel="Force close"
        destructive
        loading={forceClose.isPending}
        onConfirm={() => closing && forceClose.mutate(closing.id, { onSuccess: () => setClosing(null) })}
      />
    </div>
  );
}
