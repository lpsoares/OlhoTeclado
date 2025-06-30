import { getCurrentParticipant } from '@/db/participant';
import { getLatestSession } from '@/db/session';
import { NextResponse } from 'next/server';

export async function GET() {
  const participant = getCurrentParticipant();
  const session = participant?.id ? getLatestSession(participant.id) : null;
  return NextResponse.json({ participant, session });
}
