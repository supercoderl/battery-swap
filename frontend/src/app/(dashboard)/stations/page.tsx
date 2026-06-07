"use client";
import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Plus, Loader2, MapPin } from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { DataTable, type Column } from "@/components/shared/data-table";
import { RowActions } from "@/components/shared/row-actions";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from "@/components/ui/dialog";
import { stationHooks } from "@/hooks/use-resources";
import { useDebounce } from "@/hooks/use-debounce";
import { formatDate } from "@/lib/utils";
import type { Station } from "@/types";

const schema = z.object({
  address: z.string().min(1, "Address is required").max(300),
  latitude: z.number().min(-90).max(90),
  longitude: z.number().min(-180).max(180),
});
type FormValues = z.infer<typeof schema>;

export default function StationsPage() {
  const [page, setPage] = React.useState(1);
  const [search, setSearch] = React.useState("");
  const debouncedSearch = useDebounce(search);

  const { data, isLoading } = stationHooks.useList({ page, pageSize: 10, search: debouncedSearch });
  const create = stationHooks.useCreate();
  const update = stationHooks.useUpdate();
  const remove = stationHooks.useDelete();

  const [formOpen, setFormOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<Station | null>(null);
  const [deleting, setDeleting] = React.useState<Station | null>(null);

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { address: "", latitude: 0, longitude: 0 },
  });

  function openCreate() {
    setEditing(null);
    form.reset({ address: "", latitude: 10.78, longitude: 106.7 });
    setFormOpen(true);
  }
  function openEdit(s: Station) {
    setEditing(s);
    form.reset({ address: s.address, latitude: s.latitude, longitude: s.longitude });
    setFormOpen(true);
  }

  function onSubmit(values: FormValues) {
    if (editing) {
      update.mutate({ id: editing.id, dto: values }, { onSuccess: () => setFormOpen(false) });
    } else {
      create.mutate(values, { onSuccess: () => setFormOpen(false) });
    }
  }

  const columns: Column<Station>[] = [
    { header: "ID", cell: (s) => <span className="text-muted-foreground">#{s.id}</span>, className: "w-16" },
    {
      header: "Address",
      cell: (s) => (
        <div className="flex items-center gap-2 font-medium">
          <MapPin className="h-4 w-4 text-primary" /> {s.address}
        </div>
      ),
    },
    { header: "Coordinates", cell: (s) => <span className="text-muted-foreground">{s.latitude.toFixed(4)}, {s.longitude.toFixed(4)}</span> },
    { header: "Cabinets", cell: (s) => s.cabinetCount },
    { header: "Created", cell: (s) => <span className="text-muted-foreground">{formatDate(s.createdAt, false)}</span> },
    {
      header: "",
      className: "w-12 text-right",
      cell: (s) => <RowActions onEdit={() => openEdit(s)} onDelete={() => setDeleting(s)} />,
    },
  ];

  const submitting = create.isPending || update.isPending;

  return (
    <div>
      <PageHeader
        title="Stations"
        description="Manage battery swapping stations and their locations."
        actions={<Button onClick={openCreate}><Plus className="h-4 w-4" /> Add Station</Button>}
      />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        page={page}
        onPageChange={setPage}
        search={search}
        onSearchChange={(v) => { setSearch(v); setPage(1); }}
        searchPlaceholder="Search by address…"
        rowKey={(s) => s.id}
      />

      {/* Create / Edit dialog */}
      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Station" : "Add Station"}</DialogTitle>
          </DialogHeader>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="address">Address</Label>
              <Input id="address" {...form.register("address")} placeholder="12 Nguyen Hue, District 1…" />
              {form.formState.errors.address && <p className="text-xs text-destructive">{form.formState.errors.address.message}</p>}
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="latitude">Latitude</Label>
                <Input id="latitude" type="number" step="any" {...form.register("latitude", { valueAsNumber: true })} />
                {form.formState.errors.latitude && <p className="text-xs text-destructive">{form.formState.errors.latitude.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="longitude">Longitude</Label>
                <Input id="longitude" type="number" step="any" {...form.register("longitude", { valueAsNumber: true })} />
                {form.formState.errors.longitude && <p className="text-xs text-destructive">{form.formState.errors.longitude.message}</p>}
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
        title="Delete station?"
        description={`This will permanently remove "${deleting?.address}". Stations with cabinets cannot be deleted.`}
        confirmLabel="Delete"
        destructive
        loading={remove.isPending}
        onConfirm={() => deleting && remove.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
      />
    </div>
  );
}
