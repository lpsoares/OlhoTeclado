"use client";

import {
  APIClientProvider,
  useCurrentParticipant,
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
  const { participant, session, method } = useCurrentParticipant();
  const started = !!participant && !!method && !!session;
  return (
    <>
      {started && (
        <ExperimentPage
          participant={participant}
          session={session}
          method={method}
        />
      )}
      {!started && <StartExperimentPage />}
    </>
  );
}
