import { createOrUpdateParticipant, listParticipants } from '@/db/participant';
import { NextResponse } from 'next/server';

export async function GET() {
  const participants = listParticipants();
  return NextResponse.json(participants, { status: 200 });
}

export async function POST(request: Request) {
  const data = await request.json();
  const participant = createOrUpdateParticipant(data);
  return NextResponse.json(participant, { status: 201 });
}
