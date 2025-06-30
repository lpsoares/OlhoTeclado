import { listSessions } from '@/db/session';
import { NextResponse } from 'next/server';

export async function GET(request: Request,
  { params }: { params: { participantId: string } }
) {
  const { participantId } = params;
  const sessions = listSessions(participantId);
  if (!sessions) {
    return NextResponse.json(
      { error: 'No sessions found for this participant' },
      { status: 404 }
    );
  }
  return NextResponse.json(sessions, { status: 200 });
}
