import { listTrials } from '@/db/trial';
import { NextResponse } from 'next/server';

export async function GET(request: Request,
  { params }: { params: Promise<{ participantId: string, session: string }> }
) {
  const { participantId, session: sessionsStr } = await params;
  const session = parseInt(sessionsStr, 10);
  const trials = listTrials(participantId, session);
  if (!trials) {
    return NextResponse.json(
      { error: 'No trials found for this participant' },
      { status: 404 }
    );
  }
  return NextResponse.json(trials, { status: 200 });
}
