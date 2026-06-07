"use client";
import * as React from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Plus, Loader2 } from "lucide-react";
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
import { userHooks } from "@/hooks/use-resources";
import { useDebounce } from "@/hooks/use-debounce";
import type { User } from "@/types";

const schema = z.object({
  fullName: z.string().min(1, "Full name is required").max(150),
  phone: z.string().min(6, "Invalid phone").max(20),
  balanceTrips: z.number().min(0),
  isActive: z.boolean(),
});
type FormValues = z.infer<typeof schema>;

export default function UsersPage() {
  const [page, setPage] = React.useState(1);
  const [search, setSearch] = React.useState("");
  const [activeFilter, setActiveFilter] = React.useState<string>("all");
  const debouncedSearch = useDebounce(search);

  const { data, isLoading } = userHooks.useList({
    page, pageSize: 10, search: debouncedSearch,
    isActive: activeFilter === "all" ? undefined : activeFilter === "active",
  });
  const create = userHooks.useCreate();
  const update = userHooks.useUpdate();
  const remove = userHooks.useDelete();

  const [formOpen, setFormOpen] = React.useState(false);
  const [editing, setEditing] = React.useState<User | null>(null);
  const [deleting, setDeleting] = React.useState<User | null>(null);

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { fullName: "", phone: "", balanceTrips: 0, isActive: true },
  });

  function openCreate() {
    setEditing(null);
    form.reset({ fullName: "", phone: "", balanceTrips: 0, isActive: true });
    setFormOpen(true);
  }
  function openEdit(u: User) {
    setEditing(u);
    form.reset({ fullName: u.fullName, phone: u.phone, balanceTrips: u.balanceTrips, isActive: u.isActive });
    setFormOpen(true);
  }
  function onSubmit(values: FormValues) {
    if (editing) update.mutate({ id: editing.id, dto: values }, { onSuccess: () => setFormOpen(false) });
    else create.mutate(values, { onSuccess: () => setFormOpen(false) });
  }

  const columns: Column<User>[] = [
    { header: "ID", cell: (u) => <span className="text-muted-foreground">#{u.id}</span>, className: "w-16" },
    { header: "Full Name", cell: (u) => <span className="font-medium">{u.fullName}</span> },
    { header: "Phone", cell: (u) => u.phone },
    { header: "Balance Trips", cell: (u) => <Badge variant={u.balanceTrips > 0 ? "default" : "secondary"}>{u.balanceTrips}</Badge> },
    { header: "Transactions", cell: (u) => u.transactionCount },
    { header: "Status", cell: (u) => <Badge variant={u.isActive ? "success" : "secondary"}>{u.isActive ? "Active" : "Inactive"}</Badge> },
    { header: "", className: "w-12 text-right", cell: (u) => <RowActions onEdit={() => openEdit(u)} onDelete={() => setDeleting(u)} /> },
  ];

  const submitting = create.isPending || update.isPending;

  return (
    <div>
      <PageHeader
        title="Users"
        description="Manage EV drivers and their trip balances."
        actions={<Button onClick={openCreate}><Plus className="h-4 w-4" /> Add User</Button>}
      />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        page={page}
        onPageChange={setPage}
        search={search}
        onSearchChange={(v) => { setSearch(v); setPage(1); }}
        searchPlaceholder="Search by name or phone…"
        rowKey={(u) => u.id}
        toolbar={
          <Select value={activeFilter} onValueChange={(v) => { setActiveFilter(v); setPage(1); }}>
            <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All users</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="inactive">Inactive</SelectItem>
            </SelectContent>
          </Select>
        }
      />

      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent>
          <DialogHeader><DialogTitle>{editing ? "Edit User" : "Add User"}</DialogTitle></DialogHeader>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="fullName">Full Name</Label>
              <Input id="fullName" {...form.register("fullName")} placeholder="Nguyen Van An" />
              {form.formState.errors.fullName && <p className="text-xs text-destructive">{form.formState.errors.fullName.message}</p>}
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="phone">Phone</Label>
                <Input id="phone" {...form.register("phone")} placeholder="0912345678" />
                {form.formState.errors.phone && <p className="text-xs text-destructive">{form.formState.errors.phone.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="balanceTrips">Balance Trips</Label>
                <Input id="balanceTrips" type="number" {...form.register("balanceTrips", { valueAsNumber: true })} />
                {form.formState.errors.balanceTrips && <p className="text-xs text-destructive">{form.formState.errors.balanceTrips.message}</p>}
              </div>
            </div>
            <div className="flex items-center justify-between rounded-lg border p-3">
              <div>
                <Label>Active</Label>
                <p className="text-xs text-muted-foreground">Inactive users cannot perform swaps.</p>
              </div>
              <Controller control={form.control} name="isActive" render={({ field }) => (
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
        title="Delete user?"
        description={`This will permanently remove "${deleting?.fullName}".`}
        confirmLabel="Delete"
        destructive
        loading={remove.isPending}
        onConfirm={() => deleting && remove.mutate(deleting.id, { onSuccess: () => setDeleting(null) })}
      />
    </div>
  );
}
