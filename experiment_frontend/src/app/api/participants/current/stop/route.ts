import { stopExperiment } from '@/db/participant';
import { NextResponse } from 'next/server';

export async function POST() {
  const ended = stopExperiment();

  if (!ended) {
    return NextResponse.json(
      { error: 'Failed to end participant' },
      { status: 404 }
    );
  }

  return NextResponse.json(ended);
}
