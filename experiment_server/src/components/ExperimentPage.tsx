import { Method } from "@/models/method";
import { Participant } from "@/models/participant";
import clsx from "clsx";
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
  method: Method;
};
export default function ExperimentPage({
  participant,
  session,
  method,
}: ExperimentPageProps) {
  const { stopExperiment } = useStopExperiment();

  const handleStopExperiment = () => {
    stopExperiment();
  };

  return (
    <section className="p-4">
      <h1 className="text-4xl font-bold">{participant.name} </h1>
      <h2 className="text-xl text-slate-400">
        {participant.id} ({participant.sex} - {participant.age} y.o.)
      </h2>
      <h3 className="text-lg text-slate-500">
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
      </h3>
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
