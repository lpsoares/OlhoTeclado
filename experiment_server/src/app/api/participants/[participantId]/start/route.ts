import { startExperiment } from '@/db/participant';
import { startSession } from '@/db/session';
import { NextResponse } from 'next/server';

export async function POST(request: Request,
  { params }: { params: Promise<{ participantId: string }> }
) {
  const { participantId } = await params;
  const started = startExperiment(participantId);

  if (!started) {
    return NextResponse.json(
      { error: 'Failed to start participant' },
      { status: 404 }
    );
  }

  startSession(participantId);

  return NextResponse.json(started);
}
