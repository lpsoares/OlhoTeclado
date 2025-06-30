import { startTrial } from '@/db/trial';
import { NextResponse } from 'next/server';

export async function POST(request: Request,
  { params }: { params: { participantId: string, session: string } }
) {
  const { participantId, session: sessionsStr } = params;
  const session = parseInt(sessionsStr, 10);
  try {
    const trial = startTrial(participantId, session);
    return NextResponse.json(trial, { status: 201 });
  }
  catch (error) {
    console.error('Error starting trial:', error);
    return NextResponse.json(
      { error: 'Failed to start trial' },
      { status: 500 }
    );
  }
}
