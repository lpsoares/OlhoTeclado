import { startExperiment } from '@/db/participant';
import { startSession } from '@/db/session';
import { Method } from '@/models/method';
import { NextResponse } from 'next/server';

export async function POST(request: Request,
  { params }: { params: Promise<{ participantId: string, method: string }> }
) {
  const { participantId, method } = await params;
  const started = startExperiment(participantId, method as Method);

  if (!started) {
    return NextResponse.json(
      { error: 'Failed to start participant' },
      { status: 404 }
    );
  }

  startSession(participantId, method as Method);

  return NextResponse.json(started);
}
