"use client";
import Link from "next/link";
import {
  MapPin, Server, Grid3x3, BatteryFull, BatteryCharging,
  Zap, Users, ArrowLeftRight, Loader2,
} from "lucide-react";
import { PageHeader } from "@/components/shared/page-header";
import { StatCard } from "@/components/shared/stat-card";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "@/components/ui/table";
import { BarChartCard, AreaChartCard, PieChartCard } from "@/components/charts/charts";
import {
  CabinetStatusBadge, TransactionStatusBadge, SessionStatusBadge, SocBadge,
} from "@/components/shared/status-badge";
import { useDashboardOverview } from "@/hooks/use-resources";
import { formatDate, formatDuration } from "@/lib/utils";

export default function DashboardPage() {
  const { data, isLoading } = useDashboardOverview();

  if (isLoading || !data) {
    return (
      <div className="flex h-96 items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin text-primary" />
      </div>
    );
  }

  const { stats, charts, latestTransactions, latestBatteryLogs, activeSessions } = data;

  return (
    <div>
      <PageHeader title="Dashboard" description="Real-time overview of the battery swapping network." />

      {/* Stat cards */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Total Stations" value={stats.totalStations} icon={MapPin} accent="primary" />
        <StatCard label="Total Cabinets" value={stats.totalCabinets} icon={Server} accent="primary" />
        <StatCard label="Total Slots" value={stats.totalSlots} icon={Grid3x3} accent="muted" />
        <StatCard label="Total Batteries" value={stats.totalBatteries} icon={BatteryFull} accent="success" />
        <StatCard label="Batteries In Slot" value={stats.batteriesInSlot} icon={BatteryCharging} accent="success" />
        <StatCard label="Batteries Rented" value={stats.batteriesRented} icon={Zap} accent="warning" />
        <StatCard label="Active Users" value={stats.activeUsers} icon={Users} accent="primary" />
        <StatCard label="Transactions Today" value={stats.totalTransactionsToday} icon={ArrowLeftRight} accent="destructive" />
      </div>

      {/* Charts row 1 */}
      <div className="mt-6 grid gap-4 lg:grid-cols-3">
        <AreaChartCard className="lg:col-span-2" title="Daily Swap Transactions" description="Last 7 days" data={charts.dailySwaps} />
        <PieChartCard title="Cabinet Status" description="Distribution by status" data={charts.cabinetStatusDistribution} />
      </div>

      {/* Charts row 2 */}
      <div className="mt-4 grid gap-4 lg:grid-cols-3">
        <BarChartCard title="Monthly Transaction Trend" description="Last 6 months" data={charts.monthlyTrend} />
        <PieChartCard title="Battery Health" description="Good vs Degraded" data={charts.batteryHealthDistribution} />
        <BarChartCard title="Battery SOC Distribution" description="By charge bucket" data={charts.batterySocDistribution} />
      </div>

      {/* Most active stations */}
      <div className="mt-4">
        <BarChartCard title="Most Active Stations" description="Top stations by transaction volume" data={charts.mostActiveStations} height={260} />
      </div>

      {/* Tables */}
      <div className="mt-6 grid gap-4 xl:grid-cols-2">
        <Card>
          <CardHeader className="flex-row items-center justify-between">
            <CardTitle>Latest Transactions</CardTitle>
            <Link href="/transactions" className="text-sm text-primary hover:underline">View all</Link>
          </CardHeader>
          <CardContent className="px-0 pb-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>User</TableHead>
                  <TableHead>Cabinet</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Duration</TableHead>
                  <TableHead>Time</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {latestTransactions.map((t) => (
                  <TableRow key={t.id}>
                    <TableCell className="font-medium">{t.userFullName}</TableCell>
                    <TableCell>{t.cabinetModel}</TableCell>
                    <TableCell><TransactionStatusBadge status={t.status} /></TableCell>
                    <TableCell>{formatDuration(t.durationSeconds)}</TableCell>
                    <TableCell className="text-muted-foreground">{formatDate(t.createdAt)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex-row items-center justify-between">
            <CardTitle>Active Sessions</CardTitle>
            <Link href="/sessions" className="text-sm text-primary hover:underline">View all</Link>
          </CardHeader>
          <CardContent className="px-0 pb-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>User</TableHead>
                  <TableHead>Cabinet</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Started</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {activeSessions.length === 0 ? (
                  <TableRow><TableCell colSpan={4} className="py-8 text-center text-muted-foreground">No active sessions</TableCell></TableRow>
                ) : activeSessions.map((s) => (
                  <TableRow key={s.id}>
                    <TableCell className="font-medium">{s.userFullName}</TableCell>
                    <TableCell>{s.cabinetModel}</TableCell>
                    <TableCell><SessionStatusBadge status={s.status} /></TableCell>
                    <TableCell className="text-muted-foreground">{formatDate(s.createdAt)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </div>

      {/* Latest battery logs */}
      <Card className="mt-4">
        <CardHeader className="flex-row items-center justify-between">
          <CardTitle>Latest Battery Telemetry</CardTitle>
          <Link href="/batteries" className="text-sm text-primary hover:underline">View batteries</Link>
        </CardHeader>
        <CardContent className="px-0 pb-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Battery</TableHead>
                <TableHead>SOC</TableHead>
                <TableHead>Temperature</TableHead>
                <TableHead>Voltage</TableHead>
                <TableHead>Recorded</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {latestBatteryLogs.map((l) => (
                <TableRow key={l.id}>
                  <TableCell className="font-medium">#{l.batteryId}</TableCell>
                  <TableCell><SocBadge soc={l.soc} /></TableCell>
                  <TableCell>{l.temperature.toFixed(1)} °C</TableCell>
                  <TableCell>{l.voltage.toFixed(2)} V</TableCell>
                  <TableCell className="text-muted-foreground">{formatDate(l.recordedAt)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
