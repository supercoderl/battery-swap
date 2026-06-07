"use client";
import { Loader2 } from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { BarChartCard, LineChartCard, PieChartCard } from "@/components/charts/charts";
import { useReports } from "@/hooks/use-resources";

export default function ReportsPage() {
  const { monthly, daily, utilization, health, cabinet } = useReports();
  const loading = monthly.isLoading || daily.isLoading || utilization.isLoading || health.isLoading || cabinet.isLoading;

  if (loading) {
    return <div className="flex h-96 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div>;
  }

  return (
    <div>
      <PageHeader title="Reports" description="Operational analytics across swaps, batteries and cabinets." />

      <div className="grid gap-4 lg:grid-cols-2">
        <LineChartCard title="Daily Swaps" description="Last 30 days" data={daily.data ?? []} />
        <BarChartCard title="Monthly Swaps" description="Current year" data={monthly.data ?? []} />
        <PieChartCard title="Battery Utilization" description="In slot vs rented" data={utilization.data ?? []} />
        <PieChartCard title="Battery Health Report" description="Good vs degraded" data={health.data ?? []} />
        <BarChartCard className="lg:col-span-2" title="Cabinet Utilization Report" description="Slot occupancy percentage per cabinet" data={cabinet.data ?? []} height={320} />
      </div>
    </div>
  );
}
