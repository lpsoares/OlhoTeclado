import { getCurrentParticipant } from '@/db/participant';
import { getLatestSession } from '@/db/session';
import { NextResponse } from 'next/server';

export async function GET() {
  const currentData = getCurrentParticipant();
  const participant = currentData?.participant || null;
  const method = currentData?.method || null;
  const session = (participant?.id && method) ? getLatestSession(participant.id, method) : null;
  return NextResponse.json({ participant, method, session }, { status: 200 });
}
