"use client";
import * as React from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Plus, Loader2, Grid3x3, Lock, Unlock } from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { DataTable, type Column } from "@/components/shared/data-table";
import { RowActions } from "@/components/shared/row-actions";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { SocBadge } from "@/components/shared/status-badge";
import { slotHooks, cabinetHooks } from "@/hooks/use-resources";
import type { Slot } from "@/types";

const createSchema = z.object({
  cabinetId: z.number().min(1, "Cabinet is required"),
  slotNumber: z.number().min(1, "Slot number is required"),
  isHardwareLocked: z.boolean(),
});
type CreateValues = z.infer<typeof createSchema>;

export default function SlotsPage() {
  const [page, setPage] = React.useState(1);
  const [cabinetFilter, setCabinetFilter] = React.useState("all");

  const { data, isLoading } = slotHooks.useList({
    page, pageSize: 10,
    cabinetId: cabinetFilter === "all" ? undefined : Number(cabinetFilter),
  });
  const cabinetsList = cabinetHooks.useList({ page: 1, pageSize: 100 });
  const create = slotHooks.useCreate();
  const update = slotHooks.useUpdate();
  const remove = slotHooks.useDelete();

  const [formOpen, setFormOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<Slot | null>(null);
  const [deleting, setDeleting] = React.useState<Slot | null>(null);

  const form = useForm<CreateValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { cabinetId: 0, slotNumber: 1, isHardwareLocked: false },
  });

  function openCreate() {
    setEditing(null);
    form.reset({ cabinetId: 0, slotNumber: 1, isHardwareLocked: false });
    setFormOpen(true);
  }
  function openEdit(s: Slot) {
    setEditing(s);
    form.reset({ cabinetId: s.cabinetId, slotNumber: s.slotNumber, isHardwareLocked: s.isHardwareLocked });
    setFormOpen(true);
  }
  function onSubmit(values: CreateValues) {
    if (editing) {
      update.mutate(
        { id: editing.id, dto: { slotNumber: values.slotNumber, isHardwareLocked: values.isHardwareLocked, currentBatteryId: editing.currentBatteryId } },
        { onSuccess: () => setFormOpen(false) }
      );
    } else {
      create.mutate(
        { cabinetId: values.cabinetId, slotNumber: values.slotNumber, isHardwareLocked: values.isHardwareLocked },
        { onSuccess: () => setFormOpen(false) }
      );
    }
  }

  const columns: Column<Slot>[] = [
    { header: "ID", cell: (s) => <span className="text-muted-foreground">#{s.id}</span>, className: "w-16" },
    { header: "Slot", cell: (s) => <div className="flex items-center gap-2 font-medium"><Grid3x3 className="h-4 w-4 text-primary" /> #{s.slotNumber}</div> },
    { header: "Cabinet", cell: (s) => <span className="text-muted-foreground">{s.cabinetModel}</span> },
    {
      header: "Battery",
      cell: (s) => s.currentBatteryId
        ? <span className="flex items-center gap-2">#{s.currentBatteryId} {s.batterySoc != null && <SocBadge soc={s.batterySoc} />}</span>
        : <Badge variant="secondary">Empty</Badge>,
    },
    {
      header: "Hardware Lock",
      cell: (s) => s.isHardwareLocked
        ? <Badge variant="destructive"><Lock className="h-3 w-3" /> Locked</Badge>
        : <Badge variant="success"><Unlock className="h-3 w-3" /> Unlocked</Badge>,
    },
    { header: "", className: "w-12 text-right", cell: (s) => <RowActions onEdit={() => openEdit(s)} onDelete={() => setDeleting(s)} /> },
  ];

  const submitting = create.isPending || update.isPending;

  return (
    <div>
      <PageHeader
        title="Slots"
        description="Manage cabinet slots and hardware lock state."
        actions={<Button onClick={openCreate}><Plus className="h-4 w-4" /> Add Slot</Button>}
      />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        page={page}
        onPageChange={setPage}
        rowKey={(s) => s.id}
        toolbar={
          <Select value={cabinetFilter} onValueChange={(v) => { setCabinetFilter(v); setPage(1); }}>
            <SelectTrigger className="w-56"><SelectValue placeholder="Filter by cabinet" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All cabinets</SelectItem>
              {cabinetsList.data?.items.map((c) => (
                <SelectItem key={c.id} value={String(c.id)}>{c.cabinetModel} #{c.id}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        }
      />

      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent>
          <DialogHeader><DialogTitle>{editing ? "Edit Slot" : "Add Slot"}</DialogTitle></DialogHeader>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {!editing && (
              <div className="space-y-1.5">
                <Label>Cabinet</Label>
                <Controller control={form.control} name="cabinetId" render={({ field }) => (
                  <Select value={field.value ? String(field.value) : ""} onValueChange={(v) => field.onChange(Number(v))}>
                    <SelectTrigger><SelectValue placeholder="Select a cabinet" /></SelectTrigger>
                    <SelectContent>
                      {cabinetsList.data?.items.map((c) => (
                        <SelectItem key={c.id} value={String(c.id)}>{c.cabinetModel} #{c.id} — {c.stationAddress}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )} />
                {form.formState.errors.cabinetId && <p className="text-xs text-destructive">{form.formState.errors.cabinetId.message}</p>}
              </div>
            )}
            <div className="space-y-1.5">
              <Label htmlFor="slotNumber">Slot Number</Label>
              <Input id="slotNumber" type="number" {...form.register("slotNumber", { valueAsNumber: true })} />
              {form.formState.errors.slotNumber && <p className="text-xs text-destructive">{form.formState.errors.slotNumber.message}</p>}
            </div>
            <div className="flex items-center justify-between rounded-lg border p-3">
              <div><Label>Hardware Locked</Label><p className="text-xs text-muted-foreground">Locked slots cannot dispense batteries.</p></div>
              <Controller control={form.control} name="isHardwareLocked" render={({ field }) => (
                <Switch checked={field.value} onCheckedChange={field.onChange} />
              )} />
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
        title="Delete slot?"
        description={`This will permanently remove slot #${deleting?.slotNumber}.`}
        confirmLabel="Delete"
        destructive
        loading={remove.isPending}
        onConfirm={() => deleting && remove.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
      />
    </div>
  );
}
