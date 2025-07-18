import { TrialEvent } from "@/db/event";
import { Method } from "@/models/method";
import { Participant } from "@/models/participant";
import { useEffect, useState } from "react";
import { computeTrialStats, TrialStats } from "./stats";
import { TrialVideo } from "./TrialVideo";
import { Select, SelectContent, SelectItem, SelectTrigger } from "./ui/select";

type TrialViewProps = {
  participant: Participant | null;
  method: Method | null;
  session: number | null;
  allTrials: TrialEvent[][] | null;
};

export function TrialView({
  participant,
  method,
  session,
  allTrials,
}: TrialViewProps) {
  const [trialNumber, setTrialNumber] = useState<number | null>(null);
  const [trialEvents, setTrialEvents] = useState<TrialEvent[] | null>(null);

  useEffect(() => {
    if (allTrials && trialNumber !== null) {
      setTrialEvents(allTrials[trialNumber - 1] || null);
    } else {
      setTrialEvents(null);
    }
  }, [allTrials, trialNumber]);

  if (!participant || !method || session === null) {
    return (
      <div className="text-red-500">
        Please select a participant and method.
      </div>
    );
  }

  if (!allTrials || allTrials.length === 0) {
    return (
      <div className="text-red-500">
        No trials available for this session yet.
      </div>
    );
  }

  return (
    <div>
      <SelectTrial
        trials={allTrials?.map((_, i) => i + 1) || null}
        selectedTrial={trialNumber}
        onSelect={(selectedTrial) => setTrialNumber(selectedTrial)}
      />
      {trialNumber !== null && (
        <div className="mt-4">
          {trialEvents ? (
            <>
              <TrialVideo trialEvents={trialEvents} />
              <TrialStatsView trialEvents={trialEvents} />
            </>
          ) : (
            <div className="text-red-500">No trial data available.</div>
          )}
        </div>
      )}
    </div>
  );
}

type SelectTrialProps = {
  trials: number[] | null;
  selectedTrial: number | null;
  onSelect: (trial: number) => void;
};
function SelectTrial({ trials, selectedTrial, onSelect }: SelectTrialProps) {
  if (!trials || trials.length === 0) {
    return (
      <div className="text-red-500">
        No trials available for this session yet.
      </div>
    );
  }

  return (
    <Select
      value={selectedTrial?.toString() || ""}
      onValueChange={(value) => onSelect(parseInt(value, 10))}
    >
      <SelectTrigger className="w-full">
        <span className="text-sm">
          {selectedTrial !== null ? `Trial ${selectedTrial}` : "Select Trial"}
        </span>
      </SelectTrigger>
      <SelectContent>
        {trials?.map((trial) => (
          <SelectItem key={trial} value={trial.toString()}>
            Trial {trial}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

type TrialStatsViewProps = {
  trialEvents: TrialEvent[];
};
export function TrialStatsView({ trialEvents }: TrialStatsViewProps) {
  const [stats, setStats] = useState<TrialStats | null>(null);

  useEffect(() => {
    if (trialEvents && trialEvents.length > 0) {
      const stats = computeTrialStats(trialEvents);
      setStats(stats);
    }
  }, [trialEvents]);

  if (!trialEvents || trialEvents.length === 0 || !stats) {
    return <div className="text-red-500">No trial events available.</div>;
  }

  return (
    <div className="my-4 text-sm">
      <ul>
        <li>
          <span className="text-gray-400">Target Text:</span> {stats.targetText}
        </li>
        <li>
          <span className="text-gray-400">Final Text:</span> {stats.finalText}
        </li>
        <li>
          <span className="text-gray-400">Minimum String Distance:</span>{" "}
          {stats.minStringDistance} characters
        </li>
        <li>
          <span className="text-gray-400">Duration:</span>{" "}
          {stats.duration.toFixed(2)} seconds
        </li>
        <li>
          <span className="text-gray-400">Typing Speed:</span>{" "}
          {stats.typingSpeed.toFixed(2)} wpm
        </li>
      </ul>
    </div>
  );
}
