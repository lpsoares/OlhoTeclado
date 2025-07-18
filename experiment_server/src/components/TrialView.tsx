import { Method } from "@/models/method";
import { Participant } from "@/models/participant";
import { useState } from "react";
import { useListTrials, useTrial } from "./APIClient";
import { TrialVideo } from "./TrialVideo";
import { Select, SelectContent, SelectItem, SelectTrigger } from "./ui/select";

type TrialViewProps = {
  participant: Participant | null;
  method: Method | null;
  session: number | null;
};

export function TrialView({ participant, method, session }: TrialViewProps) {
  const [trialNumber, setTrialNumber] = useState<number | null>(null);
  const { trials } = useListTrials(participant?.id, method, session || 0);
  const { trialEvents, isLoading: isLoadingTrial } = useTrial(
    participant?.id,
    method,
    session,
    trialNumber
  );

  if (!participant || !method || session === null) {
    return (
      <div className="text-red-500">
        Please select a participant and method.
      </div>
    );
  }

  return (
    <div>
      <SelectTrial
        trials={trials}
        selectedTrial={trialNumber}
        onSelect={(selectedTrial) => setTrialNumber(selectedTrial)}
      />
      {trialNumber !== null && (
        <div className="mt-4">
          <h3 className="text-lg font-semibold">Trial {trialNumber}</h3>
          {isLoadingTrial ? (
            <div>Loading...</div>
          ) : trialEvents ? (
            <TrialVideo trialEvents={trialEvents} />
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
