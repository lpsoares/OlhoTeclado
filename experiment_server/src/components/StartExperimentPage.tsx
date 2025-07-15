import { Participant, participantSchema } from "@/models/participant";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useParticipants, useStartExperiment } from "./APIClient";
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

export default function StartExperimentPage() {
  const form = useParticipantForm();
  const { isValid } = form.formState;

  const { participants } = useParticipants();
  const { startExperiment } = useStartExperiment();

  const handleChangeParticipant = (participant: Participant) => {
    form.reset(participant);
  };
  const handleSubmit = async (data: Participant) => {
    const participant = participantSchema.parse(data);
    startExperiment(participant.id);
  };

  return (
    <div className="flex size-full items-center justify-center">
      <Card className="w-[400px]">
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
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Sex</FormLabel>
                    <RadioGroup
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                      className="flex gap-6"
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

              <Button type="submit" className="w-full" disabled={!isValid}>
                Start Experiment
              </Button>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
}

function useParticipantForm(participant?: Participant) {
  return useForm<Participant>({
    resolver: zodResolver(participantSchema),
    defaultValues: {
      id: participant?.id || "",
      name: participant?.name || "",
      age: participant?.age || 18,
      sex: participant?.sex || "M",
    },
  });
}
