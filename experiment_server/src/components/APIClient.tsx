import { Method } from "@/models/method";
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
    queryFn: async () =>
      await doGet(`/participants`, "Failed to fetch participants"),
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
    queryKey: ["participants", participantId],
    queryFn: () => {
      return doGet(
        `/participants/${participantId}`,
        "Failed to fetch participant"
      );
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
    queryKey: ["participants", "current"],
    queryFn: async () => {
      return doGet(
        `/participants/current`,
        "Failed to fetch current participant"
      );
    },
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
  return {
    ...query,
    participant: (query.data?.participant ?? null) as Participant | null,
    session: (query.data?.session ?? null) as number | null,
    method: (query.data?.method ?? null) as Method | null,
  };
}

export function useStartExperiment() {
  const mutation = useMutation({
    mutationFn: async ({
      participant,
      method,
    }: {
      participant: Participant;
      method: Method;
    }) => {
      await doPost(`/participants`, {
        body: participant,
        errorMsg: "Failed to create or update participant",
      });
      return await doPost(`/participants/${participant.id}/${method}/start`, {
        errorMsg: "Failed to start participant",
        invalidateKey: ["participants", "current"],
      });
    },
  });
  return {
    ...mutation,
    startExperiment: mutation.mutate as (data: {
      participant: Participant;
      method: Method;
    }) => void,
  };
}

export function useStopExperiment() {
  const mutation = useMutation({
    mutationFn: () => {
      return doPost(`/participants/current/stop`, {
        errorMsg: "Failed to stop participant",
        invalidateKey: ["participants", "current"],
      });
    },
  });
  return {
    ...mutation,
    stopExperiment: mutation.mutate as () => void,
  };
}

export function useListSessions(participantId: string, method: Method) {
  const query = useQuery({
    queryKey: ["participants", participantId, method, "sessions"],
    queryFn: () => {
      return doGet(
        `/participants/${participantId}/${method}/sessions`,
        "Failed to fetch sessions"
      );
    },
    refetchOnWindowFocus: false,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
  return {
    ...query,
    sessions: query.data as number[] | null,
  };
}

async function doPost(
  uri: string,
  config?: {
    body?: any;
    errorMsg?: string;
    invalidateKey?: string[];
  }
) {
  const response = await doRequest("POST", uri, config?.body, config?.errorMsg);
  if (config?.invalidateKey) {
    await queryClient.invalidateQueries({ queryKey: config?.invalidateKey });
  }
  return response;
}

function doGet(uri: string, errorMsg?: string) {
  return doRequest("GET", uri, null, errorMsg);
}

async function doRequest(
  method: "POST" | "GET",
  uri: string,
  body?: any,
  errorMsg?: string
) {
  const requestInit: RequestInit = {};
  if (method === "POST") {
    requestInit.method = "POST";
  }
  if (body) {
    requestInit.body = JSON.stringify(body);
    requestInit.headers = {
      "Content-Type": "application/json",
    };
  }
  const response = await fetch(`${API_URL}${uri}`, requestInit);
  if (!response.ok) {
    throw new Error(
      errorMsg ??
        `Failed to complete request to ${uri} with status ${response.status}: ${response.statusText}`
    );
  }
  const ret = await response.json();
  return ret;
}
