"use client";
import * as React from "react";
import { useParams, useRouter } from "next/navigation";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import {
  ArrowLeft, Plus, Loader2, Server, Grid3x3, BatteryFull,
  Lock, Unlock, MapPin,
} from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { StatCard } from "@/components/shared/stat-card";
import { DataTable, type Column } from "@/components/shared/data-table";
import { RowActions } from "@/components/shared/row-actions";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { CabinetStatusBadge, SocBadge } from "@/components/shared/status-badge";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/misc";
import { cabinetHooks, slotHooks } from "@/hooks/use-resources";
import type { Slot } from "@/types";

const slotSchema = z.object({
  slotNumber: z.number().min(1, "Slot number is required"),
  isHardwareLocked: z.boolean(),
});
type SlotFormValues = z.infer<typeof slotSchema>;

export default function CabinetDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const id = Number(params.id);

  const { data: cabinet, isLoading } = cabinetHooks.useDetail(id);
  const { data: slotsData, isLoading: slotsLoading } = slotHooks.useList({ page: 1, pageSize: 100, cabinetId: id });
  const createSlot = slotHooks.useCreate();
  const updateSlot = slotHooks.useUpdate();
  const removeSlot = slotHooks.useDelete();

  const [formOpen, setFormOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<Slot | null>(null);
  const [deleting, setDeleting] = React.useState<Slot | null>(null);

  const form = useForm<SlotFormValues>({
    resolver: zodResolver(slotSchema),
    defaultValues: { slotNumber: 1, isHardwareLocked: false },
  });

  function openCreate() {
    setEditing(null);
    const nextNumber = (slotsData?.items.length ?? 0) + 1;
    form.reset({ slotNumber: nextNumber, isHardwareLocked: false });
    setFormOpen(true);
  }
  function openEdit(s: Slot) {
    setEditing(s);
    form.reset({ slotNumber: s.slotNumber, isHardwareLocked: s.isHardwareLocked });
    setFormOpen(true);
  }
  function onSubmit(values: SlotFormValues) {
    if (editing) {
      updateSlot.mutate(
        { id: editing.id, dto: { ...values, currentBatteryId: editing.currentBatteryId } },
        { onSuccess: () => setFormOpen(false) }
      );
    } else {
      createSlot.mutate({ cabinetId: id, ...values }, { onSuccess: () => setFormOpen(false) });
    }
  }

  const batteriesInCabinet = React.useMemo(
    () => (slotsData?.items ?? []).filter((s) => s.currentBatteryId != null),
    [slotsData]
  );

  const slotColumns: Column<Slot>[] = [
    { header: "ID", cell: (s) => <span className="text-muted-foreground">#{s.id}</span>, className: "w-16" },
    { header: "Slot", cell: (s) => <div className="flex items-center gap-2 font-medium"><Grid3x3 className="h-4 w-4 text-primary" /> #{s.slotNumber}</div> },
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

  const batteryColumnsDef: Column<Slot>[] = [
    { header: "Battery ID", cell: (s) => <div className="flex items-center gap-2 font-medium"><BatteryFull className="h-4 w-4 text-primary" />#{s.currentBatteryId}</div> },
    { header: "Slot", cell: (s) => <span className="text-muted-foreground">#{s.slotNumber}</span> },
    { header: "SOC", cell: (s) => s.batterySoc != null ? <SocBadge soc={s.batterySoc} /> : <span className="text-muted-foreground">—</span> },
    { header: "", className: "w-12 text-right", cell: (s) => <RowActions onView={() => router.push(`/batteries/${s.currentBatteryId}`)} /> },
  ];

  const submitting = createSlot.isPending || updateSlot.isPending;

  if (isLoading || !cabinet) {
    return <div className="flex h-96 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div>;
  }

  return (
    <div>
      <Button variant="ghost" size="sm" className="mb-3 -ml-2" onClick={() => router.push("/cabinets")}>
        <ArrowLeft className="h-4 w-4" /> Back to cabinets
      </Button>

      <PageHeader
        title={cabinet.cabinetModel}
        description={`Cabinet #${cabinet.id} · ${cabinet.stationAddress}`}
        actions={<CabinetStatusBadge status={cabinet.status} />}
      />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Status" value={cabinet.status} icon={Server} accent={cabinet.status === "Online" ? "success" : cabinet.status === "Maintenance" ? "warning" : "muted"} />
        <StatCard label="Station" value={cabinet.stationAddress} icon={MapPin} accent="primary" />
        <StatCard label="Total Slots" value={cabinet.slotCount} icon={Grid3x3} accent="primary" />
        <StatCard label="Occupied Slots" value={`${cabinet.occupiedSlots}/${cabinet.slotCount}`} icon={BatteryFull} accent="warning" />
      </div>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle>Slots &amp; Batteries</CardTitle>
          <CardDescription>Manage slots in this cabinet and view the batteries currently docked.</CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="slots">
            <TabsList>
              <TabsTrigger value="slots">Slots ({slotsData?.items.length ?? 0})</TabsTrigger>
              <TabsTrigger value="batteries">Batteries ({batteriesInCabinet.length})</TabsTrigger>
            </TabsList>

            <TabsContent value="slots">
              <div className="mb-3 flex justify-end">
                <Button size="sm" onClick={openCreate}><Plus className="h-4 w-4" /> Add Slot</Button>
              </div>
              <DataTable
                data={slotsData}
                columns={slotColumns}
                isLoading={slotsLoading}
                page={1}
                onPageChange={() => {}}
                rowKey={(s) => s.id}
              />
            </TabsContent>

            <TabsContent value="batteries">
              <DataTable
                data={slotsData ? { ...slotsData, items: batteriesInCabinet } : undefined}
                columns={batteryColumnsDef}
                isLoading={slotsLoading}
                page={1}
                onPageChange={() => {}}
                rowKey={(s) => s.id}
                onRowClick={(s) => router.push(`/batteries/${s.currentBatteryId}`)}
              />
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent>
          <DialogHeader><DialogTitle>{editing ? "Edit Slot" : "Add Slot"}</DialogTitle></DialogHeader>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
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
        loading={removeSlot.isPending}
        onConfirm={() => deleting && removeSlot.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
      />
    </div>
  );
}
