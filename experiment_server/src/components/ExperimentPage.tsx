import { Participant } from "@/models/participant";
import { useStopExperiment } from "./APIClient";
import { Button } from "./ui/button";
import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "./ui/resizable";

type ExperimentPageProps = {
  participant: Participant;
  session: number;
};
export default function ExperimentPage({
  participant,
  session,
}: ExperimentPageProps) {
  const { stopExperiment } = useStopExperiment();

  const handleStopExperiment = () => {
    stopExperiment();
  };

  return (
    <section className="p-4">
      <h1 className="text-4xl">{participant.id} </h1>
      <h2 className="text-xl text-slate-400">
        {participant.name} ({participant.sex} - {participant.age} y.o.)
      </h2>
      <h3 className="text-lg text-slate-500">Session: {session}</h3>
      <ResizablePanelGroup direction="horizontal" className="max-h-1/2 my-4">
        <ResizablePanel>One</ResizablePanel>
        <ResizableHandle />
        <ResizablePanel>Two</ResizablePanel>
      </ResizablePanelGroup>
      <Button
        type="submit"
        className="w-full"
        variant="destructive"
        onClick={handleStopExperiment}
      >
        Stop Experiment
      </Button>
    </section>
  );
}
