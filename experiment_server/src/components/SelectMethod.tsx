import { Method, methods } from "@/models/method";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "./ui/select";

type SelectMethodProps = {
  onChange: (method: string) => void;
  defaultValue?: Method;
  className?: string;
  disabled?: boolean;
};
export default function SelectMethod({
  onChange,
  defaultValue,
  className,
  disabled,
}: SelectMethodProps) {
  return (
    <Select onValueChange={onChange} defaultValue={defaultValue}>
      <SelectTrigger className={className} disabled={disabled}>
        <SelectValue placeholder="Select the typing method" />
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectLabel>Select Method</SelectLabel>
          {methods.map((method) => (
            <SelectItem key={method} value={method}>
              {method.charAt(0).toUpperCase() + method.slice(1)}
            </SelectItem>
          ))}
        </SelectGroup>
      </SelectContent>
    </Select>
  );
}
