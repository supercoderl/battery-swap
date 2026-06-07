"use client";
import * as React from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, BatteryFull, Gauge, Thermometer, Zap, Loader2 } from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { StatCard } from "@/components/shared/stat-card";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card";
import { HealthBadge, LocationBadge } from "@/components/shared/status-badge";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/misc";
import { MultiLineChart } from "@/components/charts/charts";
import { batteryHooks } from "@/hooks/use-resources";
import { useBatteryLogs } from "@/hooks/use-resources";
import { formatDate } from "@/lib/utils";

export default function BatteryDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const id = Number(params.id);

  const { data: battery, isLoading } = batteryHooks.useDetail(id);
  const { data: logs } = useBatteryLogs(id, 100);

  const chartData = React.useMemo(
    () =>
      (logs ?? [])
        .slice()
        .reverse()
        .map((l) => ({
          label: new Date(l.recordedAt).toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" }),
          soc: l.soc,
          temperature: Number(l.temperature.toFixed(1)),
          voltage: Number(l.voltage.toFixed(2)),
        })),
    [logs]
  );

  if (isLoading || !battery) {
    return <div className="flex h-96 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div>;
  }

  return (
    <div>
      <Button variant="ghost" size="sm" className="mb-3 -ml-2" onClick={() => router.push("/batteries")}>
        <ArrowLeft className="h-4 w-4" /> Back to batteries
      </Button>

      <PageHeader
        title={`Battery #${battery.id}`}
        description={`Last updated ${formatDate(battery.updatedAt)}`}
        actions={<div className="flex gap-2"><HealthBadge state={battery.healthState} /><LocationBadge type={battery.locationType} /></div>}
      />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="State of Charge" value={`${battery.soc}%`} icon={Gauge} accent={battery.soc >= 60 ? "success" : battery.soc >= 30 ? "warning" : "destructive"} />
        <StatCard label="Temperature" value={`${battery.temperature.toFixed(1)} °C`} icon={Thermometer} accent="warning" />
        <StatCard label="Voltage" value={`${battery.voltage.toFixed(2)} V`} icon={Zap} accent="primary" />
        <StatCard label="Health" value={battery.healthState} icon={BatteryFull} accent={battery.healthState === "Good" ? "success" : "warning"} />
      </div>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle>Telemetry History</CardTitle>
          <CardDescription>Recent recorded measurements (auto-refreshes every 15s).</CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="all">
            <TabsList>
              <TabsTrigger value="all">Combined</TabsTrigger>
              <TabsTrigger value="soc">SOC</TabsTrigger>
              <TabsTrigger value="temp">Temperature</TabsTrigger>
              <TabsTrigger value="voltage">Voltage</TabsTrigger>
            </TabsList>
            <TabsContent value="all">
              <MultiLineChart data={chartData} height={320} series={[
                { key: "soc", label: "SOC (%)", color: "#0057b8" },
                { key: "temperature", label: "Temp (°C)", color: "#d97706" },
                { key: "voltage", label: "Voltage (V)", color: "#16a34a" },
              ]} />
            </TabsContent>
            <TabsContent value="soc">
              <MultiLineChart data={chartData} height={320} series={[{ key: "soc", label: "SOC (%)", color: "#0057b8" }]} />
            </TabsContent>
            <TabsContent value="temp">
              <MultiLineChart data={chartData} height={320} series={[{ key: "temperature", label: "Temp (°C)", color: "#d97706" }]} />
            </TabsContent>
            <TabsContent value="voltage">
              <MultiLineChart data={chartData} height={320} series={[{ key: "voltage", label: "Voltage (V)", color: "#16a34a" }]} />
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>
    </div>
  );
}
