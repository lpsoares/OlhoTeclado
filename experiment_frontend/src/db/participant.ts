import { Participant, participantSchema } from '@/models/participant';
import fs from 'fs';
import path from 'path';
import { DATA_DIR } from './database';

const DEMOGRAPHICS_FILENAME = 'demographics.json';
const CURRENT_PARTICIPANT_FILE = path.join(DATA_DIR, 'current-participant.json');

export function listParticipants(): Participant[] {
  const participants: Participant[] = [];

  const dirEntries = fs.readdirSync(DATA_DIR, { withFileTypes: true });
  for (const entry of dirEntries) {
    if (entry.isDirectory()) {
      const demographicsFile = path.join(DATA_DIR, entry.name, DEMOGRAPHICS_FILENAME);

      if (fs.existsSync(demographicsFile)) {
        const participantData = JSON.parse(fs.readFileSync(demographicsFile, 'utf-8'));
        participants.push(participantData);
      }
    }
  }

  return participants;
}

export function createOrUpdateParticipant(participant: Participant): Participant {
  validateParticipant(participant);

  const pid = participant.id;
  const participantDir = path.join(DATA_DIR, pid);
  const participantDemographics = path.join(participantDir, DEMOGRAPHICS_FILENAME);

  if (!fs.existsSync(participantDir)) {
    fs.mkdirSync(participantDir, { recursive: true });
  }

  fs.writeFileSync(participantDemographics, JSON.stringify(participant, null, 2));
  return participant;
}

export function getCurrentParticipant(): Participant | null {
  if (!fs.existsSync(CURRENT_PARTICIPANT_FILE)) {
    return null;
  }
  const currentParticipantId = fs.readFileSync(CURRENT_PARTICIPANT_FILE, 'utf-8');
  const participant = getParticipantDemographics(currentParticipantId);
  if (participant === null) {
    console.error("Current participant's demographics not found.");
    return null;
  }

  return participant;
}

export function startExperiment(participantId: string): boolean {
  const currentParticipant = getCurrentParticipant();
  if (currentParticipant !== null) {
    console.error("Experiment already started.");
    return false;
  }

  const participantDemographics = getParticipantDemographics(participantId);
  if (participantDemographics === null) {
    console.error("Participant demographics not found.");
    return false;
  }

  fs.writeFileSync(CURRENT_PARTICIPANT_FILE, participantId);
  console.log(`Participant ${participantId} is now set as current.`);
  return true;
}

export function stopExperiment(): boolean {
  if (!fs.existsSync(CURRENT_PARTICIPANT_FILE)) {
    console.error("No current participant to end.");
    return false;
  }

  fs.unlinkSync(CURRENT_PARTICIPANT_FILE);
  console.log("Current participant has been ended.");
  return true;
}

export function getParticipantDemographics(participantId: string): Participant | null {
  const participantDir = path.join(DATA_DIR, participantId);
  const demographicsFile = path.join(participantDir, DEMOGRAPHICS_FILENAME);

  if (fs.existsSync(demographicsFile)) {
    const participantData = JSON.parse(fs.readFileSync(demographicsFile, 'utf-8'));
    validateParticipant(participantData);
    if (participantData.id !== participantId) {
      console.error(`Participant ID mismatch: expected ${participantId}, got ${participantData.id}`);
      throw new Error(`Participant ID mismatch: expected ${participantId}, got ${participantData.id}`);
    }
    return participantData as Participant;
  }

  return null;
}

function validateParticipant(participant: Participant): void {
  const validationResult = participantSchema.safeParse(participant);
  if (!validationResult.success) {
    console.error("Invalid participant data:", validationResult.error);
    throw new Error("Invalid participant data");
  }
}
