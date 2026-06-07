"use client";
import * as React from "react";
import { useRouter } from "next/navigation";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Plus, Loader2, BatteryFull } from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { DataTable, type Column } from "@/components/shared/data-table";
import { RowActions } from "@/components/shared/row-actions";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { HealthBadge, LocationBadge, SocBadge } from "@/components/shared/status-badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { batteryHooks } from "@/hooks/use-resources";
import { useDebounce } from "@/hooks/use-debounce";
import { formatDate } from "@/lib/utils";
import type { Battery } from "@/types";

const schema = z.object({
  soc: z.number().min(0).max(100),
  temperature: z.number().min(-40).max(150),
  voltage: z.number().min(0).max(100),
  healthState: z.enum(["Good", "Degraded"]),
  locationType: z.enum(["InSlot", "RentedByUser"]),
});
type FormValues = z.infer<typeof schema>;

export default function BatteriesPage() {
  const router = useRouter();
  const [page, setPage] = React.useState(1);
  const [search, setSearch] = React.useState("");
  const [health, setHealth] = React.useState("all");
  const [location, setLocation] = React.useState("all");
  const debouncedSearch = useDebounce(search);

  const { data, isLoading } = batteryHooks.useList({
    page, pageSize: 10, search: debouncedSearch,
    healthState: health === "all" ? undefined : health,
    locationType: location === "all" ? undefined : location,
  });
  const create = batteryHooks.useCreate();
  const update = batteryHooks.useUpdate();
  const remove = batteryHooks.useDelete();

  const [formOpen, setFormOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<Battery | null>(null);
  const [deleting, setDeleting] = React.useState<Battery | null>(null);

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { soc: 100, temperature: 28, voltage: 50, healthState: "Good", locationType: "InSlot" },
  });

  function openCreate() {
    setEditing(null);
    form.reset({ soc: 100, temperature: 28, voltage: 50, healthState: "Good", locationType: "InSlot" });
    setFormOpen(true);
  }
  function openEdit(b: Battery) {
    setEditing(b);
    form.reset({ soc: b.soc, temperature: b.temperature, voltage: b.voltage, healthState: b.healthState, locationType: b.locationType });
    setFormOpen(true);
  }
  function onSubmit(values: FormValues) {
    if (editing) update.mutate({ id: editing.id, dto: values }, { onSuccess: () => setFormOpen(false) });
    else create.mutate(values, { onSuccess: () => setFormOpen(false) });
  }

  const columns: Column<Battery>[] = [
    { header: "ID", cell: (b) => <div className="flex items-center gap-2 font-medium"><BatteryFull className="h-4 w-4 text-primary" />#{b.id}</div>, className: "w-24" },
    { header: "SOC", cell: (b) => <SocBadge soc={b.soc} /> },
    { header: "Temperature", cell: (b) => `${b.temperature.toFixed(1)} °C` },
    { header: "Voltage", cell: (b) => `${b.voltage.toFixed(2)} V` },
    { header: "Health", cell: (b) => <HealthBadge state={b.healthState} /> },
    { header: "Location", cell: (b) => <LocationBadge type={b.locationType} /> },
    { header: "Updated", cell: (b) => <span className="text-muted-foreground">{formatDate(b.updatedAt)}</span> },
    {
      header: "", className: "w-12 text-right",
      cell: (b) => <RowActions onView={() => router.push(`/batteries/${b.id}`)} onEdit={() => openEdit(b)} onDelete={() => setDeleting(b)} />,
    },
  ];

  const submitting = create.isPending || update.isPending;

  return (
    <div>
      <PageHeader
        title="Batteries"
        description="Manage battery inventory and inspect telemetry."
        actions={<Button onClick={openCreate}><Plus className="h-4 w-4" /> Add Battery</Button>}
      />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        page={page}
        onPageChange={setPage}
        search={search}
        onSearchChange={(v) => { setSearch(v); setPage(1); }}
        searchPlaceholder="Search by battery ID…"
        rowKey={(b) => b.id}
        onRowClick={(b) => router.push(`/batteries/${b.id}`)}
        toolbar={
          <>
            <Select value={health} onValueChange={(v) => { setHealth(v); setPage(1); }}>
              <SelectTrigger className="w-36"><SelectValue placeholder="Health" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All health</SelectItem>
                <SelectItem value="Good">Good</SelectItem>
                <SelectItem value="Degraded">Degraded</SelectItem>
              </SelectContent>
            </Select>
            <Select value={location} onValueChange={(v) => { setLocation(v); setPage(1); }}>
              <SelectTrigger className="w-36"><SelectValue placeholder="Location" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All locations</SelectItem>
                <SelectItem value="InSlot">In Slot</SelectItem>
                <SelectItem value="RentedByUser">Rented</SelectItem>
              </SelectContent>
            </Select>
          </>
        }
      />

      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent>
          <DialogHeader><DialogTitle>{editing ? "Edit Battery" : "Add Battery"}</DialogTitle></DialogHeader>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="soc">SOC (%)</Label>
                <Input id="soc" type="number" {...form.register("soc", { valueAsNumber: true })} />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="temperature">Temp (°C)</Label>
                <Input id="temperature" type="number" step="any" {...form.register("temperature", { valueAsNumber: true })} />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="voltage">Voltage (V)</Label>
                <Input id="voltage" type="number" step="any" {...form.register("voltage", { valueAsNumber: true })} />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Health State</Label>
                <Controller control={form.control} name="healthState" render={({ field }) => (
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Good">Good</SelectItem>
                      <SelectItem value="Degraded">Degraded</SelectItem>
                    </SelectContent>
                  </Select>
                )} />
              </div>
              <div className="space-y-1.5">
                <Label>Location Type</Label>
                <Controller control={form.control} name="locationType" render={({ field }) => (
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="InSlot">In Slot</SelectItem>
                      <SelectItem value="RentedByUser">Rented By User</SelectItem>
                    </SelectContent>
                  </Select>
                )} />
              </div>
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
        title="Delete battery?"
        description={`This will permanently remove battery #${deleting?.id} and its telemetry history.`}
        confirmLabel="Delete"
        destructive
        loading={remove.isPending}
        onConfirm={() => deleting && remove.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
      />
    </div>
  );
}
