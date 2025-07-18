import { Participant } from "@/models/participant";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "./ui/select";

type SelectRegisteredParticipantProps = {
  participants: Participant[];
  onChange: (participant: Participant) => void;
  className?: string;
  disabled?: boolean;
};
export default function SelectRegisteredParticipant({
  participants,
  onChange,
  className,
  disabled,
}: SelectRegisteredParticipantProps) {
  const handleValueChange = (value: string) => {
    const selectedParticipant = participants.find(
      (participant) => participant.id === value
    ) ?? { id: "", name: "", age: 20, sex: "M" };
    onChange(selectedParticipant);
  };

  return (
    <Select onValueChange={handleValueChange}>
      <SelectTrigger className={className} disabled={disabled}>
        <SelectValue placeholder="Select a registered participant" />
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>Select Participant</SelectLabel>
          <SelectItem
            value="NEW"
            onClick={() => onChange({ id: "", name: "", age: 20, sex: "M" })}
          >
            New Participant
          </SelectItem>
          {participants.map((participant) => (
            <SelectItem
              key={participant.id}
              value={participant.id}
              onClick={() => onChange(participant)}
            >
              {participant.id} ({participant.name})
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}
