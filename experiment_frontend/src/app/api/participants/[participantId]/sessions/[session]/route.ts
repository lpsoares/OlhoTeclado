import { listTrials } from '@/db/trial';
import { NextResponse } from 'next/server';

export async function GET(request: Request,
  { params }: { params: { participantId: string, session: string } }
) {
  const { participantId, session: sessionsStr } = params;
  const session = parseInt(sessionsStr, 10);
  const sessions = listTrials(participantId, session);
  if (!sessions) {
    return NextResponse.json(
      { error: 'No sessions found for this participant' },
      { status: 404 }
    );
  }
  return NextResponse.json(sessions, { status: 200 });
}
