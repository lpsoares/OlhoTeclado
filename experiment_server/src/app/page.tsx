"use client";

import {
  APIClientProvider,
  useCurrentSession,
  useParticipants,
} from "@/components/APIClient";
import ExperimentPage from "@/components/ExperimentPage";
import StartExperimentPage from "@/components/StartExperimentPage";

export default function Home() {
  return (
    <main className="grid min-h-screen">
      <APIClientProvider>
        <HomeClient />
      </APIClientProvider>
    </main>
  );
}

function HomeClient() {
  const { participants } = useParticipants();
  const currentSession = useCurrentSession();
  return (
    <div className="flex flex-wrap m-4 gap-4">
      <StartExperimentPage
        participants={participants}
        currentSession={currentSession}
      />
      <ExperimentPage
        participants={participants}
        currentSession={currentSession}
      />
    </div>
  );
}
