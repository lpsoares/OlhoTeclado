import { Method, methodSchema } from "@/models/method";
import { Participant, participantSchema } from "@/models/participant";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import z from "zod";
import { useStartExperiment, useStopExperiment } from "./APIClient";
import SelectMethod from "./SelectMethod";
import SelectRegisteredParticipant from "./SelectRegisteredParticipant";
import { Button } from "./ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "./ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "./ui/form";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { RadioGroup, RadioGroupItem } from "./ui/radio-group";
import { Separator } from "./ui/separator";

type StartExperimentPageProps = {
  participants: Participant[] | null;
  currentSession: {
    participant: Participant | null;
    session: number | null;
    method: Method | null;
  };
};
export default function StartExperimentPage({
  participants,
  currentSession,
}: StartExperimentPageProps) {
  const form = useExperimentStartForm(currentSession.participant);
  const { isValid } = form.formState;
  const { stopExperiment } = useStopExperiment();

  const { startExperiment } = useStartExperiment();
  const experimentRunning =
    currentSession.participant !== null &&
    currentSession.session !== null &&
    currentSession.method !== null;

  useEffect(() => {
    if (currentSession.participant) {
      form.reset(currentSession.participant);
    }
  }, [currentSession.participant, form]);

  const handleChangeParticipant = (participant: Participant) => {
    form.reset(participant);
  };
  const handleSubmit = async (data: ParticipantWithMethod) => {
    const participantWithMethod = participantWithMethodSchema.parse(data);
    const { method, ...participant } = participantWithMethod;
    startExperiment({
      participant,
      method: method! as Method,
    });
  };

  const handleStopExperiment = () => {
    stopExperiment();
  };

  return (
    <div className="flex min-h-full items-center justify-center">
      <Card className="w-[400px] min-h-full">
        <CardHeader>
          <CardTitle>Participant Information</CardTitle>
          <CardDescription>
            Please fill in your details to start the experiment
          </CardDescription>
        </CardHeader>
        <CardContent>
          {!!participants?.length && (
            <>
              <SelectRegisteredParticipant
                participants={participants}
                onChange={handleChangeParticipant}
                className="w-full"
                disabled={experimentRunning}
              />
              <Separator className="my-6" />
            </>
          )}

          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(handleSubmit)}
              className="space-y-4"
            >
              <FormField
                control={form.control}
                name="id"
                disabled={experimentRunning}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>ID</FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="name"
                disabled={experimentRunning}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Name</FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="age"
                disabled={experimentRunning}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Age</FormLabel>
                    <FormControl>
                      <Input type="number" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="sex"
                disabled={experimentRunning}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Sex</FormLabel>
                    <RadioGroup
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                      className="flex gap-6"
                      disabled={experimentRunning}
                    >
                      <div className="flex items-center gap-3">
                        <RadioGroupItem value="M" id="sex-M" />
                        <Label htmlFor="sex-M">Male</Label>
                      </div>
                      <div className="flex items-center gap-3">
                        <RadioGroupItem value="F" id="sex-F" />
                        <Label htmlFor="sex-F">Female</Label>
                      </div>
                    </RadioGroup>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <Separator className="my-6" />

              <FormField
                control={form.control}
                name="method"
                disabled={experimentRunning}
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Typing Method</FormLabel>
                    <SelectMethod
                      onChange={field.onChange}
                      defaultValue={field.value}
                      disabled={experimentRunning}
                      className="w-full"
                    />
                    <FormMessage />
                  </FormItem>
                )}
              />

              {!experimentRunning && (
                <Button type="submit" className="w-full" disabled={!isValid}>
                  Start Experiment
                </Button>
              )}
            </form>
          </Form>
          {experimentRunning && (
            <Button
              type="submit"
              className="w-full mt-4"
              variant="destructive"
              onClick={handleStopExperiment}
            >
              Stop Experiment
            </Button>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

const participantWithMethodSchema = participantSchema
  .extend({
    method: methodSchema.optional(),
  })
  .refine(
    (data) => {
      return data.method !== undefined;
    },
    {
      message: "Method is required",
    }
  );
type ParticipantWithMethod = z.infer<typeof participantWithMethodSchema>;

function useExperimentStartForm(participant?: Participant | null) {
  return useForm<ParticipantWithMethod>({
    resolver: zodResolver(participantWithMethodSchema),
    defaultValues: {
      id: participant?.id || "",
      name: participant?.name || "",
      age: participant?.age || 18,
      sex: participant?.sex || "M",
    },
  });
}
