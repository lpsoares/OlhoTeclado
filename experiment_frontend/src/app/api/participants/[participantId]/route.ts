import { getParticipantDemographics } from '@/db/participant';
import { NextResponse } from 'next/server';

export async function GET(
  request: Request,
  { params }: { params: { participantId: string } }
) {
  const { participantId } = params;
  const participant = getParticipantDemographics(participantId);

  if (!participant) {
    return NextResponse.json(
      { error: 'Participant not found' },
      { status: 404 }
    );
  }

  return NextResponse.json(participant);
}
