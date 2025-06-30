import * as z from "zod";

export const participantSchema = z.object({
  id: z.string().min(1, { message: "ID is required" }),
  name: z.string().min(1, { message: "Name is required" }),
  age: z.coerce
    .number()
    .min(15, { message: "Age is required" })
    .max(90, { message: "Age must be between 15 and 90" }),
  sex: z.union([z.literal("M"), z.literal("F")]),
});
export type Participant = z.infer<typeof participantSchema>;
