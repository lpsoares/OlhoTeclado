import z from "zod";

export const methodSchema = z.union([z.literal("green"), z.literal("blue")]);
export const methods = methodSchema.options.map((option) => option.value);
export type Method = z.infer<typeof methodSchema>;
