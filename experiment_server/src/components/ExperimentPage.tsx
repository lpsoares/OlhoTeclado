import { Method } from "@/models/method";
import { Participant } from "@/models/participant";
import clsx from "clsx";
import { useEffect, useState } from "react";
import { useListFullTrials, useListSessions, useListTrials } from "./APIClient";
import SessionView from "./SessionView";
import { TrialView } from "./TrialView";
import { Label } from "./ui/label";
import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "./ui/resizable";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { Switch } from "./ui/switch";

type ExperimentPageProps = {
  participants: Participant[] | null;
  currentSession: {
    participant: Participant | null;
    session: number | null;
    method: Method | null;
  };
};
export default function ExperimentPage({
  participants,
  currentSession,
}: ExperimentPageProps) {
  const [watchCurrentSession, setWatchCurrentSession] = useState(true);
  const [participant, setParticipant] = useState<Participant | null>(
    currentSession.participant
  );
  const [method, setMethod] = useState<Method | null>(currentSession.method);
  const [session, setSession] = useState<number | null>(currentSession.session);
  const { sessions } = useListSessions(participant?.id || null, method || null);
  const { trials } = useListTrials(participant?.id, method, session || 0);
  const fullTrialQueries = useListFullTrials(participant?.id, method, session, trials);
  const allTrials = fullTrialQueries.map((query) => query.fullTrial);
  const isLoadingTrials = fullTrialQueries.every((query) => query.isLoading);

  useEffect(() => {
    if (watchCurrentSession && currentSession.participant) {
      setParticipant(currentSession.participant);
      setSession(currentSession.session);
      setMethod(currentSession.method);
    }
  }, [watchCurrentSession, currentSession]);

  return (
    <section className="flex flex-col flex-grow p-4">
      {participants && (
        <div className="space-y-4 mb-4">
          <div className="flex items-center space-x-2">
            <Switch
              id="watch-current"
              checked={watchCurrentSession}
              onCheckedChange={setWatchCurrentSession}
            />
            <Label htmlFor="watch-current">Watch current session</Label>
          </div>
          <div className="flex space-x-2">
            <SelectParticipant
              participants={participants}
              onChange={setParticipant}
              selected={participant}
            />
            <SelectMethod
              onChange={setMethod}
              value={method}
              disabled={!watchCurrentSession || !participant}
            />
            <SelectSession
              sessions={sessions}
              onChange={setSession}
              selected={session}
              disabled={!watchCurrentSession || !participant || !method}
            />
          </div>
        </div>
      )}
      <h2 className="text-4xl font-bold">
        {participant?.name ?? "No Participant Selected"}{" "}
      </h2>
      {participant && (
        <h3 className="text-xl text-slate-400">
          {participant.id} ({participant.sex} - {participant.age} y.o.)
        </h3>
      )}
      {method && (
        <h4 className="text-lg text-slate-500">
          Method{" "}
          <span
            className={clsx("font-bold", {
              "text-green-600": method === "green",
              "text-blue-600": method === "blue",
            })}
          >
            {method.toLocaleUpperCase()}
          </span>{" "}
          | Session <span className="font-bold">{session}</span>
        </h4>
      )}
      <div className="flex-grow">
        <ResizablePanelGroup direction="horizontal" className="my-4">
          <ResizablePanel>
            {isLoadingTrials ? (
              <div className="text-gray-500">Loading trials...</div>
            ) : (
              <TrialView
                participant={participant}
                method={method}
                session={session}
                allTrials={allTrials}
              />
            )}
          </ResizablePanel>
          <ResizableHandle className="mx-4" />
          <ResizablePanel>
            <SessionView allTrials={allTrials} />
          </ResizablePanel>
        </ResizablePanelGroup>
      </div>
    </section>
  );
}

type SelectParticipantProps = {
  participants: Participant[];
  onChange: (participant: Participant) => void;
  selected: Participant | null;
};
function SelectParticipant({
  participants,
  onChange,
  selected,
}: SelectParticipantProps) {
  const handleValueChange = (selectedId: string) => {
    const selectedParticipant = participants.find(
      (participant) => participant.id === selectedId
    );
    if (selectedParticipant) {
      onChange(selectedParticipant);
    }
  };

  return (
    <Select
      name="participant-select"
      onValueChange={handleValueChange}
      value={selected?.id || ""}
    >
      <SelectTrigger>
        <SelectValue placeholder="Select a participant" />
      </SelectTrigger>
      <SelectContent>
        {participants.map((participant) => (
          <SelectItem key={participant.id} value={participant.id}>
            {participant.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

type SelectMethodProps = {
  onChange: (method: Method) => void;
  value: Method | null;
  disabled?: boolean;
};
function SelectMethod({ onChange, value, disabled }: SelectMethodProps) {
  const handleValueChange = (selectedMethod: string) => {
    onChange(selectedMethod as Method);
  };

  return (
    <Select
      name="method-select"
      onValueChange={handleValueChange}
      value={value || ""}
      disabled={disabled}
    >
      <SelectTrigger>
        <SelectValue placeholder="Select a method" />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value="green">Green</SelectItem>
        <SelectItem value="blue">Blue</SelectItem>
      </SelectContent>
    </Select>
  );
}

type SelectSessionProps = {
  sessions: number[] | null;
  onChange: (session: number) => void;
  selected: number | null;
  disabled?: boolean;
};
function SelectSession({
  sessions,
  onChange,
  selected,
  disabled,
}: SelectSessionProps) {
  const handleValueChange = (selectedSession: string) => {
    const sessionNumber = parseInt(selectedSession, 10);
    if (!isNaN(sessionNumber)) {
      onChange(sessionNumber);
    }
  };

  return (
    <Select
      name="session-select"
      onValueChange={handleValueChange}
      value={selected?.toString() || ""}
      disabled={disabled}
    >
      <SelectTrigger>
        <SelectValue placeholder="Select a session" />
      </SelectTrigger>
      <SelectContent>
        {sessions?.map((session) => (
          <SelectItem key={session} value={session.toString()}>
            Session {session}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
