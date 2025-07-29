import { Method, methods } from '@/models/method';
import { Participant, participantSchema } from '@/models/participant';
import fs from 'fs';
import path from 'path';
import { DATA_DIR } from './database';

const USED_SENTENCES_FILENAME = 'used-sentences.json';
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
  for (const method of methods) {
    const methodDir = path.join(participantDir, method);
    if (!fs.existsSync(methodDir)) {
      fs.mkdirSync(methodDir, { recursive: true });
    }
  }

  fs.writeFileSync(participantDemographics, JSON.stringify(participant, null, 2));
  return participant;
}

export function getCurrentParticipant(): { participant: Participant, method: Method } | null {
  if (!fs.existsSync(CURRENT_PARTICIPANT_FILE)) {
    return null;
  }
  const { participantId: currentParticipantId, method } = JSON.parse(fs.readFileSync(CURRENT_PARTICIPANT_FILE, 'utf-8'));
  const participant = getParticipantDemographics(currentParticipantId);
  if (participant === null) {
    console.error("Current participant's demographics not found.");
    return null;
  }

  return { participant, method };
}

export function startExperiment(participantId: string, method: Method): boolean {
  const currentData = getCurrentParticipant();
  if (currentData !== null) {
    console.error("Experiment already started.");
    return false;
  }

  fs.writeFileSync(CURRENT_PARTICIPANT_FILE, JSON.stringify({ participantId, method }));
  console.log(`Participant ${participantId} and method ${method} are now set as current.`);
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

export function getParticipantUsedSentences(participantId: string): number[] {
  const participantDir = path.join(DATA_DIR, participantId);
  const sentencesFile = path.join(participantDir, USED_SENTENCES_FILENAME);

  if (fs.existsSync(sentencesFile)) {
    const sentencesData = JSON.parse(fs.readFileSync(sentencesFile, 'utf-8'));
    if (Array.isArray(sentencesData)) {
      return sentencesData;
    }
  }
  return [];
}

export function addParticipantUsedSentence(participantId: string, sentenceId: number): void {
  const participantDir = path.join(DATA_DIR, participantId);
  const sentencesFile = path.join(participantDir, USED_SENTENCES_FILENAME);

  const usedSentences = getParticipantUsedSentences(participantId);
  if (!usedSentences.includes(sentenceId)) {
    usedSentences.push(sentenceId);
  }

  fs.writeFileSync(sentencesFile, JSON.stringify(usedSentences, null, 2));
}

function validateParticipant(participant: Participant): void {
  const validationResult = participantSchema.safeParse(participant);
  if (!validationResult.success) {
    console.error("Invalid participant data:", validationResult.error);
    throw new Error("Invalid participant data");
  }
}
