import { TrialEvent } from "@/db/event";
import { Dispatch, useEffect, useState } from "react";
import {
  CartesianGrid,
  ResponsiveContainer,
  Scatter,
  ScatterChart,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { Payload } from "recharts/types/component/DefaultTooltipContent";
import { computeTrialStats, TrialStats } from "./stats";
import { ChartContainer } from "./ui/chart";

type SessionViewProps = {
  allTrials: (TrialEvent[] | null)[];
  setTrialNumber: Dispatch<React.SetStateAction<number | null>>;
};

export default function SessionView({
  allTrials,
  setTrialNumber,
}: SessionViewProps) {
  const [trialStats, setTrialStats] = useState<TrialStats[]>([]);

  useEffect(() => {
    if (allTrials) {
      const stats = allTrials.map((trialEvents, index) =>
        computeTrialStats(trialEvents, index + 1)
      );
      setTrialStats(stats);
    }
  }, [allTrials]);

  const handleClickPoint = (data: TrialStats) => {
    setTrialNumber(data.trialId);
  };

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
              content={CustomTooltip}
            />
            <Scatter
              data={trialStats}
              fill="#8884d8"
              onClick={handleClickPoint}
            />
          </ScatterChart>
        </ChartContainer>
      </ResponsiveContainer>
    </div>
  );
}

const CustomTooltip = ({
  active,
  payload,
}: {
  active?: boolean;
  payload?: Payload<number, string>[];
  label?: string;
}) => {
  const isVisible = active && payload && payload.length;
  return (
    <div
      className="border p-2 bg-white shadow-lg rounded"
      style={{ visibility: isVisible ? "visible" : "hidden" }}
    >
      {isVisible && (
        <>
          <p className="font-bold">Trial {payload[0].payload.trialId}</p>
          <ul className="mt-2">
            <li>
              Target text: <pre>{payload[0].payload.targetText}</pre>
            </li>
            <li>
              Final text: <pre>{payload[0].payload.finalText}</pre>
            </li>
            <li className="mt-2">
              Duration: {payload[0].payload.duration.toFixed(2)} seconds
            </li>
            <li>
              Min String Distance: {payload[0].payload.minStringDistance} chars
            </li>
            <li>
              Typing Speed: {payload[0].payload.typingSpeed.toFixed(2)} wpm
            </li>
          </ul>
        </>
      )}
    </div>
  );
};
