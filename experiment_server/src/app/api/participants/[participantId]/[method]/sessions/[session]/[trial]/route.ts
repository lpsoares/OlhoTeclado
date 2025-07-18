import { addTrialData, getTrialData } from "@/db/trial";
import { Method } from "@/models/method";
import { NextResponse } from "next/server";

export async function GET(request: Request,
  { params }: { params: Promise<{ participantId: string, method: string, session: string, trial: string }> }
) {
  const { participantId, method, session: sessionsStr, trial: trialStr } = await params;
  const session = parseInt(sessionsStr, 10);
  const trial = parseInt(trialStr, 10);

  if (isNaN(session) || isNaN(trial)) {
    return NextResponse.json(
      { error: 'Invalid session or trial number' },
      { status: 400 }
    );
  }

  try {
    const trialData = getTrialData(participantId, method as Method, session, trial);
    return NextResponse.json(trialData, { status: 200 });
  } catch (error) {
    console.error(`Error fetching trial data for participant ${participantId}, session ${session}, trial ${trial}:`, error);
    return NextResponse.json(
      { error: 'Trial data not found' },
      { status: 404 }
    );
  }
}

export async function POST(request: Request,
  { params }: { params: Promise<{ participantId: string, method: string, session: string, trial: string }> }
) {
  const { participantId, method, session: sessionsStr, trial: trialStr } = await params;
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
    addTrialData(participantId, method as Method, session, trial, timestamp, type, data);
  });
  return NextResponse.json({ success: true });
}
