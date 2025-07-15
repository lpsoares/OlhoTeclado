import { addTrialData } from "@/db/trial";
import { NextResponse } from "next/server";

export async function POST(request: Request,
  { params }: { params: Promise<{ participantId: string, session: string, trial: string }> }
) {
  const { participantId, session: sessionsStr, trial: trialStr } = await params;
  const session = parseInt(sessionsStr, 10);
  const trial = parseInt(trialStr, 10);
  const events = await request.text();
  
  events.split('\n').forEach(event => {
    const [timestamp, type, data] = event.split(',');
    if (!timestamp || !type || !data) {
      console.error('Invalid event data:', event);
      return NextResponse.json(
        { error: 'Invalid event data' },
        { status: 400 }
      );
    }
    addTrialData(participantId, session, trial, timestamp, type, data);
  });
  return NextResponse.json({ success: true });
}