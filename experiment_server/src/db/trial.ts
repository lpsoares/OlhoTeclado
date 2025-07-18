import { Method } from '@/models/method';
import fs from 'fs';
import path from "path";
import { DATA_DIR } from "./database";
import { addParticipantUsedSentence, getParticipantUsedSentences } from './participant';
import { getSessionDirname } from './session';

const TRIAL_FILE_HEADER = "timestamp,type,data\n";

export function listTrials(participantId: string, method: Method, session: number): number[] {
  const sessionDir = path.join(DATA_DIR, participantId, method, getSessionDirname(session));
  if (!fs.existsSync(sessionDir)) {
    return [];
  }
  return fs.readdirSync(sessionDir)
    .filter(file => file.startsWith('trial-'))
    .map(trialFile => parseInt(trialFile.split("-")[1]))
    .sort();
}

export function startTrial(participantId: string, method: Method, session: number, timestamp: string): { trial: number; sentence: string } {
  const sessionDir = path.join(DATA_DIR, participantId, method, getSessionDirname(session));
  if (!fs.existsSync(sessionDir)) {
    throw new Error(`Session directory does not exist for participant ${participantId} and session ${session}`);
  }

  const trialNumber = Math.max(0, ...listTrials(participantId, method, session)) + 1;
  const trialFile = path.join(sessionDir, getTrialFilename(trialNumber));

  if (!fs.existsSync(trialFile)) {
    fs.writeFileSync(trialFile, TRIAL_FILE_HEADER, 'utf8');
  }

  const sentence = getRandomSentence(participantId);
  if (!sentence) {
    throw new Error(`No more unused sentences available for participant ${participantId}`);
  }
  addTrialData(participantId, method, session, trialNumber, timestamp, 'TRIAL_START', sentence);

  return { trial: trialNumber, sentence };
}

export function addTrialData(participantId: string, method: Method, session: number, trial: number, timestamp: string, type: string, data: string): void {
  const trialFile = path.join(DATA_DIR, participantId, method, getSessionDirname(session), getTrialFilename(trial));
  if (!fs.existsSync(trialFile)) {
    throw new Error(`Trial file does not exist for participant ${participantId}, session ${session}, trial ${trial}`);
  }
  const line = `${timestamp},${type},${data}\n`;
  fs.appendFileSync(trialFile, line, 'utf8');
}

export function getTrialData(participantId: string, method: Method, session: number, trial: number): string {
  const trialFile = path.join(DATA_DIR, participantId, method, getSessionDirname(session), getTrialFilename(trial));
  if (!fs.existsSync(trialFile)) {
    throw new Error(`Trial file does not exist for participant ${participantId}, session ${session}, trial ${trial}`);
  }
  return fs.readFileSync(trialFile, 'utf8');
}

function getTrialFilename(trialNumber: number): string {
  return `trial-${trialNumber.toString().padStart(3, '0')}.csv`;
}

function getRandomSentence(participantId: string): string | null {
  const allSentences = getSentences();
  const usedSentences = getParticipantUsedSentences(participantId);
  if (allSentences.length === usedSentences.length) {
    return null;
  }

  const unusedSentences = allSentences
    .map((_, index) => index)
    .filter(index => !usedSentences.includes(index));
  const randomIndex = Math.floor(Math.random() * unusedSentences.length);
  const sentenceId = unusedSentences[randomIndex];
  addParticipantUsedSentence(participantId, sentenceId);
  return allSentences[sentenceId];
}

function getSentences(): string[] {
  // This information is in data/sentences.txt
  const sentencesFile = path.join(DATA_DIR, 'sentences.txt');
  if (!fs.existsSync(sentencesFile)) {
    throw new Error("Sentences file does not exist.");
  }
  const sentencesData = fs.readFileSync(sentencesFile, 'utf-8');
  return sentencesData.split('\n').filter(sentence => sentence.trim() !== '');
}
