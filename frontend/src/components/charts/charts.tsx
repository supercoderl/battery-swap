"use client";
import * as React from "react";
import {
  ResponsiveContainer, BarChart, Bar, LineChart, Line, AreaChart, Area,
  PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend,
} from "recharts";
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from "@/components/ui/card";
import type { ChartPoint } from "@/types";

const CHART_COLORS = ["#0057b8", "#16a34a", "#d97706", "#9333ea", "#dc2626", "#0ea5e9"];

const axisProps = {
  stroke: "var(--muted-foreground)",
  fontSize: 12,
  tickLine: false,
  axisLine: false,
};

const tooltipStyle = {
  contentStyle: {
    background: "var(--popover)",
    border: "1px solid var(--border)",
    borderRadius: "0.5rem",
    color: "var(--popover-foreground)",
    fontSize: "12px",
  },
};

interface ChartCardProps {
  title: string;
  description?: string;
  data: ChartPoint[];
  height?: number;
  className?: string;
}

function ChartShell({
  title, description, className, children,
}: { title: string; description?: string; className?: string; children: React.ReactNode }) {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        {description && <CardDescription>{description}</CardDescription>}
      </CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
  );
}

export function BarChartCard({ title, description, data, height = 280, className }: ChartCardProps) {
  return (
    <ChartShell title={title} description={description} className={className}>
      <ResponsiveContainer width="100%" height={height}>
        <BarChart data={data} margin={{ left: -16, right: 8, top: 8 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
          <XAxis dataKey="label" {...axisProps} />
          <YAxis allowDecimals={false} {...axisProps} />
          <Tooltip {...tooltipStyle} cursor={{ fill: "var(--muted)" }} />
          <Bar dataKey="value" fill="#0057b8" radius={[6, 6, 0, 0]} maxBarSize={48} />
        </BarChart>
      </ResponsiveContainer>
    </ChartShell>
  );
}

export function LineChartCard({ title, description, data, height = 280, className }: ChartCardProps) {
  return (
    <ChartShell title={title} description={description} className={className}>
      <ResponsiveContainer width="100%" height={height}>
        <LineChart data={data} margin={{ left: -16, right: 8, top: 8 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
          <XAxis dataKey="label" {...axisProps} />
          <YAxis allowDecimals={false} {...axisProps} />
          <Tooltip {...tooltipStyle} />
          <Line type="monotone" dataKey="value" stroke="#0057b8" strokeWidth={2.5} dot={{ r: 3 }} activeDot={{ r: 5 }} />
        </LineChart>
      </ResponsiveContainer>
    </ChartShell>
  );
}

export function AreaChartCard({ title, description, data, height = 280, className }: ChartCardProps) {
  return (
    <ChartShell title={title} description={description} className={className}>
      <ResponsiveContainer width="100%" height={height}>
        <AreaChart data={data} margin={{ left: -16, right: 8, top: 8 }}>
          <defs>
            <linearGradient id="areaFill" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor="#0057b8" stopOpacity={0.35} />
              <stop offset="95%" stopColor="#0057b8" stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
          <XAxis dataKey="label" {...axisProps} />
          <YAxis allowDecimals={false} {...axisProps} />
          <Tooltip {...tooltipStyle} />
          <Area type="monotone" dataKey="value" stroke="#0057b8" strokeWidth={2.5} fill="url(#areaFill)" />
        </AreaChart>
      </ResponsiveContainer>
    </ChartShell>
  );
}

export function PieChartCard({ title, description, data, height = 280, className }: ChartCardProps) {
  const total = data.reduce((sum, d) => sum + d.value, 0);
  return (
    <ChartShell title={title} description={description} className={className}>
      {total === 0 ? (
        <div className="flex h-[280px] items-center justify-center text-sm text-muted-foreground">No data</div>
      ) : (
        <ResponsiveContainer width="100%" height={height}>
          <PieChart>
            <Pie data={data} dataKey="value" nameKey="label" cx="50%" cy="50%" innerRadius={60} outerRadius={95} paddingAngle={3}>
              {data.map((_, i) => <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />)}
            </Pie>
            <Tooltip {...tooltipStyle} />
            <Legend iconType="circle" wrapperStyle={{ fontSize: "12px" }} />
          </PieChart>
        </ResponsiveContainer>
      )}
    </ChartShell>
  );
}

/** Multi-series line chart for battery telemetry (SOC/Temp/Voltage). */
export function MultiLineChart({
  data, series, height = 280,
}: {
  data: Record<string, number | string>[];
  series: { key: string; label: string; color: string }[];
  height?: number;
}) {
  return (
    <ResponsiveContainer width="100%" height={height}>
      <LineChart data={data} margin={{ left: -16, right: 8, top: 8 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
        <XAxis dataKey="label" {...axisProps} />
        <YAxis {...axisProps} />
        <Tooltip {...tooltipStyle} />
        <Legend iconType="plainline" wrapperStyle={{ fontSize: "12px" }} />
        {series.map((s) => (
          <Line key={s.key} type="monotone" dataKey={s.key} name={s.label} stroke={s.color} strokeWidth={2} dot={false} />
        ))}
      </LineChart>
    </ResponsiveContainer>
  );
}
