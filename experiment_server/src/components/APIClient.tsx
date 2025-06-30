import { Participant } from "@/models/participant";
import {
  QueryClient,
  QueryClientProvider,
  useMutation,
  useQuery,
} from "@tanstack/react-query";

const queryClient = new QueryClient();

export const API_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:3000/api";

export function APIClientProvider({ children }: { children: React.ReactNode }) {
  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

export function useParticipants() {
  const query = useQuery({
    queryKey: ["participants"],
    queryFn: async () => {
      const response = await fetch(`${API_URL}/participants`);
      if (!response.ok) {
        throw new Error("Failed to fetch participants");
      }
      return response.json();
    },
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
  return {
    ...query,
    participants: query.data as Participant[] | null,
  };
}

export function useParticipant(participantId: string) {
  const query = useQuery({
    queryKey: ["participant", participantId],
    queryFn: async () => {
      const response = await fetch(`${API_URL}/participants/${participantId}`);
      if (!response.ok) {
        throw new Error("Failed to fetch participant");
      }
      return response.json();
    },
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
  return {
    ...query,
    participant: query.data as Participant | null,
  };
}

export function useCurrentParticipant() {
  const query = useQuery({
    queryKey: ["currentParticipant"],
    queryFn: async () => {
      const response = await fetch(`${API_URL}/participants/current`);
      if (!response.ok) {
        throw new Error("Failed to fetch current participant");
      }
      queryClient.invalidateQueries({ queryKey: ["currentSession"] });
      return response.json();
    },
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
  return {
    ...query,
    participant: (query.data?.participant ?? null) as Participant | null,
    session: (query.data?.session ?? null) as number | null,
  };
}

export function useStartExperiment() {
  const mutation = useMutation({
    mutationFn: async (participantId: string) => {
      const response = await fetch(
        `${API_URL}/participants/${participantId}/start`,
        {
          method: "POST",
        }
      );
      if (!response.ok) {
        throw new Error("Failed to start participant");
      }
      queryClient.invalidateQueries({ queryKey: ["currentParticipant"] });
      return response.json();
    },
  });
  return {
    ...mutation,
    startExperiment: mutation.mutate as (participantId: string) => void,
  };
}

export function useStopExperiment() {
  const mutation = useMutation({
    mutationFn: async () => {
      const response = await fetch(`${API_URL}/participants/current/stop`, {
        method: "POST",
      });
      if (!response.ok) {
        throw new Error("Failed to stop participant");
      }
      queryClient.invalidateQueries({ queryKey: ["currentParticipant"] });
      return response.json();
    },
  });
  return {
    ...mutation,
    stopExperiment: mutation.mutate as () => void,
  };
}

export function useCreateOrUpdateParticipant() {
  const mutation = useMutation({
    mutationFn: async (participant: Participant) => {
      const response = await fetch(`${API_URL}/participants`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(participant),
      });
      if (!response.ok) {
        throw new Error("Failed to create or update participant");
      }
      queryClient.invalidateQueries({ queryKey: ["participants"] });
      return response.json();
    },
  });
  return {
    ...mutation,
    createOrUpdateParticipant: mutation.mutate as (
      participant: Participant
    ) => void,
  };
}

export function useListSessions(participantId: string) {
  const query = useQuery({
    queryKey: ["sessions"],
    queryFn: async () => {
      const response = await fetch(
        `${API_URL}/participants/${participantId}/sessions`
      );
      if (!response.ok) {
        throw new Error("Failed to fetch sessions");
      }
      return response.json();
    },
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
  return {
    ...query,
    sessions: query.data as number[] | null,
  };
}
