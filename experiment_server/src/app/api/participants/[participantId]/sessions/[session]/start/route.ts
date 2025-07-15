import { startTrial } from '@/db/trial';
import { NextResponse } from 'next/server';

export async function POST(request: Request,
  { params }: { params: Promise<{ participantId: string, session: string }> }
) {
  const { participantId, session: sessionsStr } = await params;
  const session = parseInt(sessionsStr, 10);
  const { timestamp } = await request.json();
  
  try {
    const trialData = startTrial(participantId, session, timestamp);
    return NextResponse.json(trialData, { status: 201 });
  }
  catch (error) {
    console.error('Error starting trial:', error);
    return NextResponse.json(
      { error: 'Failed to start trial' },
      { status: 500 }
    );
  }
}
