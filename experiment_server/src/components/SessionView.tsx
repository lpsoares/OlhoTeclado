import { TrialEvent } from "@/db/event";
import { useEffect, useState } from "react";
import {
  CartesianGrid,
  ResponsiveContainer,
  Scatter,
  ScatterChart,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { computeTrialStats, TrialStats } from "./stats";
import { ChartContainer } from "./ui/chart";

type SessionViewProps = {
  allTrials: TrialEvent[][] | null;
};

export default function SessionView({ allTrials }: SessionViewProps) {
  const [trialStats, setTrialStats] = useState<TrialStats[]>([]);

  useEffect(() => {
    if (allTrials) {
      const stats = allTrials.map((trialEvents) =>
        computeTrialStats(trialEvents)
      );
      setTrialStats(stats);
    }
  }, [allTrials]);

  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">Session Statistics</h2>
      <ResponsiveContainer>
        <ChartContainer config={{}} className="min-h-[200px] w-full">
          <ScatterChart
            width={730}
            height={250}
            margin={{
              top: 20,
              right: 20,
              bottom: 10,
              left: 10,
            }}
          >
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis
              dataKey="minStringDistance"
              type="number"
              name="Min String Distance"
              unit=" chars"
            />
            <YAxis
              dataKey="typingSpeed"
              type="number"
              name="Typing Speed"
              unit=" wpm"
            />
            <Tooltip
              cursor={{ strokeDasharray: "3 3" }}
              formatter={(value: number, name: string) => [
                value.toFixed(name === "Min String Distance" ? 0 : 2),
                name,
              ]}
            />
            <Scatter data={trialStats} fill="#8884d8" />
          </ScatterChart>
        </ChartContainer>
      </ResponsiveContainer>
    </div>
  );
}
