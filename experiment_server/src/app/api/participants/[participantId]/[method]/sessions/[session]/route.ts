import { listTrials } from '@/db/trial';
import { Method } from '@/models/method';
import { NextResponse } from 'next/server';

export async function GET(request: Request,
  { params }: { params: Promise<{ participantId: string, method: string, session: string }> }
) {
  const { participantId, method, session: sessionsStr } = await params;
  const session = parseInt(sessionsStr, 10);
  const trials = listTrials(participantId, method as Method, session);
  if (!trials) {
    return NextResponse.json(
      { error: 'No trials found for this participant' },
      { status: 404 }
    );
  }
  return NextResponse.json(trials, { status: 200 });
}
