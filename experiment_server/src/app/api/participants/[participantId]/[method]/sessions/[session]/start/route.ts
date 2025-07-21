import { startTrial } from '@/db/trial';
import { Method } from '@/models/method';
import { NextResponse } from 'next/server';

export async function POST(request: Request,
  { params }: { params: Promise<{ participantId: string, method: string, session: string }> }
) {
  const { participantId, method, session: sessionsStr } = await params;
  const session = parseInt(sessionsStr, 10);
  const { timestamp } = await request.json();
  
  try {
    const trialData = startTrial(participantId, method as Method, session, timestamp);
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
