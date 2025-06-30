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
  const { participant, session } = useCurrentParticipant();
  const started = !!participant && !!session;
  return (
    <>
      {started && (
        <ExperimentPage participant={participant} session={session} />
      )}
      {!started && <StartExperimentPage />}
    </>
  );
}
