import { listSessions } from '@/db/session';
import { Method } from '@/models/method';
import { NextResponse } from 'next/server';

export async function GET(request: Request,
  { params }: { params: Promise<{ participantId: string, method: string }> }
) {
  const { participantId, method } = await params;
  const sessions = listSessions(participantId, method as Method);
  if (!sessions) {
    return NextResponse.json(
      { error: 'No sessions found for this participant' },
      { status: 404 }
    );
  }
  return NextResponse.json(sessions, { status: 200 });
}
