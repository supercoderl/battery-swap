"use client";
import * as React from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Plus, Loader2, Server } from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { DataTable, type Column } from "@/components/shared/data-table";
import { RowActions } from "@/components/shared/row-actions";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { CabinetStatusBadge } from "@/components/shared/status-badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { cabinetHooks, stationHooks } from "@/hooks/use-resources";
import { useDebounce } from "@/hooks/use-debounce";
import { formatDate } from "@/lib/utils";
import type { Cabinet, CabinetStatus } from "@/types";

const STATUSES: CabinetStatus[] = ["Online", "Offline", "Maintenance"];

const schema = z.object({
  stationId: z.number().min(1, "Station is required"),
  cabinetModel: z.string().min(1, "Model is required").max(100),
  status: z.enum(["Online", "Offline", "Maintenance"]),
});
type FormValues = z.infer<typeof schema>;

export default function CabinetsPage() {
  const [page, setPage] = React.useState(1);
  const [search, setSearch] = React.useState("");
  const [statusFilter, setStatusFilter] = React.useState<string>("all");
  const debouncedSearch = useDebounce(search);

  const { data, isLoading } = cabinetHooks.useList({
    page, pageSize: 10, search: debouncedSearch,
    status: statusFilter === "all" ? undefined : statusFilter,
  });
  const stationsList = stationHooks.useList({ page: 1, pageSize: 100 });
  const create = cabinetHooks.useCreate();
  const update = cabinetHooks.useUpdate();
  const remove = cabinetHooks.useDelete();

  const [formOpen, setFormOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<Cabinet | null>(null);
  const [deleting, setDeleting] = React.useState<Cabinet | null>(null);

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { stationId: 0, cabinetModel: "", status: "Offline" },
  });

  function openCreate() {
    setEditing(null);
    form.reset({ stationId: 0, cabinetModel: "VF-SWAP-X1", status: "Offline" });
    setFormOpen(true);
  }
  function openEdit(c: Cabinet) {
    setEditing(c);
    form.reset({ stationId: c.stationId, cabinetModel: c.cabinetModel, status: c.status });
    setFormOpen(true);
  }
  function onSubmit(values: FormValues) {
    const action = editing
      ? update.mutate({ id: editing.id, dto: values }, { onSuccess: () => setFormOpen(false) })
      : create.mutate(values, { onSuccess: () => setFormOpen(false) });
    return action;
  }

  const columns: Column<Cabinet>[] = [
    { header: "ID", cell: (c) => <span className="text-muted-foreground">#{c.id}</span>, className: "w-16" },
    {
      header: "Model",
      cell: (c) => <div className="flex items-center gap-2 font-medium"><Server className="h-4 w-4 text-primary" /> {c.cabinetModel}</div>,
    },
    { header: "Station", cell: (c) => <span className="text-muted-foreground">{c.stationAddress}</span> },
    { header: "Status", cell: (c) => <CabinetStatusBadge status={c.status} /> },
    { header: "Slots", cell: (c) => <span>{c.occupiedSlots}/{c.slotCount} occupied</span> },
    { header: "Created", cell: (c) => <span className="text-muted-foreground">{formatDate(c.createdAt, false)}</span> },
    { header: "", className: "w-12 text-right", cell: (c) => <RowActions onEdit={() => openEdit(c)} onDelete={() => setDeleting(c)} /> },
  ];

  const submitting = create.isPending || update.isPending;

  return (
    <div>
      <PageHeader
        title="Cabinets"
        description="Monitor and manage swapping cabinets across all stations."
        actions={<Button onClick={openCreate}><Plus className="h-4 w-4" /> Add Cabinet</Button>}
      />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        page={page}
        onPageChange={setPage}
        search={search}
        onSearchChange={(v) => { setSearch(v); setPage(1); }}
        searchPlaceholder="Search by model…"
        rowKey={(c) => c.id}
        toolbar={
          <Select value={statusFilter} onValueChange={(v) => { setStatusFilter(v); setPage(1); }}>
            <SelectTrigger className="w-40"><SelectValue placeholder="Status" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All statuses</SelectItem>
              {STATUSES.map((s) => <SelectItem key={s} value={s}>{s}</SelectItem>)}
            </SelectContent>
          </Select>
        }
      />

      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent>
          <DialogHeader><DialogTitle>{editing ? "Edit Cabinet" : "Add Cabinet"}</DialogTitle></DialogHeader>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1.5">
              <Label>Station</Label>
              <Controller
                control={form.control}
                name="stationId"
                render={({ field }) => (
                  <Select value={field.value ? String(field.value) : ""} onValueChange={(v) => field.onChange(Number(v))}>
                    <SelectTrigger><SelectValue placeholder="Select a station" /></SelectTrigger>
                    <SelectContent>
                      {stationsList.data?.items.map((s) => (
                        <SelectItem key={s.id} value={String(s.id)}>{s.address}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
              {form.formState.errors.stationId && <p className="text-xs text-destructive">{form.formState.errors.stationId.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="cabinetModel">Cabinet Model</Label>
              <Input id="cabinetModel" {...form.register("cabinetModel")} placeholder="VF-SWAP-X1" />
              {form.formState.errors.cabinetModel && <p className="text-xs text-destructive">{form.formState.errors.cabinetModel.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label>Status</Label>
              <Controller
                control={form.control}
                name="status"
                render={({ field }) => (
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      {STATUSES.map((s) => <SelectItem key={s} value={s}>{s}</SelectItem>)}
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
            <DialogFooter className="gap-2">
              <Button type="button" variant="outline" onClick={() => setFormOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={submitting}>
                {submitting && <Loader2 className="h-4 w-4 animate-spin" />}
                {editing ? "Save changes" : "Create"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <ConfirmDialog
        open={!!deleting}
        onOpenChange={(o) => !o && setDeleting(null)}
        title="Delete cabinet?"
        description={`This will permanently remove cabinet "${deleting?.cabinetModel}". Cabinets with slots cannot be deleted.`}
        confirmLabel="Delete"
        destructive
        loading={remove.isPending}
        onConfirm={() => deleting && remove.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
      />
    </div>
  );
}
